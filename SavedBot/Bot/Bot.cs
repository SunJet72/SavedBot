using Microsoft.Extensions.Logging;
using SavedBot.Chat.Add;
using SavedBot.Exceptions;
using SavedBot.Handlers;
using SavedBot.Model;
using SavedBot.Localization;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using SavedBot.Data;

namespace SavedBot.Bot
{
    public class Bot
    {
        #region Fields
        private const int searchLimit = 25;
        private const int cacheInlineTime = 30;
        private static readonly CultureInfo defaultCulture = new("en");

        private readonly Telegram.Bot.Types.User botInfo;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ReceiverOptions _receiverOptions;
        private readonly TelegramBotClient _client;

        private readonly ILogger _logger;
        private readonly IModelContext _modelContext;

        private readonly AddCommandHandler _addCommandHandler;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a bot with the default configuration: logs in Console, storing data in memory
        /// You need the Telegram bot token to be stored in User Secrets with the key TG_BOT_KEY
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TelegramBotTokenNotFoundException">Token was not found in User Secrets</exception>
        public static Bot BuildBot(string token, AppDbContext dbContext)
        {
            ILogger logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(Bot));
            IModelContext context = new ModelContext(dbContext);

            return new(token, context, logger);
        }

        private Bot(string token, IModelContext modelContext, ILogger logger)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _receiverOptions = new() { AllowedUpdates = [] };

            _client = new TelegramBotClient(new TelegramBotClientOptions(token));

            _logger = logger;
            _addCommandHandler = new AddCommandHandler(modelContext, logger);
            _modelContext = modelContext;

            botInfo = _client.GetMeAsync().Result;
        }
        #endregion

        #region Long Polling
        public async void StartPolling()
        {
            await _client.DeleteWebhookAsync();
            _client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingError,
                receiverOptions: _receiverOptions,
                cancellationToken: _cancellationTokenSource.Token
            );
            var me = await _client.GetMeAsync();
            _logger.LogInformation("Started {FirstName} with id {Id}", me.FirstName, me.Id);
        }
        #endregion

        #region Webhook
        public async void SetWebhook(string url)
        {
            await _client.SetWebhookAsync(url);
        }
        public Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
            => HandleUpdateAsync(_client, update, cancellationToken);

        #endregion

        #region Handlers
        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await (update.Type switch
                {
                    UpdateType.Message => HandleMessageAsync(update.Message!),
                    UpdateType.InlineQuery => HandleInlineQueryAsync(update.InlineQuery!),
                    _ => throw new NotImplementedException()
                });

            }
            catch (Exception ex)
            {
                _logger.LogError("Global Exception caught: {Message}", ex.ToString());
            }
        }
        private async Task HandleInlineQueryAsync(InlineQuery inlineQuery)
        {
            if (inlineQuery is null) return;
            TelegramUser? user = await _modelContext.GetUser(inlineQuery.From.Id);

            //Changing the culture for the ResourceManager to find the right localization
            BotStrings.Culture = new CultureInfo(user?.LanguageCode ?? defaultCulture.Name) ?? defaultCulture;

            _logger.LogInformation("User {Id} language code: {Lang}", user?.Id, user?.LanguageCode);

            if (user is null)
            {
                await _client.AnswerInlineQueryAsync(inlineQuery.Id, [
                new InlineQueryResultArticle("warning", BotStrings.Warning, new InputTextMessageContent(BotStrings.ErrorBotNotStarted + botInfo.Username))
                ]);
                return;
            }
            _logger.LogDebug("Searching by query: {Query}", inlineQuery.Query);


            IEnumerable<SavedItem> searchResult = await _modelContext.Search(user, inlineQuery.Query, searchLimit);
            InlineQueryResult[] inlineQueries = new InlineQueryResult[searchResult.Count()];

            int i = 0;
            foreach (SavedItem item in searchResult)
            {
                //TODO: Implement less searches
                try
                {
                    if (item as SavedFile is { } file)
                    {
                        inlineQueries[i++] = file.FileType switch
                        {
                            MessageType.Photo => new InlineQueryResultCachedPhoto(file.FileName, file.FileId),
                            MessageType.Sticker => new InlineQueryResultCachedSticker(file.FileName, file.FileId),
                            MessageType.Audio => new InlineQueryResultCachedAudio(file.FileName, file.FileId),
                            MessageType.Document => new InlineQueryResultCachedDocument(file.FileName, file.FileId, file.FileName),
                            MessageType.Video => new InlineQueryResultCachedVideo(file.FileName, file.FileId, file.FileName),
                            MessageType.VideoNote => new InlineQueryResultCachedVideo(file.FileName, file.FileId, file.FileName),
                            MessageType.Voice => new InlineQueryResultCachedVoice(file.FileName, file.FileId, file.FileName),
                            MessageType.Animation => new InlineQueryResultCachedGif(file.FileName, file.FileId),
                            _ => throw new NotImplementedException()
                        };
                    }
                    //TODO: SavedText
                    //else if (item as SavedText is { } text)
                    //    inlineQueries[i] = new InlineQueryResultArticle(searchResult[i], searchResult[i], new InputTextMessageContent(text));
                }
                catch (SavedMessageNotFoundException) { }
            }
            await _client.AnswerInlineQueryAsync(inlineQuery.Id, inlineQueries, cacheInlineTime);

        }
        private async Task HandleMessageAsync(Message message)
        {
            if (message is null) return;

            long chatId = message.Chat.Id;
            long userId = message.From?.Id ?? throw new NotImplementedException();
            string languageCode = message.From?.LanguageCode ?? defaultCulture.Name;
            BotStrings.Culture = new CultureInfo(languageCode);
            _logger.LogInformation("User {Id} language code: {Lang}", userId, message.From?.LanguageCode);
#pragma warning disable  // Dereference of a possibly null reference.
            //TODO: Save chatId only in private chat!
            _modelContext.AddUser(new Model.TelegramUser(userId, chatId, languageCode));

            

            
            //TODO: Validate all of the possible Telegram size restrictions
            await (message.Type switch
            {
                MessageType.Text => HandleTextReceivedAsync(chatId, message.Text),
                MessageType.Photo => HandleFileReceivedAsync(userId, chatId, message.Photo.Last().FileId, message.Type),

                MessageType.Video => HandleFileReceivedAsync(userId, chatId, message.Video.FileId, message.Type),
                MessageType.Animation => HandleFileReceivedAsync(userId, chatId, message.Animation.FileId, message.Type),
                MessageType.Audio => HandleFileReceivedAsync(userId, chatId, message.Audio.FileId, message.Type),
                MessageType.Document => HandleFileReceivedAsync(userId, chatId, message.Document.FileId, message.Type),
                MessageType.Sticker => HandleFileReceivedAsync(userId, chatId, message.Sticker.FileId, message.Type),
                MessageType.VideoNote => HandleFileReceivedAsync(userId, chatId, message.VideoNote.FileId, message.Type),
                MessageType.Voice => HandleFileReceivedAsync(userId, chatId, message.Voice.FileId, message.Type),
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                MessageType.Contact => HandleNotImplementedAsync(chatId),
                MessageType.Location => HandleNotImplementedAsync(chatId),
                MessageType.Poll => HandleNotImplementedAsync(chatId),
                MessageType.Venue => HandleNotImplementedAsync(chatId),
            });
        }
        private async Task HandleFileReceivedAsync(long userId, long chatId, string fileId, MessageType messageType)
        {
            try
            {
                if (fileId is not null)
                {
                    _addCommandHandler.Handle(new OngoingAddFileChat(userId, new SavedFile(string.Empty, fileId, messageType)));
                    await _client.SendTextMessageAsync(chatId, BotStrings.AddFileSaved);
                }
            }
            catch (NotFoundOngoingAddChatException ex)
            {
                await _client.SendTextMessageAsync(chatId, ex.Message);
            }
        }
        private async Task HandleTextReceivedAsync(long chatId, string text)
        {
            if (text is not null)
            {
                _logger.LogDebug($"Received message from {chatId}: {text}");
                if (text.StartsWith("/")) await HandleCommandAsync(chatId, text);
                else
                {
                    try
                    {
                        if (_addCommandHandler.IsNamed(chatId))
                        {
                            _addCommandHandler.Handle(new OngoingAddTextChat(chatId, text));
                            await _client.SendTextMessageAsync(chatId, BotStrings.AddTextSaved);
                        }
                        else
                        {
                            _addCommandHandler.Handle(new OngoingNameChat(chatId, text));
                            await _client.SendTextMessageAsync(chatId, BotStrings.AddSend);
                        }
                    }
                    catch (NotFoundOngoingAddChatException)
                    {
                        await _client.SendTextMessageAsync(chatId, BotStrings.ErrorCommandSeqBroken);
                    }
                }
            }
            else
            {
                _logger.LogInformation($"Message is of type Text, but Text property is null. ChatId: {chatId}");
            }
            return;
        }
        private async Task HandleCommandAsync(long chatId, string messageText)
        {
            (string, string?) GetCommand(string text)
            {
                int commandLength = text.IndexOf(" ");
                string command = commandLength != -1 ? text.Substring(0, commandLength) : text;
                return commandLength != -1 ? (command, text.Substring(commandLength + 1)) : (command, null);
            }

            (string command, string? argument) = GetCommand(messageText);
            _logger.LogDebug($"Received a command {command} from {chatId}");

            switch (command)
            {
                case "/start":
                    {
                        //await _client.SendTextMessageAsync(chatId, "Welcome to bot! You can start by typing /help");
                        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                        {
                             BotStrings.MenuButtons.Split(';').Select(s => new KeyboardButton(s))
                        })
                        { ResizeKeyboard = true };
                        await _client.SendTextMessageAsync(chatId, BotStrings.Start, replyMarkup: replyKeyboardMarkup);
                    }
                    break;
                case "/help":
                    {
                        await _client.SendTextMessageAsync(chatId, BotStrings.Help + "\n" +
                            BotStrings.HelpCommands.Replace(';', '\n'));
                    }
                    break;
                case "/add":
                    {
                        _addCommandHandler.Handle(new OngoingAddChat(chatId));
                        await _client.SendTextMessageAsync(chatId,
                            BotStrings.AddName);
                        return;
                    }
                    break;
                default:
                    {
                        await _client.SendTextMessageAsync(chatId,
                           BotStrings.ErrorUnknownCommand);
                    }
                    break;
            }
        }
        private async Task HandleNotImplementedAsync(long chatId)
        {
            await _client.SendTextMessageAsync(chatId, BotStrings.ErrorNotImplemented);
        }
        private Task HandlePollingError(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\nCode: {apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        #endregion
    }
}

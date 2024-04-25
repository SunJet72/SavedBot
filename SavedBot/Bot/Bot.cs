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
        public static Bot BuildBot(string token)
        {
            ILogger logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(Bot));
            IModelContext context = new MockModelContext(logger);

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
            if (inlineQuery is not { }) return;
            Model.User? user = _modelContext.GetUserById(inlineQuery.From.Id);

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

            string[] searchResult = _modelContext.Search(user.ChatId, inlineQuery.Query, searchLimit).ToArray();
            InlineQueryResult[] inlineQueries = new InlineQueryResult[searchResult.Length];

            for (int i = 0; i < searchResult.Length; i++)
            {
                //TODO: Implement less searches
                try
                {
                    if (_modelContext.GetFile(user.ChatId, searchResult[i]) is { } file)
                    {
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                        inlineQueries[i] = file.FileType switch
                        {
                            MessageType.Photo => new InlineQueryResultCachedPhoto(searchResult[i], file.Id),
                            MessageType.Sticker => new InlineQueryResultCachedSticker(searchResult[i], file.Id),
                            MessageType.Audio => new InlineQueryResultCachedAudio(searchResult[i], file.Id),
                            MessageType.Document => new InlineQueryResultCachedDocument(searchResult[i], file.Id, searchResult[i]),
                            MessageType.Video => new InlineQueryResultCachedVideo(searchResult[i], file.Id, searchResult[i]),
                            MessageType.VideoNote => new InlineQueryResultCachedVideo(searchResult[i], file.Id, searchResult[i]),
                            MessageType.Voice => new InlineQueryResultCachedVoice(searchResult[i], file.Id, searchResult[i]),
                            MessageType.Animation => new InlineQueryResultCachedGif(searchResult[i], file.Id)
                        };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                    }
                    else if (_modelContext.GetText(user.ChatId, searchResult[i]) is { } text)
                        inlineQueries[i] = new InlineQueryResultArticle(searchResult[i], searchResult[i], new InputTextMessageContent(text));
                }
                catch (SavedMessageNotFoundException) { }
            }
            await _client.AnswerInlineQueryAsync(inlineQuery.Id, inlineQueries, cacheInlineTime);

        }
        private async Task HandleMessageAsync(Message message)
        {
            if (message is not { }) return;

            long chatId = message.Chat.Id;
#pragma warning disable  // Dereference of a possibly null reference.
            _modelContext.AddUser(new Model.User(message.From.Id, chatId));

            BotStrings.Culture = new CultureInfo(message.From?.LanguageCode ?? defaultCulture.Name) ?? defaultCulture;
            _logger.LogInformation("User {Id} language code: {Lang}", message.From?.Id, message.From?.LanguageCode);


            //TODO: Validate all of the possible Telegram size restrictions
            await (message.Type switch
            {
                MessageType.Text => HandleTextReceivedAsync(chatId, message.Text),
                MessageType.Photo => HandleFileReceivedAsync(chatId, message.Photo.Last().FileId, message.Type),

                MessageType.Video => HandleFileReceivedAsync(chatId, message.Video.FileId, message.Type),
                MessageType.Animation => HandleFileReceivedAsync(chatId, message.Animation.FileId, message.Type),
                MessageType.Audio => HandleFileReceivedAsync(chatId, message.Audio.FileId, message.Type),
                MessageType.Document => HandleFileReceivedAsync(chatId, message.Document.FileId, message.Type),
                MessageType.Sticker => HandleFileReceivedAsync(chatId, message.Sticker.FileId, message.Type),
                MessageType.VideoNote => HandleFileReceivedAsync(chatId, message.VideoNote.FileId, message.Type),
                MessageType.Voice => HandleFileReceivedAsync(chatId, message.Voice.FileId, message.Type),
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                MessageType.Contact => HandleNotImplementedAsync(chatId),
                MessageType.Location => HandleNotImplementedAsync(chatId),
                MessageType.Poll => HandleNotImplementedAsync(chatId),
                MessageType.Venue => HandleNotImplementedAsync(chatId),
            });
        }
        private async Task HandleFileReceivedAsync(long chatId, string fileId, MessageType messageType)
        {
            try
            {
                if (fileId is not null)
                {
                    _addCommandHandler.Handle(new OngoingAddFileChat(chatId, new SavedFile(fileId, messageType)));
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
            if (text is { })
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
                case "/find":
                    {
                        if (argument is not null)
                        {
                            try
                            {
                                SavedFile file = _modelContext.GetFile(chatId, argument);
                                if (file.FileType == MessageType.Photo)
                                    await _client.SendPhotoAsync(chatId, InputFile.FromFileId(file.Id));
                                else
                                    await _client.SendDocumentAsync(chatId, InputFile.FromFileId(file.Id));
                            }
                            catch (SavedMessageNotFoundException)
                            {
                                try
                                {
                                    string text = _modelContext.GetText(chatId, argument);
                                    await _client.SendTextMessageAsync(chatId, text);
                                }
                                catch (SavedMessageNotFoundException ex)
                                {
                                    await _client.SendTextMessageAsync(chatId, ex.Message);
                                }
                            }
                        }
                        else
                        {
                            await _client.SendTextMessageAsync(chatId,
                                BotStrings.FindNoName);
                            return;
                        }
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

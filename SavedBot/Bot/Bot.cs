using Microsoft.Extensions.Logging;
using SavedBot.Chat.Add.File;
using SavedBot.Chat.Add.Text;
using SavedBot.Chat.Cancel;
using SavedBot.Chat.Edit;
using SavedBot.Chat.RemoveText;
using SavedBot.Data;
using SavedBot.Exceptions;
using SavedBot.Handlers;
using SavedBot.Localization;
using SavedBot.Model;
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
        #region Constants
        private const string RemoveFile = "removeFile";
        private const string RemoveText = "removeText";
        private const string SaveText = "saveText";
        private const string CancelFileEdit = "cancelFile";
        private const string CancelTextSave = "cancelTextSave";
        private const string CancelTextRemove = "cancelTextRemove";

        private const int searchLimit = 25;
        private const int cacheInlineTime = 30;
        private const int textHeaderLimit = 30;

        private static readonly CultureInfo defaultCulture = new("en");

        #endregion

        #region Fields

        private readonly Telegram.Bot.Types.User botInfo;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ReceiverOptions _receiverOptions;
        private readonly TelegramBotClient _client;

        private readonly ILogger _logger;
        private readonly IModelContext _modelContext;

        private readonly CommandHandler _commandHandler;
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
            _commandHandler = new CommandHandler(modelContext, logger);
            _modelContext = modelContext;

            botInfo = _client.GetMeAsync().Result;
        }
        #endregion

        #region Long Polling
        public async void StartPolling()
        {
            await _client.DeleteWebhookAsync();
            await _client.DropPendingUpdatesAsync();
            _client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingError,
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
                    UpdateType.CallbackQuery => HandleCallbackQueryAsync(update.CallbackQuery!),
                    _ => throw new NotImplementedException()
                });

            }
            catch (Exception ex)
            {
                _logger.LogError("Global Exception caught: {Message}", ex.ToString());
            }
        }

        private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            if (callbackQuery is null) return;
            TelegramUser? user = await _modelContext.GetUserAsync(callbackQuery.From.Id);
            if (user is null) return;
            BotStrings.Culture = new CultureInfo(user?.LanguageCode ?? defaultCulture.Name) ?? defaultCulture;

            switch (callbackQuery.Data) 
            {
                case RemoveFile:
                    {
                        await _commandHandler.HandleAsync(new RemoveFileOngoingChat(user.Id, callbackQuery.Message.MessageId));
                        await _client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Your saved media is successfully removed!");
                    }
                    break;
                case SaveText:
                    {
                        await _commandHandler.HandleAsync(new AddTextConfirmOngoingChat(user.Id));
                        await _client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, BotStrings.AddTextSaved);
                    }
                    break;
                case RemoveText:
                    {
                        await _commandHandler.HandleAsync(new RemoveTextConfirmOngoingChat(user.Id));
                        await _client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Your saved text is successfully removed!");
                    }
                    break;
                case CancelFileEdit:
                    {
                        await _commandHandler.HandleAsync(new CancelOngoingChat(user.Id));
                        await _client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Your media remains as it is");
                    }
                    break;
                case CancelTextSave:
                    {
                        await _commandHandler.HandleAsync(new CancelOngoingChat(user.Id));
                        await _client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Text was not saved");
                    }
                    break;
                case CancelTextRemove:
                    {
                        await _commandHandler.HandleAsync(new CancelOngoingChat(user.Id));
                        await _client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "Text was not removed");
                    }
                    break;
                default: return;
            }
        }

        private async Task HandleInlineQueryAsync(InlineQuery inlineQuery)
        {
            if (inlineQuery is null) return;
            TelegramUser? user = await _modelContext.GetUserAsync(inlineQuery.From.Id);

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


            IEnumerable<SavedItem> searchResult = await _modelContext.SearchAsync(user, inlineQuery.Query, searchLimit);
            InlineQueryResult[] inlineQueries = new InlineQueryResult[searchResult.Count()];

            int i = 0;
            foreach (SavedItem item in searchResult)
            {
                //TODO: Implement less searches
                try
                {
                    if (item as SavedFile is { } file)
                    {
                        inlineQueries[i] = file.FileType switch
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
                    //TODO: Finish text search
                    else if (item as SavedText is { } text)
                    {
                        string header = text.Text.Substring(0, Math.Min(textHeaderLimit, text.Text.Length));
                        inlineQueries[i] = new InlineQueryResultArticle(header, header, new InputTextMessageContent(text.Text));
                    }
                }
                catch (SavedMessageNotFoundException) { }
                i++;
            }
            await _client.AnswerInlineQueryAsync(inlineQuery.Id, inlineQueries, cacheInlineTime);
        }
        private async Task HandleMessageAsync(Message message)
        {
            if (message is null || message.Chat.Type != ChatType.Private) return;

            long chatId = message.Chat.Id;
            long userId = message.From?.Id ?? throw new NotImplementedException();
            string languageCode = message.From?.LanguageCode ?? defaultCulture.Name;
            BotStrings.Culture = new CultureInfo(languageCode);
            _logger.LogInformation("User {Id} language code: {Lang}", userId, message.From?.LanguageCode);
#pragma warning disable  // Dereference of a possibly null reference.

            _modelContext.AddUserAsync(new Model.TelegramUser(userId, chatId, languageCode));

            bool viaBot = message.ViaBot?.Id == botInfo.Id;

            //TODO: Validate all of the possible Telegram size restrictions
            await (message.Type switch
            {
                MessageType.Text => HandleTextReceivedAsync(userId, chatId, message.Text),
                MessageType.Photo => HandleFileReceivedAsync(userId, chatId, message.Photo.Last().FileId, message.Photo.Last().FileUniqueId, message.Type, viaBot),
                MessageType.Video => HandleFileReceivedAsync(userId, chatId, message.Video.FileId, message.Video.FileUniqueId, message.Type, viaBot),
                MessageType.Animation => HandleFileReceivedAsync(userId, chatId, message.Animation.FileId, message.Animation.FileUniqueId, message.Type, viaBot),
                MessageType.Audio => HandleFileReceivedAsync(userId, chatId, message.Audio.FileId, message.Audio.FileUniqueId, message.Type, viaBot),
                MessageType.Document => HandleFileReceivedAsync(userId, chatId, message.Document.FileId, message.Document.FileUniqueId, message.Type, viaBot),
                MessageType.Sticker => HandleFileReceivedAsync(userId, chatId, message.Sticker.FileId, message.Sticker.FileUniqueId, message.Type, viaBot),
                MessageType.VideoNote => HandleFileReceivedAsync(userId, chatId, message.VideoNote.FileId, message.VideoNote.FileUniqueId, message.Type, viaBot),
                MessageType.Voice => HandleFileReceivedAsync(userId, chatId, message.Voice.FileId, message.Voice.FileUniqueId, message.Type, viaBot),
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                MessageType.Contact => HandleNotImplementedAsync(chatId),
                MessageType.Location => HandleNotImplementedAsync(chatId),
                MessageType.Poll => HandleNotImplementedAsync(chatId),
                MessageType.Venue => HandleNotImplementedAsync(chatId),
            });
        }
        private async Task HandleFileReceivedAsync(long userId, long chatId, string fileId, string fileUniqueId, MessageType messageType, bool viaBot)
        {
            try
            {
                if (fileId is not null && fileUniqueId is not null)
                {
                    if (viaBot && await _modelContext.ItemExistsAsync(new SavedFile() {
                        FileUniqueId = fileUniqueId, User = new TelegramUser(userId)}))
                    {
                        var inlineMarkup = new InlineKeyboardMarkup()
                            .AddButton("Remove", RemoveFile)
                            .AddNewRow()
                            .AddButton("Cancel", CancelFileEdit);

                        int messageId = (await _client.SendTextMessageAsync(chatId, "Do you want to remove this media from your collection, or just rename it?", 
                            replyMarkup: inlineMarkup)).MessageId;
                        _commandHandler.HandleAsync(new EditFileOngoingChat(userId, fileId, fileUniqueId, messageId));
                    }
                    else
                    {
                        _commandHandler.HandleAsync(new AddFileOngoingChat(userId, fileId, fileUniqueId, messageType));
                        await _client.SendTextMessageAsync(chatId, BotStrings.AddName);
                    }
                }
            }
            catch (OngoingChatNotFoundException)
            {
                await _client.SendTextMessageAsync(chatId, BotStrings.ErrorCommandSeqBroken);
            }
        }
        private async Task HandleTextReceivedAsync(long userId, long chatId, string text)
        {
            if (text is not null)
            {
                _logger.LogDebug($"Received message from {chatId}: {text}");
                if (text.StartsWith("/")) await HandleCommandAsync(chatId, text);
                else
                {
                    try
                    {
                        int? messageId = _commandHandler.IsRenamingInProgress(userId);
                        if (messageId is not null)
                        {
                            await _commandHandler.HandleAsync(new RenameFileOngoingChat(userId, text, messageId.Value));
                            await _client.EditMessageTextAsync(chatId, messageId.Value, "Your saved media is successfully renamed!");
                        }
                        else if (_commandHandler.IsNamingInProgress(userId))
                        {
                            _commandHandler.HandleAsync(new AddFileNameOngoingChat(chatId, text));
                            await _client.SendTextMessageAsync(chatId, BotStrings.AddFileSaved);
                        }
                        else if(await _modelContext.ItemExistsAsync(new SavedText(text, new TelegramUser(userId))))
                        {

                            var inlineMarkup = new InlineKeyboardMarkup()
                            .AddButton("Remove", RemoveText)
                            .AddNewRow()
                            .AddButton("Cancel", CancelTextRemove);

                            int textRemoveMessageId = (await _client.SendTextMessageAsync(chatId, "Do you want to remove this text from your collection?",
                                replyMarkup: inlineMarkup)).MessageId;
                            _commandHandler.HandleAsync(new RemoveTextOngoingChat(chatId, text, textRemoveMessageId));
                        }
                        else
                        {
                            var inlineMarkup = new InlineKeyboardMarkup()
                            .AddButton("Save", SaveText)
                            .AddNewRow()
                            .AddButton("Cancel", CancelTextSave);

                            int textSaveMessageId = (await _client.SendTextMessageAsync(chatId, "Do you want to save this text?",
                                replyMarkup: inlineMarkup)).MessageId;
                            _commandHandler.HandleAsync(new AddTextOngoingChat(chatId, text, textSaveMessageId));
                        }
                    }
                    catch (OngoingChatNotFoundException)
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
                        await _client.SendTextMessageAsync(chatId, BotStrings.Start);
                    }
                    break;
                case "/help":
                    {
                        await _client.SendTextMessageAsync(chatId, BotStrings.Help + "\n" +
                            BotStrings.HelpCommands.Replace(';', '\n'));
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

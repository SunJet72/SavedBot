using SavedBot.Chat;
using SavedBot.Exceptions;
using SavedBot.Handlers;
using SavedBot.Loggers;
using SavedBot.Model;
using System.Diagnostics.Metrics;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace SavedBot
{
    internal class Bot
    {
        private const int searchLimit = 5;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ReceiverOptions _receiverOptions;
        private readonly TelegramBotClient _client;
        private readonly ILogger _logger;
        private readonly IModelContext _modelContext;

        private readonly AddCommandHandler _addCommandHandler;
        public Bot(string token, IModelContext modelContext, ILogger logger)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _receiverOptions = new() { AllowedUpdates = Array.Empty<UpdateType>() };

            _client = new TelegramBotClient(new TelegramBotClientOptions(token));

            _logger = logger;
            _addCommandHandler = new AddCommandHandler(modelContext, logger);
            _modelContext = modelContext;
        }
        public async void Start()
        {
            _client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingError,
                receiverOptions: _receiverOptions,
                cancellationToken: _cancellationTokenSource.Token
            );
            var me = await _client.GetMeAsync();
            _logger.Log($"Started {me.FirstName} with id {me.Id}");
        }
        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            try
            {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                await (update.Type switch
                {
                    UpdateType.Message => HandleMessageAsync(update.Message),
                    UpdateType.InlineQuery => HandleInlineQueryAsync(update.InlineQuery)
                });
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
#pragma warning restore CS8604 // Possible null reference argument.
            }
            catch (Exception ex)
            {
                _logger.Log("Global Exception caught: " + ex.ToString());
            }
        }
        private async Task HandleInlineQueryAsync(InlineQuery inlineQuery)
        {
            if (inlineQuery is not { }) return;
            Model.TelegramUser? user = _modelContext.GetUserById(inlineQuery.From.Id);

            if (user is null)
            {
                await _client.AnswerInlineQueryAsync(inlineQuery.Id, new InlineQueryResult[] {
                new InlineQueryResultArticle("warning", "Warning", new InputTextMessageContent("You must activate the bot before using it. Just type /start to me in pm :)"))
                });
                return;
            }
            _logger.Log($"Searching by query: {inlineQuery.Query}");


            IQueryable<SavedItem> searchResultQuery = await _modelContext.Search(user, inlineQuery.Query, searchLimit);
            string[] searchResult = searchResultQuery.Select(item => item.ToString()).ToArray();
            InlineQueryResult[] inlineQueries = new InlineQueryResult[searchResult.Length];

            for(int i = 0; i < searchResult.Length; i++)
            {
                //TODO: Implement less searches
                try
                {
                    if (_modelContext.FindFile(user.ChatId, searchResult[i]) is { } file)
                    {
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                        inlineQueries[i] = (file.FileType switch
                        {
                            MessageType.Photo => new InlineQueryResultCachedPhoto(searchResult[i], file.Id),
                            MessageType.Sticker => new InlineQueryResultCachedSticker(searchResult[i], file.Id),
                            MessageType.Audio => new InlineQueryResultCachedAudio(searchResult[i], file.Id),
                            MessageType.Document => new InlineQueryResultCachedDocument(searchResult[i], file.Id, searchResult[i]),
                            MessageType.Video => new InlineQueryResultCachedVideo(searchResult[i], file.Id, searchResult[i]),
                            MessageType.VideoNote => new InlineQueryResultCachedVideo(searchResult[i], file.Id, searchResult[i]),
                            MessageType.Voice => new InlineQueryResultCachedVoice(searchResult[i], file.Id, searchResult[i]),
                            MessageType.Animation => new InlineQueryResultCachedGif(searchResult[i], file.Id)
                        });
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                    }
                    else if (_modelContext.FindText(user.ChatId, searchResult[i]) is { } text)
                        inlineQueries[i] = new InlineQueryResultArticle(searchResult[i], searchResult[i], new InputTextMessageContent(text));
                }
                catch (SavedMessageNotFoundException) { }
            }
            await _client.AnswerInlineQueryAsync(inlineQuery.Id, inlineQueries);

        }
        private async Task HandleMessageAsync(Message message)
        {
            if (message is not { } ) return;

            long chatId = message.Chat.Id;
#pragma warning disable  // Dereference of a possibly null reference.
            _modelContext.AddUser(new Model.TelegramUser(message.From.Id, chatId));

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
            }) ;
        }
        private async Task HandleCommandAsync(long chatId, string messageText)
        {
            (string, string?) GetCommand(string text)
            {
                int commandLength = text.IndexOf(" ");
                string command = (commandLength != -1) ? text.Substring(0, commandLength) : text;
                return (commandLength != -1) ? (command, text.Substring(commandLength + 1)) : (command, null);
            }

            (string command, string? argument) = GetCommand(messageText);
            _logger.Log($"Received a command {command} from {chatId}");

            switch (command)
            {
                case "/start":
                    {
                        await _client.SendTextMessageAsync(chatId,
                            "Welcome to bot! You can start by typing /help");
                    }
                    break;
                case "/help":
                    {
                        await _client.SendTextMessageAsync(chatId,
                            "Here is the list of my commands:\n\n/start\n/help\n/add {name}\n/find {name}");
                    }
                    break;
                case "/add":
                    {
                        if (argument is not null) 
                        { 
                            _addCommandHandler.Handle(new OngoingAddChat(chatId, argument));
                            await _client.SendTextMessageAsync(chatId,
                                "Great, now send the message that you want to save. It can be anything: from text, photo or GIF to location or a contact");
                        }
                        else
                        {
                            await _client.SendTextMessageAsync(chatId,
                                "You must pass a name to /add command");
                            return;
                        }
                    }
                    break;
                case "/find":
                    {
                        if (argument is not null)
                        {
                            try
                            {
                                SavedFile file = _modelContext.FindFile(chatId, argument);
                                if(file.FileType == MessageType.Photo)
                                    await _client.SendPhotoAsync(chatId, InputFile.FromFileId(file.Id));                            
                                else
                                    await _client.SendDocumentAsync(chatId, InputFile.FromFileId(file.Id));
                            }
                            catch (SavedMessageNotFoundException)
                            {
                                try
                                {
                                    string text = _modelContext.FindText(chatId, argument);
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
                                "You must pass a name to /find command");
                            return;
                        }
                    }
                    break;
                default:
                    {
                        await _client.SendTextMessageAsync(chatId,
                            "Unknown command");
                    }
                    break;
            }
        }
        private async Task HandleFileReceivedAsync(long chatId, string fileId, MessageType messageType)
        {
            try
            {
                if (fileId is not null)
                {
                    _addCommandHandler.Handle(new OngoingAddFileChat(chatId, new SavedFile(fileId, messageType)));
                    await _client.SendTextMessageAsync(chatId, "Successfully saved your file!");
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
                _logger.Log($"Received message from {chatId}: {text}");
                if (text.StartsWith("/")) await HandleCommandAsync(chatId, text);
                else
                {
                    try
                    {
                        _addCommandHandler.Handle(new OngoingAddTextChat(chatId, text));
                        await _client.SendTextMessageAsync(chatId, "Successfully saved your text!");
                    }
                    catch (NotFoundOngoingAddChatException)
                    {
                        await _client.SendTextMessageAsync(chatId, "I cannot recognise what you're trying to say to me. Maybe try one of the commands from /help list?");
                    }
                }
            }
            else
            {
                _logger.Log($"Message is of type Text, but Text property is null. ChatId: {chatId}");
            }
            return;
        }
        private async Task HandleNotImplementedAsync(long chatId)
        {
            await _client.SendTextMessageAsync(chatId, "You got me, I don't support this kind of messages yet :(");
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
       
    }
}

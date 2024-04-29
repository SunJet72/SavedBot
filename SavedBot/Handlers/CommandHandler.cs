using Microsoft.Extensions.Logging;
using SavedBot.Chat;
using SavedBot.Chat.Add.File;
using SavedBot.Chat.Add.Text;
using SavedBot.Chat.Edit;
using SavedBot.Chat.RemoveText;
using SavedBot.Exceptions;
using SavedBot.Model;

namespace SavedBot.Handlers
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    internal class CommandHandler(IModelContext _modelContext, ILogger _logger)
    {
        private readonly HashSet<OngoingChat> _chats = [];

        #region Add File

        public async Task HandleAsync(AddFileOngoingChat fileChat)
        { 
            _chats.Remove(fileChat);
            _chats.Add(fileChat);
        }
        public async Task HandleAsync(AddFileNameOngoingChat nameChat)
        {
            if (_chats.FirstOrDefault((c) => c.UserId == nameChat.UserId) is AddFileOngoingChat addFileChat)
            {
                _chats.Remove(addFileChat);
                await _modelContext.AddItemAsync(
                    new SavedFile(nameChat.Name, addFileChat.FileId, addFileChat.FileType, new TelegramUser(addFileChat.UserId)));
            }
            else throw new OngoingChatNotFoundException();
        }

        #endregion

        #region Add Text
        public async Task HandleAsync(AddTextOngoingChat textChat)
        {
            _chats.Remove(textChat);
            _chats.Add(textChat);
        }
        public async Task HandleAsync(AddTextConfirmOngoingChat textConfirmChat)
        {
            if (_chats.FirstOrDefault((c) => c.UserId == textConfirmChat.UserId) is AddTextOngoingChat textChat)
            {
                _chats.Remove(textChat);
                await _modelContext.AddItemAsync(new SavedText(textChat.Text, new TelegramUser(textChat.UserId)));
            }
            else throw new OngoingChatNotFoundException();
        }
        #endregion

        #region Edit File
        public async Task HandleAsync(EditFileOngoingChat editFileChat)
        {
            _chats.Remove(editFileChat);
            _chats.Add(editFileChat);
        }
        public async Task HandleAsync(RemoveFileOngoingChat removeFileChat)
        {
            if (_chats.FirstOrDefault((c) => c.UserId == removeFileChat.UserId) is EditFileOngoingChat editFileChat)
            {
                _chats.Remove(editFileChat);
                await _modelContext.RemoveItemAsync(new SavedFile() { FileId = editFileChat.FileId, User = new TelegramUser(editFileChat.UserId) });
            }
            else throw new OngoingChatNotFoundException();
        }
        public async Task HandleAsync(RenameFileOngoingChat renameFileChat)
        {
            if (_chats.FirstOrDefault((c) => c.UserId == renameFileChat.UserId) is EditFileOngoingChat editFileChat)
            {
                _chats.Remove(editFileChat);
                await _modelContext.RemoveItemAsync(new SavedFile() { FileId = editFileChat.FileId, User = new TelegramUser(editFileChat.UserId) });
            }
            else throw new OngoingChatNotFoundException();
        }

        #endregion

        #region Remove Text
        public async Task HandleAsync(RemoveTextOngoingChat removeTextChat)
        {
            _chats.Remove(removeTextChat);
            _chats.Add(removeTextChat);
        }
        public async Task HandleAsync(RemoveTextConfirmOngoingChat removeTextConfirmChat)
        {
            if (_chats.FirstOrDefault((c) => c.UserId == removeTextConfirmChat.UserId) is RemoveTextOngoingChat removeTextChat)
            {
                _chats.Remove(removeTextChat);
                await _modelContext.RemoveItemAsync(new SavedText(removeTextChat.Text, new TelegramUser(removeTextChat.UserId)));
            }
            else throw new OngoingChatNotFoundException();
        }

        #endregion

        public bool IsNamed(long chatId)
        {
            bool res = _chats.First((c) => c.UserId == chatId) switch
            {
                AddFileOngoingChat or AddTextOngoingChat => true,
                _ => false
            };
            return res;
        }
    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

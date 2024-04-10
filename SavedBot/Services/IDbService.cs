using SavedBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace SavedBot.Services
{
    public interface IDbService
    {
        Task CreateNewUser(User telegramUser);

        Task<TelegramUser> GetUserAsync(long userId);

        Task<List<MediaFile>> GetMediaFilesAsync(long userId, string fileName);

        Task<MediaFile> UploadMedia(Message message, string fileName);
    }
}

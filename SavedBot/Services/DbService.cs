using Microsoft.EntityFrameworkCore;
using SavedBot.Data;
using SavedBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SavedBot.Services
{
    public class DbService : IDbService
    {
        private readonly AppDbContext _appDbContext;
        private readonly TelegramBotClient _botClient;

        public DbService(AppDbContext appDbContext, TelegramBotClient botClient)
        {
            _appDbContext = appDbContext;
            _botClient = botClient;
        }
        public async Task<TelegramUser> GetUserAsync(long userId)
        {
            return await _appDbContext.TelegramUsers.FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task CreateNewUser(User telegramUser)
        {
            var user = new TelegramUser
            {
                Id = telegramUser.Id,
                FirstName = telegramUser.FirstName,
                LastName = telegramUser.LastName,
                Username = telegramUser.Username
            };

            await _appDbContext.TelegramUsers.AddAsync(user);
        }
        public async Task<MediaFile> UploadMedia(Message message, string fileName)
        {
            if (message.Document != null)
            {
                var mediaFile = new MediaFile
                {
                    UserId = message.From.Id,
                    FileName = fileName,
                    FileType = message.Document.MimeType,
                    FileId = message.Document.FileId
                };
                await _appDbContext.MediaFiles.AddAsync(mediaFile);
                await _appDbContext.SaveChangesAsync();
                return mediaFile;
            }
            else if (message.Photo != null)
            {
                var mediaFile = new MediaFile
                {
                    UserId = message.From.Id,
                    FileName = fileName,
                    FileType = "image/jpeg",
                    FileId = message.Photo.LastOrDefault().FileId
                };

                await _appDbContext.MediaFiles.AddAsync(mediaFile);
                await _appDbContext.SaveChangesAsync();
                return mediaFile;
            }
            else if (message.Animation != null)
            {
                var mediaFile = new MediaFile
                {
                    UserId = message.From.Id,
                    FileName = fileName,
                    FileType = "image/gif",
                    FileId = message.Animation.FileId
                };

                await _appDbContext.MediaFiles.AddAsync(mediaFile);
                await _appDbContext.SaveChangesAsync();
                return mediaFile;
            }
            return null;
        }
        public async Task<List<MediaFile>> GetMediaFilesAsync(long userId, string fileName)
        {
            return await _appDbContext.MediaFiles
                .Where(mf => mf.UserId == userId && mf.FileName.Contains(fileName))
                .ToListAsync();
        }
    }
}

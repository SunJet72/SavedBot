using SavedBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot.Types;
using SavedBot.Data;
using SavedBot.Model;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore;

namespace SavedBot.Model
{
    public class ModelContext(AppDbContext telegramContext) : IModelContext
    {
        public readonly AppDbContext _dbContext = telegramContext;

        public async Task AddItemAsync(SavedItem item)
        {
            TelegramUser existingUser = await _dbContext.TelegramUsers.FindAsync(item.User.Id);
            if (existingUser != null)
            {
                item.User = existingUser;
            }
            else
            {
                _dbContext.TelegramUsers.Add(item.User);
            }
            if (item is SavedFile file)
            {
                await _dbContext.SavedFiles.AddAsync(file);
            }
            else if (item is SavedText text)
            {
                await _dbContext.SavedTexts.AddAsync(text);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(SavedItem item)
        {
            if (item is SavedFile file)
            {
                SavedFile? fileToRemove = await _dbContext.SavedFiles.FirstOrDefaultAsync(x => x.FileId == file.FileId);

#pragma warning disable CS8604 // Possible null reference argument.
                _dbContext.SavedFiles.Attach(fileToRemove);
                _dbContext.SavedFiles.Remove(fileToRemove);
#pragma warning restore CS8604 // Possible null reference argument.

            }
            else if (item is SavedText text)
            {
                SavedText? textToRemove = await _dbContext.SavedTexts.FirstOrDefaultAsync(x => x.Text == text.Text);
#pragma warning disable CS8604 // Possible null reference argument.
                _dbContext.SavedTexts.Attach(textToRemove);
                _dbContext.SavedTexts.Remove(textToRemove);
#pragma warning restore CS8604 // Possible null reference argument.
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RenameFileAsync(SavedFile file)
        {
                SavedFile? fileToRename = await _dbContext.SavedFiles.FirstOrDefaultAsync(x => x.FileId == file.FileId);

                SavedFile? updatedFile = new SavedFile
                {
                    FileId = fileToRename.FileId,
                    FileType = fileToRename.FileType,
                    FileName = file.FileName
                };
                _dbContext.Entry(fileToRename).CurrentValues.SetValues(updatedFile);

                _dbContext.Entry(fileToRename).State = EntityState.Modified;

                await _dbContext.SaveChangesAsync();
        }

        public async Task ChangeLanguageAsync(TelegramUser user)
        {
            TelegramUser? userToChange = await _dbContext.TelegramUsers.FirstOrDefaultAsync(u => u.Id == user.Id);

            userToChange.LanguageCode = user.LanguageCode;


            _dbContext.Entry(userToChange).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();

        }
        public async Task<bool> ItemExistsAsync(SavedItem item)
        {
            if (item is SavedFile file)
            {
                SavedFile? fileToCheck = await _dbContext.SavedFiles.FirstOrDefaultAsync(f => f.FileId == file.FileId);

                return fileToCheck != null;
            }
            else if (item is SavedText text)
            {
                SavedText? textToCheck = await _dbContext.SavedTexts.FirstOrDefaultAsync(t => t.Text.Equals(text.Text));

                return textToCheck != null;
            }
            else
            {
                throw new ArgumentException("The provided item is neither SavedFile nor SavedText");
            }
        }

        public async Task<bool> UserExistsAsync(long userId)
        {
            TelegramUser userCheck = await _dbContext.TelegramUsers.FirstOrDefaultAsync(x => x.Id == userId);
            if (userCheck != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task AddUserAsync(TelegramUser user)
        {
            TelegramUser existingUser = await _dbContext.TelegramUsers.FindAsync(user.Id);
            if (existingUser == null)
            {
                await _dbContext.TelegramUsers.AddAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                _dbContext.TelegramUsers.Attach(user);
                _dbContext.Entry(user).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<TelegramUser?> GetUserAsync(long userId)
        {
            return await _dbContext.TelegramUsers.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IEnumerable<SavedItem>> SearchAsync(TelegramUser user, string partial, int limit)
        {
            List<SavedItem> savedFiles = await _dbContext.SavedFiles
                .Where(f => f.User.Id == user.Id)
                .Where(f => f.FileName.Contains(partial))
                .OfType<SavedItem>()
                .Take(limit)
                .ToListAsync();

            List<SavedItem> savedTexts = await _dbContext.SavedTexts
                .Where(t => t.User.Id == user.Id)
                .Where(t => t.Text.Contains(partial))
                .OfType<SavedItem>()
                .Take(limit)
                .ToListAsync();

            return savedFiles.Union(savedTexts);
        }

    }
}

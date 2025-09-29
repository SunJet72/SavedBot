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
            TelegramUser? existingUser = await _dbContext.TelegramUsers.FindAsync(item.User.Id);
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
                //await _dbContext.SavedFiles.AddAsync(file);
            }
            else if (item is SavedText text)
            {
                //await _dbContext.SavedTexts.AddAsync(text);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(SavedItem item)
        {
            if (item is SavedFile file)
            {
                //SavedFile? fileToRemove = await _dbContext.SavedFiles.FirstOrDefaultAsync(x => x.FileUniqueId.Equals(file.FileUniqueId));

#pragma warning disable CS8604 // Possible null reference argument.
                //_dbContext.SavedFiles.Attach(fileToRemove);
                //_dbContext.SavedFiles.Remove(fileToRemove);
#pragma warning restore CS8604 // Possible null reference argument.

            }
            else if (item is SavedText text)
            {
                //SavedText? textToRemove = await _dbContext.SavedTexts.FirstOrDefaultAsync(x => x.Text.Equals(text.Text));
#pragma warning disable CS8604 // Possible null reference argument.
                //_dbContext.SavedTexts.Attach(textToRemove);
                //_dbContext.SavedTexts.Remove(textToRemove);
#pragma warning restore CS8604 // Possible null reference argument.
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RenameFileAsync(SavedFile file)
        {
            SavedFile? fileToRename = null;  //await _dbContext.SavedFiles.FirstOrDefaultAsync(x => x.FileUniqueId.Equals(file.FileUniqueId));
            if (fileToRename is null) throw new SavedFileNotFoundException(file.FileUniqueId);

            SavedFile? updatedFile = new SavedFile() { 
                Id = fileToRename.Id,
                FileName = file.FileName,
                FileId = fileToRename.FileId,
                FileUniqueId = fileToRename.FileUniqueId,
                FileType = fileToRename.FileType
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
                SavedFile? fileToCheck = null;//await _dbContext.SavedFiles.FirstOrDefaultAsync(f => f.FileUniqueId.Equals(file.FileUniqueId) && f.User.Id == file.User.Id);

                return fileToCheck != null;
            }
            else if (item is SavedText text)
            {
                SavedText? textToCheck = null;//await _dbContext.SavedTexts.FirstOrDefaultAsync(t => t.Text.Equals(text.Text) && t.User.Id == text.User.Id);

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
            //list<saveditem> savedfiles = await _dbcontext.savedfiles
            //    .where(f => f.user.id == user.id)
            //    .where(f => f.filename.contains(partial))
            //    .oftype<saveditem>()
            //    .orderby(f => f.id)
            //    .take(limit)
            //    .tolistasync();

            //list<saveditem> savedtexts = await _dbcontext.savedtexts
            //    .where(t => t.user.id == user.id)
            //    .where(t => t.text.contains(partial))
            //    .oftype<saveditem>()
            //    .orderby(f => f.id)
            //    .take(limit)
            //    .tolistasync();

            //return savedfiles.union(savedtexts);
            return null;
        }

    }
}

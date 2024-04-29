using SavedBot.Exceptions;
using System.Collections;
using System.Reflection.Metadata;
using SavedBot.Model;

namespace SavedBot.Model
{
    internal interface IModelContext
    {
        /// <summary>
        /// Adds saved file to the Model container
        /// </summary>
        /// <param name="chatId">Telegram Chat Id</param>
        /// <param name="name">Custom name of the saved file</param>
        /// <param name="fileId">Telegram File Id</param>
        /// <exception cref="NameAlreadyExistsException"></exception>
        /// <summary>
        /// Adds saved text to the Model container
        /// </summary>
        /// <param name="chatId">Telegram Chat Id</param>
        /// <param name="name">Custom name of the saved text</param>
        /// <param name="text">Saved text</param>
        /// <exception cref="NameAlreadyExistsException"></exception>
        Task AddItemAsync(SavedItem item);
        /// <summary>
        /// Add new user
        /// </summary>
        /// <param name="user"></param>
        /// 
        Task ChangeLanguageAsync(TelegramUser user);

        Task<bool> ItemExistsAsync(SavedItem item);

        Task<bool> UserExistsAsync(long userId);

        Task RenameFileAsync(SavedFile file);

        Task RemoveItemAsync(SavedItem item);
        Task AddUserAsync(TelegramUser user);
        /// <summary>
        /// Get existing User by his Id
        /// </summary>
        /// <param name="userId"></param>
        Task<TelegramUser?> GetUserAsync(long userId);

        /// <summary>
        /// Searches for the saved messages which names contain the partial
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="partial"></param>
        /// <returns></returns>
        Task<IEnumerable<SavedItem>> SearchAsync(TelegramUser user, string partial, int limit);
    }
}

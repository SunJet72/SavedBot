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
        Task AddItem(SavedItem item);
        /// <summary>
        /// Add new user
        /// </summary>
        /// <param name="user"></param>
        Task AddUser(TelegramUser user);
        /// <summary>
        /// Get existing User by his Id
        /// </summary>
        /// <param name="userId"></param>
        Task<TelegramUser> GetUser(long userId);
        /// <summary>
        /// Get existing User by his ChatId
        /// </summary>
        /// <param name="chatId"></param>

        // SavedFile FindFile(long chatId, string name);
        /// <summary>
        /// Returns found saved text or throws an exception
        /// </summary>
        /// <param name="chatId">Telegram Chat Id</param>
        /// <param name="name">Custom name of the saved text</param>
        /// <exception cref="SavedMessageNotFoundException"></exception>
        // string FindText(long chatId, string name);
        /// <summary>
        /// Searches for the saved messages which names contain the partial
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="partial"></param>
        /// <returns></returns>
        Task<IQueryable<SavedItem>> Search(TelegramUser user, string partial, int limit);
    }
}

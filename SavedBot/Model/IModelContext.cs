using SavedBot.Exceptions;
using System.Collections;
using System.Reflection.Metadata;

namespace SavedBot.Model
{
    public interface IModelContext
    {
        /// <summary>
        /// Adds saved file to the Model container
        /// </summary>
        /// <param name="chatId">Telegram Chat Id</param>
        /// <param name="name">Custom name of the saved file</param>
        /// <param name="fileId">Telegram File Id</param>
        /// <exception cref="NameAlreadyExistsException"></exception>
        void AddFile(long chatId, string name, SavedFile file);
        /// <summary>
        /// Adds saved text to the Model container
        /// </summary>
        /// <param name="chatId">Telegram Chat Id</param>
        /// <param name="name">Custom name of the saved text</param>
        /// <param name="text">Saved text</param>
        /// <exception cref="NameAlreadyExistsException"></exception>
        void AddText(long chatId, string name, string text);
        /// <summary>
        /// Add new user
        /// </summary>
        /// <param name="user"></param>
        void AddUser(User user);
        /// <summary>
        /// Get existing User by his Id
        /// </summary>
        /// <param name="userId"></param>
        User? GetUserById(long userId);
        /// <summary>
        /// Get existing User by his ChatId
        /// </summary>
        /// <param name="chatId"></param>
        User? GetUserByChatId(long chatId);
        /// <summary>
        /// Returns found saved file or throws an exception
        /// </summary>
        /// <param name="chatId">Telegram Chat Id</param>
        /// <param name="name">Custom name of the saved file</param>
        /// <exception cref="SavedMessageNotFoundException"></exception>
        SavedFile GetFile(long chatId, string name);
        /// <summary>
        /// Returns found saved text or throws an exception
        /// </summary>
        /// <param name="chatId">Telegram Chat Id</param>
        /// <param name="name">Custom name of the saved text</param>
        /// <exception cref="SavedMessageNotFoundException"></exception>
        string GetText(long chatId, string name);
        /// <summary>
        /// Searches for the saved messages which names contain the partial
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="partial"></param>
        /// <returns></returns>
        IEnumerable<string> Search(long chatId, string partial, int limit);
    }
}

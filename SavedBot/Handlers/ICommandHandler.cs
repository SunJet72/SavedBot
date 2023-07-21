using SavedBot.Chat;
using SavedBot.Exceptions;

namespace SavedBot.Handlers
{
    public interface ICommandHandler
    {
        /// <summary>
        /// Handles a command
        /// </summary>
        /// <param name="ongoingChat"></param>
        /// <exception cref="NotFoundOngoingAddChatException"></exception>
        public void Handle(OngoingChat ongoingChat);
    }
}

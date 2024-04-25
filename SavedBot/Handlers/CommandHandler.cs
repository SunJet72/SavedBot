using Microsoft.Extensions.Logging;
using SavedBot.Chat;
using SavedBot.Model;

namespace SavedBot.Handlers
{
    internal abstract class CommandHandler(IModelContext modelContext, ILogger logger) : ICommandHandler
    {
        protected IModelContext _modelContext = modelContext;
        protected readonly ILogger _logger = logger;

        public abstract void Handle(OngoingChat ongoingChat);
        public abstract bool IsNamed(long chatId);
    }
}

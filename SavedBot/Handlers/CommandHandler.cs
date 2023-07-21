using SavedBot.Chat;
using SavedBot.Loggers;
using SavedBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Handlers
{
    public abstract class CommandHandler : ICommandHandler
    {
        protected IModelContext _modelContext;
        protected ILogger _logger { get; private set; }
        public CommandHandler(IModelContext modelContext, ILogger logger)
        {
            _modelContext = modelContext;
            _logger = logger;
        }

        public virtual void Handle(OngoingChat ongoingChat)
        {
            throw new NotImplementedException();
        }
    }
}

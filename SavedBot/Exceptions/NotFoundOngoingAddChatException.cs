using System.Runtime.Serialization;

namespace SavedBot.Exceptions
{
    /// <summary>
    /// Thrown if the sequence of a command is broken
    /// </summary>
    [Serializable]
    internal class NotFoundOngoingAddChatException : Exception
    {
        public NotFoundOngoingAddChatException() : base("You should call /add command first to add new item to list") { }
    }
}
using System.Runtime.Serialization;

namespace SavedBot.Exceptions
{
    /// <summary>
    /// Thrown if the saved message is not found by the specified name
    /// </summary>
    [Serializable]
    internal class SavedMessageNotFoundException : Exception
    {
        public SavedMessageNotFoundException(string name) : base($"Saved message with name { name } is not found") { }
    }
}
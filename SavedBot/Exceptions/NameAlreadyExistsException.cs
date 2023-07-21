using System.Runtime.Serialization;

namespace SavedBot.Exceptions
{
    /// <summary>
    /// Thrown if the name of a saved message is already taken
    /// </summary>
    [Serializable]
    public class NameAlreadyExistsException : Exception
    {
        public NameAlreadyExistsException(string name) : base($"The name {name} is already taken, remove the existsing record first") { }
    }
}
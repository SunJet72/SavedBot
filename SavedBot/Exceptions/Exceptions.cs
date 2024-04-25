﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Exceptions
{
    /// <summary>
    /// Thrown if the name of a saved message is already taken
    /// </summary>
    public class NameAlreadyExistsException(string name) :
        Exception($"The name {name} is already taken, remove the existsing record first");

    /// <summary>
    /// Thrown if the sequence of a command is broken
    /// </summary>
    internal class NotFoundOngoingAddChatException() :
        Exception("You should call /add command first to add new item to list");

    /// <summary>
    /// Thrown if the saved message is not found by the specified name
    /// </summary>
    internal class SavedMessageNotFoundException(string name) :
        Exception($"Saved message with name {name} is not found");

    /// <summary>
    /// Thrown if the telegram token was not found in user secrets
    /// </summary>
    public class TelegramBotTokenNotFoundException() : 
        Exception($"The telegram bot token was not found, unable to proceed.");

    /// <summary>
    /// Thrown if the webhook url was not found in the configuration
    /// </summary>
    public class WebhookUrlNotFoundException() :
        Exception($"The webhook url was not found, unable to proceed.");

}

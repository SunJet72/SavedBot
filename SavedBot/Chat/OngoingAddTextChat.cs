﻿namespace SavedBot.Chat
{
    public class OngoingAddTextChat : OngoingChat
    {
        public string Text { get; private set; }
        public OngoingAddTextChat(long chatId, string text) : base(chatId)
        {
            Text = text;
        }
    }
}

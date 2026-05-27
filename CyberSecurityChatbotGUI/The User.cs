using System;

namespace CyberSecurityChatbot_1
{
    // ============================================================
    // USER CLASS - Stores information about the person using the chatbot
    // PART 2 UPGRADE: Added FavouriteTopic and LastTopic for Memory & Recall (Requirement 5)
    // ============================================================
    public class User
    {
        // --------------------------------------------------------
        // PROPERTIES
        // --------------------------------------------------------

        // Stores the user's name - used to personalise responses
        public string Name { get; set; }

        // Tracks whether the user wants to exit
        public bool IsExiting { get; set; }

        // Records when the session started - used to calculate session time
        public DateTime StartTime { get; set; }

        // Counts how many questions have been asked - for stats/personalisation
        public int QuestionCount { get; set; }

        // PART 2 - MEMORY FEATURE (Requirement 5)
        // Stores the user's favourite cybersecurity topic (e.g. "privacy", "phishing")
        // This lets the chatbot refer back to it later in the conversation
        public string FavouriteTopic { get; set; }

        // PART 2 - CONVERSATION FLOW (Requirement 4)
        // Remembers the last topic discussed so follow-up questions like
        // "tell me more" or "explain more" continue on the same topic
        public string LastTopic { get; set; }

        // --------------------------------------------------------
        // CONSTRUCTOR - Runs when a new User object is created
        // --------------------------------------------------------
        public User(string name)
        {
            Name = name;
            IsExiting = false;
            StartTime = DateTime.Now;
            QuestionCount = 0;
            FavouriteTopic = "";   // No favourite topic yet
            LastTopic = "";        // No topic discussed yet
        }

        // --------------------------------------------------------
        // INCREMENT QUESTIONS - Adds 1 to the question counter
        // Called every time the user sends a message
        // --------------------------------------------------------
        public void IncrementQuestions()
        {
            QuestionCount++;
        }

        // --------------------------------------------------------
        // GET SESSION TIME - Calculates how long the user has been chatting
        // Returns a readable string like "5 min, 30 sec"
        // --------------------------------------------------------
        public string GetSessionTime()
        {
            TimeSpan time = DateTime.Now - StartTime;
            return $"{time.Minutes} min, {time.Seconds} sec";
        }
    }
}
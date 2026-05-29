using System;

namespace CyberSecurityChatbotGUI
{
    // ============================================================
    // USER CLASS
    // Stores everything the chatbot needs to remember about the user.
    //
    // MEMORY SYSTEM (Requirement 5):
    //   FavouriteTopic  — set automatically when the user shows repeated
    //                     interest in a topic, or says "I'm interested in X"
    //   LastTopic       — updated every single message so "tell me more"
    //                     always continues on the right subject
    //   TopicMentionCount — Dictionary that counts how many times each
    //                     topic has come up; when a topic hits 2+ mentions
    //                     it becomes the FavouriteTopic automatically
    // ============================================================
    public class User
    {
        // ── Basic info ──────────────────────────────────────────
        // The user's name — used to personalise every response
        public string Name { get; set; }

        // Tracks whether the user has typed "exit" / "quit"
        public bool IsExiting { get; set; }

        // Records session start time — used by GetSessionTime()
        public DateTime StartTime { get; set; }

        // Total messages sent — shown in the header counter
        public int QuestionCount { get; set; }

        // ── Memory & Recall (Requirement 5) ─────────────────────
        // The topic the user has shown the most interest in.
        // Set by: "I'm interested in X"  OR  asking about a topic 2+ times.
        // Used to: personalise fallback responses and the header indicator.
        public string FavouriteTopic { get; set; }

        // The topic discussed in the most recent message.
        // Used by: "tell me more" / "explain more" conversation flow.
        public string LastTopic { get; set; }

        // Counts how many times each topic has been mentioned this session.
        // Key = topic name (e.g. "phishing"), Value = number of mentions.
        // When a count reaches 2, that topic becomes FavouriteTopic.
        public System.Collections.Generic.Dictionary<string, int> TopicMentionCount { get; set; }

        // ── Constructor ─────────────────────────────────────────
        public User(string name)
        {
            Name = name;
            IsExiting = false;
            StartTime = DateTime.Now;
            QuestionCount = 0;
            FavouriteTopic = "";
            LastTopic = "";
            TopicMentionCount = new System.Collections.Generic.Dictionary<string, int>();
        }

        // ── IncrementQuestions ──────────────────────────────────
        // Called every time the user sends a message
        public void IncrementQuestions()
        {
            QuestionCount++;
        }

        // ── RecordTopicMention ──────────────────────────────────
        // Called by the Chatbot every time it detects a topic.
        // Increments the mention count and auto-sets FavouriteTopic
        // when a topic has been mentioned 2 or more times.
        public void RecordTopicMention(string topic)
        {
            if (string.IsNullOrEmpty(topic)) return;

            // Add the topic to the dictionary or increase its count
            if (TopicMentionCount.ContainsKey(topic))
                TopicMentionCount[topic]++;
            else
                TopicMentionCount[topic] = 1;

            // Auto-set favourite when the topic has been mentioned 2+ times
            // (only if the user hasn't already explicitly set one)
            if (TopicMentionCount[topic] >= 2 && string.IsNullOrEmpty(FavouriteTopic))
            {
                FavouriteTopic = topic;
            }
        }

        // ── SetFavouriteTopic ────────────────────────────────────
        // Called when user explicitly says "I'm interested in X".
        // This always overrides the auto-detected favourite.
        public void SetFavouriteTopic(string topic)
        {
            if (!string.IsNullOrEmpty(topic))
            {
                FavouriteTopic = topic;
                // Also give it a high count so it stays favourite
                TopicMentionCount[topic] = 5;
            }
        }

        // ── GetSessionTime ──────────────────────────────────────
        // Returns how long the user has been chatting, e.g. "5 min, 30 sec"
        public string GetSessionTime()
        {
            TimeSpan time = DateTime.Now - StartTime;
            return $"{time.Minutes} min, {time.Seconds} sec";
        }
    }
}
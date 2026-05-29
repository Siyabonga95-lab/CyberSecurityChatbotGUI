using System;
using System.Collections.Generic;

namespace CyberSecurityChatbotGUI
{
    // ============================================================
    // CHATBOT CLASS — The brain of the cybersecurity chatbot
    //
    // REQUIREMENTS COVERED:
    //   Req 2  — Keyword Recognition   : 6+ topics, natural language input
    //   Req 3  — Random Responses      : Dictionary<string,string[]> + Random
    //   Req 4  — Conversation Flow     : "tell me more", "explain more", follow-ups
    //   Req 5  — Memory & Recall       : auto-detects + explicitly stores favourite topic
    //   Req 6  — Sentiment Detection   : 5 emotions, responds + gives tip immediately
    //   Req 7  — Error Handling        : empty input, unknown input, graceful fallbacks
    //   Req 8  — Code Optimisation     : OOP, Dictionary, private methods, comments
    // ============================================================
    public class Chatbot
    {
        // ============================================================
        // REQUIREMENT 3 + 8 — RANDOM RESPONSES stored in a Dictionary.
        //
        // Dictionary<string, string[]>:
        //   Key   = topic name (matches user.LastTopic / user.FavouriteTopic)
        //   Value = array of 5 varied tip strings
        //
        // Using a Dictionary means:
        //   - Easy to add new topics without changing GetResponse logic
        //   - GetRandomTip() works for ANY topic with one line of code
        //   - Clean, professional, expandable structure (Requirement 8)
        // ============================================================
        private readonly Dictionary<string, string[]> _randomTips = new Dictionary<string, string[]>
        {
            {
                "phishing", new[]
                {
                    "Be cautious of emails asking for personal information — scammers disguise themselves as trusted organisations.",
                    "Always check the sender's email address carefully. 'paypa1.com' is fake — 'paypal.com' is real.",
                    "Real banks and companies will NEVER ask for your password or PIN via email.",
                    "Watch for urgent language like 'ACT NOW!' or 'Your account will be closed' — classic red flags!",
                    "Hover over links before clicking — your browser shows the real destination at the bottom of the screen."
                }
            },
            {
                "password", new[]
                {
                    "Use strong, unique passwords for every account — never reuse the same password across sites.",
                    "A passphrase like 'Blue-Horse-Sunshine-Rain!' is easier to remember and harder to crack than 'P@ssw0rd'.",
                    "Never reuse passwords — if one site is hacked, all your other accounts become vulnerable.",
                    "Use a free password manager like Bitwarden — it remembers all passwords, you only need one master password.",
                    "Enable two-factor authentication (2FA) — even if your password leaks, hackers still can't get in!"
                }
            },
            {
                "privacy", new[]
                {
                    "Review your social media privacy settings — limit who can see your posts and personal information.",
                    "Avoid sharing your full birthdate, ID number, or home address on public platforms.",
                    "Use private/incognito browsing mode when using shared or public computers.",
                    "Only fill in personal information on online forms that is truly necessary — less is safer.",
                    "Regularly check which apps have access to your location, camera, and contacts — revoke what you don't need."
                }
            },
            {
                "scam", new[]
                {
                    "If an offer sounds too good to be true, it almost certainly is a scam.",
                    "Prize scams are very common — no legitimate company asks you to pay fees to claim a prize.",
                    "Verify requests for money transfers by calling the person directly on a known, verified number.",
                    "In South Africa, report scams to SABRIC: 0861 022 339 or report@cybersecurity.gov.za.",
                    "Never send money or gift cards to someone you've only met online — this is a classic scam tactic."
                }
            },
            {
                "virus", new[]
                {
                    "Keep your antivirus software updated — new viruses are created every single day.",
                    "Only download software from official, trusted websites — never from random pop-ups.",
                    "If your computer suddenly slows down or shows strange pop-ups, run a full antivirus scan immediately.",
                    "Never plug in an unknown USB drive — it could silently install malware on your computer.",
                    "Back up your important files regularly — if ransomware strikes, you won't lose everything."
                }
            },
            {
                "browsing", new[]
                {
                    "Always check for the padlock icon 🔒 and 'https://' before entering any personal information.",
                    "Avoid clicking pop-up ads — 'You won an iPhone!' is always a scam.",
                    "Keep your browser updated — updates patch security holes that hackers exploit.",
                    "Don't save passwords in your browser — use a dedicated password manager instead.",
                    "Use a VPN on public WiFi — it encrypts your connection so hackers can't steal your data."
                }
            }
        };

        // ============================================================
        // REQUIREMENT 3 — RANDOM RESPONSE SELECTOR
        // Picks a random tip from the dictionary for a given topic.
        // Uses Random.Next() to select a different tip each time.
        // ============================================================
        private string GetRandomTip(string topic)
        {
            if (_randomTips.ContainsKey(topic))
            {
                Random rand = new Random();
                string[] tips = _randomTips[topic];
                return tips[rand.Next(tips.Length)];
            }
            // Fallback if topic not in dictionary
            return "Always think before you click — when in doubt, don't! 🛡️";
        }

        // ============================================================
        // MAIN GET RESPONSE — called from MainWindow.xaml.cs on every message
        //
        // ORDER OF CHECKS (important — don't reorder):
        //   1. Empty input guard
        //   2. Sentiment detection (emotions first — most empathetic)
        //   3. Conversation flow ("tell me more" / "explain more")
        //   4. Explicit interest statement ("I'm interested in X")
        //   5. Menu / help
        //   6. Small talk (how are you, thanks)
        //   7. Topic keyword recognition (the main learning content)
        //   8. Memory-based fallback (uses FavouriteTopic if known)
        //   9. Generic error fallback
        // ============================================================
        public string GetResponse(string input, User user)
        {
            string msg = input.ToLower().Trim();
            string name = user.Name;

            // ── 1. REQUIREMENT 7: Empty input guard ─────────────
            if (string.IsNullOrWhiteSpace(msg))
                return $"Please type something so I can help you, {name}! 😊";

            // ── 2. REQUIREMENT 6: Sentiment detection ───────────
            // Check emotions BEFORE topics so the bot responds with
            // empathy first, then still delivers a relevant tip.
            string sentiment = DetectSentiment(msg, name);
            if (sentiment != null)
            {
                string tipAfterSentiment = GetTopicTipFromContext(msg, user);
                return sentiment + tipAfterSentiment;
            }

            // ── 3. REQUIREMENT 4: Conversation flow ─────────────
            // "tell me more", "another tip", "explain more" etc.
            // Continues the LAST topic without any reset.
            if (msg.Contains("tell me more") || msg.Contains("explain more") ||
                msg.Contains("more info") || msg.Contains("another tip") ||
                msg.Contains("give me another") || msg.Contains("more details") ||
                msg.Contains("keep going") || msg.Contains("continue"))
            {
                if (!string.IsNullOrEmpty(user.LastTopic))
                {
                    user.RecordTopicMention(user.LastTopic);
                    return $"Sure {name}! Here's another tip on {user.LastTopic}:\n\n" +
                           GetRandomTip(user.LastTopic) +
                           $"\n\nWant even more? Just say 'tell me more' again! 😊";
                }
                return $"We haven't discussed a topic yet, {name}. " +
                       "Try asking about phishing, passwords, privacy, scams, or viruses!";
            }

            // ── 4. REQUIREMENT 5: Explicit interest statement ───
            // "I'm interested in X", "my favourite topic is X", "I love X"
            // This EXPLICITLY sets the favourite topic in User memory.
            if (msg.Contains("i'm interested in") || msg.Contains("im interested in") ||
                msg.Contains("i am interested in") || msg.Contains("my favourite topic") ||
                msg.Contains("my favorite topic") || msg.Contains("i love learning about") ||
                msg.Contains("i want to learn about") || msg.Contains("tell me about"))
            {
                return HandleExplicitInterest(msg, user);
            }

            // ── 5. Menu / Help ───────────────────────────────────
            if (msg == "menu" || msg == "help" || msg.Contains("what can you do") ||
                msg.Contains("what topics") || msg.Contains("show topics"))
            {
                return GetMenu();
            }

            // ── 6. Small talk ────────────────────────────────────
            if (msg.Contains("how are you") || msg.Contains("how you feeling") ||
                msg.Contains("what is your purpose") || msg.Contains("who are you") ||
                msg.Contains("you okay"))
            {
                string[] feelings = {
                    $"I'm doing great {name}! 😊 Thanks for asking! I love helping people learn about cybersecurity.",
                    $"Fantastic {name}! My purpose is to help you stay safe online. What would you like to learn today?",
                    $"I'm super excited {name}! 🌟 Every time someone learns about cybersecurity, the internet gets a little safer!"
                };
                return feelings[new Random().Next(feelings.Length)];
            }

            if (msg.Contains("thank") || msg.Contains("thanks") || msg.Contains("appreciate"))
                return $"You're very welcome, {name}! 😊 Stay safe online! Anything else you'd like to learn?";

            // ── 7. REQUIREMENT 2: Topic keyword recognition ──────
            // Each block: sets LastTopic, calls RecordTopicMention (for
            // auto-favourite), then returns the topic-specific response.

            if (msg.Contains("phish") || msg.Contains("scam email") || msg.Contains("fake email"))
            {
                user.LastTopic = "phishing";
                user.RecordTopicMention("phishing");
                return GetPhishingResponse(msg, name);
            }

            if (msg.Contains("password") || msg.Contains("passphrase") ||
                msg.Contains("2fa") || msg.Contains("two factor") || msg.Contains("two-factor"))
            {
                user.LastTopic = "password";
                user.RecordTopicMention("password");
                return GetPasswordResponse(msg, name);
            }

            if (msg.Contains("privacy") || msg.Contains("private") ||
                msg.Contains("personal info") || msg.Contains("personal data"))
            {
                user.LastTopic = "privacy";
                user.RecordTopicMention("privacy");
                return GetPrivacyResponse(msg, name);
            }

            if (msg.Contains("scam") || msg.Contains("fraud") || msg.Contains("trick"))
            {
                user.LastTopic = "scam";
                user.RecordTopicMention("scam");
                return GetScamResponse(msg, name);
            }

            if (msg.Contains("virus") || msg.Contains("malware") ||
                msg.Contains("ransomware") || msg.Contains("antivirus"))
            {
                user.LastTopic = "virus";
                user.RecordTopicMention("virus");
                return GetVirusResponse(msg, name);
            }

            if (msg.Contains("link") || msg.Contains("url") || msg.Contains("click"))
            {
                user.LastTopic = "phishing";
                user.RecordTopicMention("phishing");
                return GetLinkResponse(msg, name);
            }

            if (msg.Contains("browse") || msg.Contains("internet") ||
                msg.Contains("website") || msg.Contains("web") || msg.Contains("https"))
            {
                user.LastTopic = "browsing";
                user.RecordTopicMention("browsing");
                return GetBrowsingResponse(msg, name);
            }

            if (msg.Contains("report") || msg.Contains("saps") || msg.Contains("police"))
            {
                user.LastTopic = "scam";
                user.RecordTopicMention("scam");
                return GetReportingResponse(msg, name);
            }

            if (msg.Contains("cyber") || msg.Contains("security") ||
                msg.Contains("what is") || msg.Contains("protect") || msg.Contains("safe"))
            {
                user.LastTopic = "phishing";
                user.RecordTopicMention("phishing");
                return GetCyberResponse(msg, name);
            }

            // ── 8. REQUIREMENT 5: Memory-based fallback ──────────
            // If we know the user's favourite topic, personalise the fallback
            if (!string.IsNullOrEmpty(user.FavouriteTopic))
            {
                return $"Hmm {name}, I didn't quite catch that. 🤔\n\n" +
                       $"Since you're interested in {user.FavouriteTopic}, here's a tip on that:\n\n" +
                       GetRandomTip(user.FavouriteTopic) +
                       "\n\nOr type 'menu' to explore all topics!";
            }

            // ── 9. REQUIREMENT 7: Generic error fallback ────────
            return $"Hmm {name}, I'm not sure I understood that. 🤔\n" +
                   "Try asking about phishing, passwords, privacy, scams, viruses, links, or browsing.\n" +
                   "Type 'menu' to see everything I can help with!";
        }

        // ============================================================
        // REQUIREMENT 6 — SENTIMENT DETECTION
        //
        // Checks for 5 emotional states BEFORE topic processing.
        // Returns an empathetic opening sentence, or null if no emotion found.
        // The calling code then appends a relevant tip so the user
        // doesn't have to ask twice (rubric requirement).
        // ============================================================
        private string DetectSentiment(string msg, string name)
        {
            // Worried / scared / nervous / afraid
            if (msg.Contains("worried") || msg.Contains("scared") ||
                msg.Contains("nervous") || msg.Contains("afraid") || msg.Contains("fear"))
            {
                return $"It's completely understandable to feel that way, {name}. 💙 " +
                       "Scammers can be very convincing. Let me share a tip to help you stay safe:\n\n";
            }

            // Curious / wondering / want to know
            if (msg.Contains("curious") || msg.Contains("wondering") || msg.Contains("want to know"))
            {
                return $"I love your curiosity, {name}! 🌟 " +
                       "That's exactly the right mindset for staying safe online. Here's something useful:\n\n";
            }

            // Frustrated / angry / annoyed / confused
            if (msg.Contains("frustrated") || msg.Contains("angry") ||
                msg.Contains("annoyed") || msg.Contains("confused") ||
                msg.Contains("don't understand") || msg.Contains("lost"))
            {
                return $"I hear you, {name} — cybersecurity can feel overwhelming at first. 😌 " +
                       "Take a breath! Let me break it down simply:\n\n";
            }

            // Happy / excited
            if (msg.Contains("happy") || msg.Contains("excited") || msg.Contains("love this"))
            {
                return $"Love the energy, {name}! 🎉 Let's keep that momentum going. Here's something to add to your knowledge:\n\n";
            }

            // Tired / overwhelmed / exhausted
            if (msg.Contains("tired") || msg.Contains("overwhelmed") || msg.Contains("exhausted"))
            {
                return $"Take it easy, {name}. 😊 You don't have to learn everything at once! Here's just one quick tip:\n\n";
            }

            return null; // No sentiment detected — continue normal flow
        }

        // ============================================================
        // After a sentiment is detected, this picks a relevant tip
        // based on any topic keywords in the same message.
        // ============================================================
        private string GetTopicTipFromContext(string msg, User user)
        {
            if (msg.Contains("phish") || msg.Contains("email"))
            { user.LastTopic = "phishing"; user.RecordTopicMention("phishing"); return GetRandomTip("phishing"); }
            if (msg.Contains("password") || msg.Contains("login"))
            { user.LastTopic = "password"; user.RecordTopicMention("password"); return GetRandomTip("password"); }
            if (msg.Contains("privacy") || msg.Contains("private"))
            { user.LastTopic = "privacy"; user.RecordTopicMention("privacy"); return GetRandomTip("privacy"); }
            if (msg.Contains("scam") || msg.Contains("fraud"))
            { user.LastTopic = "scam"; user.RecordTopicMention("scam"); return GetRandomTip("scam"); }
            if (msg.Contains("virus") || msg.Contains("malware"))
            { user.LastTopic = "virus"; user.RecordTopicMention("virus"); return GetRandomTip("virus"); }

            // No topic keyword — give a general safety tip
            return "Always think before you click — when in doubt, don't! 🛡️\n\n" +
                   "Type 'menu' to explore all cybersecurity topics.";
        }

        // ============================================================
        // REQUIREMENT 5 — EXPLICIT INTEREST HANDLER
        //
        // Called when user says "I'm interested in X" or similar.
        // Uses user.SetFavouriteTopic() to store the preference,
        // confirms it back to the user, and gives an opening tip.
        // On repeat mentions, acknowledges it was already remembered.
        // ============================================================
        private string HandleExplicitInterest(string msg, User user)
        {
            // Detect which topic was mentioned
            string topic = DetectTopicFromMessage(msg);

            if (!string.IsNullOrEmpty(topic))
            {
                bool alreadyFavourite = user.FavouriteTopic == topic;

                // Save to memory
                user.SetFavouriteTopic(topic);
                user.LastTopic = topic;

                if (alreadyFavourite)
                {
                    // User mentions it again — show the bot remembers
                    return $"I already remember that you're interested in {topic}, {user.Name}! 🧠\n\n" +
                           $"Here's another tip on {topic}:\n\n" +
                           GetRandomTip(topic) +
                           "\n\nType 'tell me more' for more tips on this topic!";
                }

                // First time setting this favourite
                return $"Got it, {user.Name}! I'll remember that you're interested in {topic}. 🧠\n\n" +
                       $"It's a crucial part of staying safe online. Here's a tip to get you started:\n\n" +
                       GetRandomTip(topic) +
                       $"\n\nI'll keep {topic} in mind throughout our conversation. " +
                       "Type 'tell me more' anytime for another tip!";
            }

            // Topic word not recognised
            return $"That sounds interesting, {user.Name}! 😊 " +
                   "I specialise in cybersecurity topics. Try saying:\n\n" +
                   "• 'I'm interested in phishing'\n" +
                   "• 'I'm interested in passwords'\n" +
                   "• 'I'm interested in privacy'\n" +
                   "• 'I'm interested in scams'\n" +
                   "• 'I'm interested in viruses'\n\n" +
                   "I'll remember it for our whole conversation!";
        }

        // ============================================================
        // HELPER — detects which topic a message is about
        // Used by HandleExplicitInterest and GetTopicTipFromContext
        // ============================================================
        private string DetectTopicFromMessage(string msg)
        {
            if (msg.Contains("phish") || msg.Contains("email") || msg.Contains("fake email")) return "phishing";
            if (msg.Contains("password") || msg.Contains("passphrase")) return "password";
            if (msg.Contains("privacy") || msg.Contains("private")) return "privacy";
            if (msg.Contains("scam") || msg.Contains("fraud")) return "scam";
            if (msg.Contains("virus") || msg.Contains("malware") || msg.Contains("ransomware")) return "virus";
            if (msg.Contains("link") || msg.Contains("url")) return "phishing";
            if (msg.Contains("browse") || msg.Contains("web") || msg.Contains("internet")) return "browsing";
            return "";
        }

        // ============================================================
        // GET MENU
        // ============================================================
        private string GetMenu()
        {
            return "═══ Here's what I can teach you: ═══\n\n" +
                   "🎣 PHISHING      — Spot fake emails and scams\n" +
                   "🔑 PASSWORDS     — Create strong, safe passwords\n" +
                   "🔒 PRIVACY       — Protect your personal information\n" +
                   "⚠️  SCAMS         — Recognise and avoid scams\n" +
                   "🦠 VIRUSES       — Protect against malware\n" +
                   "🔗 LINKS         — Don't get tricked by fake links\n" +
                   "🌐 BROWSING      — Stay safe on the internet\n" +
                   "📋 REPORTING     — Where to report cybercrime in SA\n" +
                   "🛡️  CYBERSECURITY — What it is and why it matters\n\n" +
                   "Just type what you want to learn!\n" +
                   "Example: 'give me a phishing tip' or 'I'm interested in passwords'";
        }

        // ============================================================
        // PHISHING RESPONSES
        // ============================================================
        private string GetPhishingResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define") || msg.Contains("meaning"))
            {
                return $"Great question {name}! 🎣\n\n" +
                       "PHISHING is when scammers send fake emails pretending to be from real companies " +
                       "like banks, Netflix, or PayPal. They want you to click dangerous links or give away " +
                       "your passwords.\n\nThe name comes from 'fishing' — they're fishing for your information!\n\n" +
                       "Want to learn how to SPOT them? Just ask!";
            }
            if (msg.Contains("spot") || msg.Contains("identify") || msg.Contains("recognize") || msg.Contains("recognise"))
            {
                return $"You're going to be a pro at this {name}! 🌟\n\nHOW TO SPOT PHISHING EMAILS:\n\n" +
                       "1️⃣ Check the sender's email — 'paypa1.com' is FAKE, 'paypal.com' is REAL\n" +
                       "2️⃣ Look for spelling mistakes — scammers have bad grammar\n" +
                       "3️⃣ Watch for URGENT words like 'ACT NOW!' or 'Account Suspended'\n" +
                       "4️⃣ Hover over links without clicking to see where they really go\n" +
                       "5️⃣ If it sounds too good to be true — it's a scam!\n\n" +
                       "Real companies NEVER ask for passwords via email!";
            }
            if (msg.Contains("tip") || msg.Contains("give me"))
            {
                return $"Here's a phishing tip for you, {name}:\n\n" +
                       GetRandomTip("phishing") +
                       "\n\nType 'tell me more' for another tip!";
            }
            if (msg.Contains("example") || msg.Contains("show me"))
            {
                return $"Here are real phishing examples {name}:\n\n" +
                       " Fake Bank: 'URGENT! Your account is locked! Click here to verify'\n" +
                       " Prize Scam: 'You won R10,000! Send your bank details to claim'\n" +
                       " Fake Delivery: 'Your package is waiting. Track here: bit.ly/...'\n" +
                       " Netflix: 'Your account is suspended. Update payment now'\n\n" +
                       "Red flags: urgent language, links, requests for personal info!";
            }
            // Default — random tip
            return $"Here's something about phishing, {name}: 🎣\n\n" +
                   GetRandomTip("phishing") +
                   "\n\nYou can ask:\n• 'What is phishing?'\n• 'How to spot phishing?'\n• 'Give me a phishing tip'";
        }

        // ============================================================
        // PASSWORD RESPONSES
        // ============================================================
        private string GetPasswordResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! 🔑\n\n" +
                       "A PASSWORD is like a digital key to your accounts. Password SAFETY means creating " +
                       "keys that hackers can't guess. A weak password is like leaving your front door unlocked!\n\n" +
                       "Want to learn how to create a strong one?";
            }
            if (msg.Contains("create") || msg.Contains("make") || msg.Contains("strong") || msg.Contains("how to"))
            {
                return $"Let's make you a password superhero {name}! 🦸\n\nHOW TO CREATE A STRONG PASSWORD:\n\n" +
                       " Make it LONG — 12+ characters minimum\n" +
                       " Mix it UP — CAPITALS, lowercase, numbers, symbols\n" +
                       " Use a PASSPHRASE — 'Blue-Horse-Sunshine-Rain!'\n" +
                       " Make it UNIQUE — different password for every account\n\n" +
                       " BAD: 'password123' (cracked in seconds)\n" +
                       " GOOD: 'MyD0g!sC00l2024@'";
            }
            if (msg.Contains("2fa") || msg.Contains("two factor") || msg.Contains("two-factor"))
            {
                return $"Excellent question {name}! 🔐\n\n" +
                       "TWO-FACTOR AUTHENTICATION (2FA) adds an extra layer of security. " +
                       "Even if a hacker steals your password, they still can't log in without the " +
                       "second factor — usually a code sent to your phone.\n\n" +
                       "Enable 2FA on your email, banking, and social media — it's one of the best things you can do!";
            }
            if (msg.Contains("tip") || msg.Contains("give me"))
            {
                return $"Here's a password tip for you, {name}:\n\n" +
                       GetRandomTip("password") +
                       "\n\nType 'tell me more' for another tip!";
            }
            return $"Here's something about passwords, {name}: 🔑\n\n" +
                   GetRandomTip("password") +
                   "\n\nYou can ask:\n• 'How to create a strong password?'\n• 'What is 2FA?'\n• 'Give me a password tip'";
        }

        // ============================================================
        // PRIVACY RESPONSES
        // ============================================================
        private string GetPrivacyResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! 🔒\n\n" +
                       "PRIVACY online means controlling who can see your personal information — your name, " +
                       "ID number, location, photos, and browsing habits. Protecting it means being careful " +
                       "what you share and with whom.\n\n" +
                       "Want a quick privacy tip?";
            }
            if (msg.Contains("tip") || msg.Contains("how to") || msg.Contains("protect"))
            {
                return $"Here's a privacy tip for you, {name}:\n\n" +
                       GetRandomTip("privacy") +
                       "\n\nType 'tell me more' for another tip!";
            }
            return $"Here's something about online privacy, {name}: 🔒\n\n" +
                   GetRandomTip("privacy") +
                   "\n\nYou can ask:\n• 'What is privacy?'\n• 'How to protect my privacy?'\n• 'Give me a privacy tip'";
        }

        // ============================================================
        // SCAM RESPONSES
        // ============================================================
        private string GetScamResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! ⚠️\n\n" +
                       "A SCAM is a dishonest scheme designed to trick you into giving away money or " +
                       "personal information. Common types include prize scams, romance scams, job scams, " +
                       "and fake investment opportunities.\n\n" +
                       "Want to know how to spot and avoid them?";
            }
            if (msg.Contains("tip") || msg.Contains("how to") || msg.Contains("avoid") || msg.Contains("spot"))
            {
                return $"Here's a scam awareness tip for you, {name}:\n\n" +
                       GetRandomTip("scam") +
                       "\n\nType 'tell me more' for another tip!";
            }
            return $"Here's something about scams, {name}: ⚠️\n\n" +
                   GetRandomTip("scam") +
                   "\n\nYou can ask:\n• 'What is a scam?'\n• 'How to spot a scam?'\n• 'Give me a scam tip'";
        }

        // ============================================================
        // VIRUS RESPONSES
        // ============================================================
        private string GetVirusResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! 🦠\n\n" +
                       "A COMPUTER VIRUS (or malware) is malicious software designed to damage your " +
                       "device, steal your data, or give hackers access to your system. Types include " +
                       "viruses, ransomware, spyware, and trojans.\n\n" +
                       "Want tips on how to protect yourself?";
            }
            if (msg.Contains("tip") || msg.Contains("protect") || msg.Contains("prevent"))
            {
                return $"Here's a virus protection tip for you, {name}:\n\n" +
                       GetRandomTip("virus") +
                       "\n\nType 'tell me more' for another tip!";
            }
            return $"Here's something about viruses and malware, {name}: 🦠\n\n" +
                   GetRandomTip("virus") +
                   "\n\nYou can ask:\n• 'What is a virus?'\n• 'How to protect against viruses?'";
        }

        // ============================================================
        // LINK RESPONSES
        // ============================================================
        private string GetLinkResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! 🔗\n\n" +
                       "A SUSPICIOUS LINK looks real but takes you to a FAKE website. Scammers use them " +
                       "to steal passwords or install viruses — like a sign saying 'BANK' that leads somewhere dangerous!\n\n" +
                       "Want to learn how to SPOT them?";
            }
            if (msg.Contains("spot") || msg.Contains("fake") || msg.Contains("identify") || msg.Contains("how to"))
            {
                return $"You're going to be a link detective {name}! 🕵️\n\nHOW TO SPOT FAKE LINKS:\n\n" +
                       "1️⃣ HOVER over the link (DON'T click!) — see the real destination\n" +
                       "2️⃣ Look for spelling tricks — 'faceb00k.com' is FAKE\n" +
                       "3️⃣ Check for 'https://' — the 's' means secure\n" +
                       "4️⃣ Shortened links like bit.ly hide where they really go\n" +
                       "5️⃣ Trust your gut — if something feels wrong, DON'T CLICK!";
            }
            return $"Stay alert about links, {name}! 🔗\n\n" +
                   "You can ask:\n• 'What are suspicious links?'\n• 'How to spot fake links?'";
        }

        // ============================================================
        // SAFE BROWSING RESPONSES
        // ============================================================
        private string GetBrowsingResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! 🌐\n\n" +
                       "SAFE BROWSING means protecting yourself while using the internet — knowing which " +
                       "websites are trustworthy and keeping your personal information private.\n\n" +
                       "Want to learn HOW to browse safely?";
            }
            if (msg.Contains("tip") || msg.Contains("how to") || msg.Contains("check"))
            {
                return $"Here's a safe browsing tip for you, {name}:\n\n" +
                       GetRandomTip("browsing") +
                       "\n\nType 'tell me more' for another tip!";
            }
            return $"Here's something about safe browsing, {name}: 🌐\n\n" +
                   GetRandomTip("browsing") +
                   "\n\nYou can ask:\n• 'What is safe browsing?'\n• 'How to browse safely?'";
        }

        // ============================================================
        // REPORTING RESPONSES
        // ============================================================
        private string GetReportingResponse(string msg, string name)
        {
            if (msg.Contains("where") || (msg.Contains("report") && msg.Contains("phish")))
            {
                return $"You're doing the right thing {name}! 📋\n\nWHERE TO REPORT IN SOUTH AFRICA:\n" +
                       " Government: report@cybersecurity.gov.za\n" +
                       " Your Bank — call their fraud department immediately\n" +
                       " SABRIC (banking scams): 0861 022 339\n\n" +
                       "Keep the evidence — don't delete the emails!";
            }
            if (msg.Contains("saps") || msg.Contains("police"))
            {
                return $"Reporting to SAPS is important {name}! 🚔\n\n" +
                       " Crime Stop: 0860 010 111 (24/7, can be anonymous)\n" +
                       " Visit your local police station — ask for the Cybercrime Unit\n" +
                       " Bring ALL evidence: emails, screenshots, bank statements\n\n" +
                       "They'll give you a case number. Don't be scared — they're there to help!";
            }
            if (msg.Contains("scammed") || msg.Contains("victim") || msg.Contains("what to do"))
            {
                return $"I'm sorry if this happened to you {name}. 💙\n\nWHAT TO DO IF SCAMMED:\n" +
                       "1️⃣ DON'T PANIC — stay calm\n" +
                       "2️⃣ CHANGE all passwords immediately\n" +
                       "3️⃣ CALL YOUR BANK — freeze accounts and report fraud\n" +
                       "4️⃣ REPORT TO SAPS — get a case number\n" +
                       "5️⃣ RUN ANTIVIRUS scan on your device\n\n" +
                       "Acting fast is what matters most. You've got this!";
            }
            return $"I'd love to teach you about reporting {name}! 📋\n\n" +
                   "You can ask:\n• 'Where to report phishing?'\n• 'How to report to SAPS?'\n" +
                   "• 'What to do after being scammed?'";
        }

        // ============================================================
        // CYBERSECURITY GENERAL RESPONSES
        // ============================================================
        private string GetCyberResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define") || msg.Contains("meaning"))
            {
                return $"Great question {name}! 🛡️\n\n" +
                       "CYBERSECURITY is protecting yourself, your devices, and your information from " +
                       "online threats. It's like having:\n" +
                       " A lock on your digital door (passwords)\n" +
                       " A security guard (antivirus)\n" +
                       " Training to spot danger (awareness — like what you're doing now!)\n\n" +
                       "Want to learn WHY it's so important?";
            }
            if (msg.Contains("why") || msg.Contains("important") || msg.Contains("matter"))
            {
                return $"This is so important {name}! 🌍\n\n" +
                       "In South Africa, cyber attacks increased 200% recently!\n\n" +
                       "Cybersecurity protects:\n" +
                       " YOUR MONEY — bank accounts and cards\n" +
                       "🪪 YOUR IDENTITY — personal information\n" +
                       " YOUR DEVICES — computer and phone\n" +
                       " YOUR PRIVACY — photos and messages\n\n" +
                       "By learning this, you're already safer than most people!";
            }
            if (msg.Contains("threats") || msg.Contains("types") || msg.Contains("common"))
            {
                return $"Let me show you what to watch for {name}! 👀\n\nCOMMON CYBER THREATS:\n" +
                       " PHISHING — Fake emails that steal info\n" +
                       " MALWARE — Viruses on your device\n" +
                       " RANSOMWARE — Hackers lock your files for payment\n" +
                       "🪪 IDENTITY THEFT — Stealing your personal info\n" +
                       " SOCIAL ENGINEERING — Psychologically tricking you\n\n" +
                       "Now you know what to watch for! Type any topic to learn more.";
            }
            return $"I'd love to teach you about cybersecurity {name}! 🛡️\n\n" +
                   "You can ask:\n• 'What is cybersecurity?'\n• 'Why is cybersecurity important?'\n" +
                   "• 'What are common cyber threats?'";
        }
    }
}
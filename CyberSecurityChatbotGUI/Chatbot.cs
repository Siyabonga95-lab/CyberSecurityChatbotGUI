using CyberSecurityChatbotGUI;
using System;
using System.Collections.Generic;

namespace CyberSecurityChatbot_1
{
    // ============================================================
    // CHATBOT CLASS - The brain of the chatbot
    //
    // PART 2 UPGRADES:
    //   Requirement 2 - Keyword Recognition  (enhanced with more keywords)
    //   Requirement 3 - Random Responses     (arrays of varied responses)
    //   Requirement 4 - Conversation Flow    (follow-up / "tell me more")
    //   Requirement 5 - Memory & Recall      (favourite topic stored + recalled)
    //   Requirement 6 - Sentiment Detection  (worried / curious / frustrated)
    //   Requirement 7 - Error Handling       (graceful fallback messages)
    //   Requirement 8 - Code Optimisation    (Dictionary, List, arrays, OOP)
    // ============================================================
    public class Chatbot
    {
        // --------------------------------------------------------
        // REQUIREMENT 8 - CODE OPTIMISATION
        // I use a Dictionary<string, string[]> to store random tip arrays
        // for each topic. This is cleaner than huge if-else blocks and
        // makes the code easy to expand in Part 3.
        // Key   = topic name (e.g. "phishing")
        // Value = string array of different tip responses
        // --------------------------------------------------------
        private Dictionary<string, string[]> _randomTips = new Dictionary<string, string[]>
        {
            {
                "phishing", new string[]
                {
                    "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                    "Always check the sender's email address carefully. 'paypa1.com' is fake — 'paypal.com' is real.",
                    "Real banks and companies will NEVER ask for your password or PIN via email.",
                    "Watch for urgent language like 'ACT NOW' or 'Your account will be closed' — that's a red flag!",
                    "Hover over links before clicking — your browser shows the real destination at the bottom of the screen."
                }
            },
            {
                "password", new string[]
                {
                    "Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords.",
                    "A passphrase like 'Blue-Horse-Sunshine-Rain!' is easier to remember and harder to crack than 'P@ssw0rd'.",
                    "Never reuse the same password on multiple sites — if one is hacked, all your accounts become vulnerable.",
                    "Use a free password manager like Bitwarden to remember all your passwords — you only need one master password.",
                    "Enable two-factor authentication (2FA) — even if someone gets your password, they still can't get in!"
                }
            },
            {
                "privacy", new string[]
                {
                    "Review your social media privacy settings — limit who can see your posts and personal information.",
                    "Avoid sharing your full birthdate, ID number, or address on public platforms.",
                    "Use private/incognito browsing when using shared or public computers.",
                    "Be careful what you share in online forms — only provide info that is truly necessary.",
                    "Regularly check which apps have access to your location, camera, and contacts and revoke unnecessary permissions."
                }
            },
            {
                "scam", new string[]
                {
                    "If an offer sounds too good to be true, it almost certainly is a scam.",
                    "Prize scams are very common — no legitimate company asks you to pay fees to claim a prize.",
                    "Verify requests for money transfers by calling the person directly on a known number.",
                    "In South Africa, report scams to SABRIC on 0861 022 339 or report@cybersecurity.gov.za.",
                    "Never send money or gift cards to someone you've only met online — this is a classic scam tactic."
                }
            },
            {
                "virus", new string[]
                {
                    "Keep your antivirus software updated — new viruses are created every day.",
                    "Never download software from unofficial websites — only use trusted sources like official app stores.",
                    "If your computer suddenly slows down or shows strange pop-ups, run a full antivirus scan immediately.",
                    "Avoid plugging in unknown USB drives — they can carry malware that installs silently.",
                    "Back up your important files regularly — if ransomware strikes, you won't lose everything."
                }
            }
        };

        // --------------------------------------------------------
        // REQUIREMENT 3 - RANDOM RESPONSES
        // This helper method picks a random tip from the dictionary
        // for a given topic. Called when user asks for "a tip" or
        // "give me another tip".
        // --------------------------------------------------------
        private string GetRandomTip(string topic)
        {
            if (_randomTips.ContainsKey(topic))
            {
                Random rand = new Random();
                string[] tips = _randomTips[topic];
                return tips[rand.Next(tips.Length)];
            }
            return "Stay safe online — always think before you click!";
        }

        // ============================================================
        // MAIN GET RESPONSE METHOD
        // This is called from MainWindow.xaml.cs every time the user sends a message.
        // It takes the user's message and the User object (for memory/name).
        // It returns the chatbot's response as a string.
        // ============================================================
        public string GetResponse(string input, User user)
        {
            // Store the original input, then work with lowercase for comparisons
            string msg = input.ToLower().Trim();
            string name = user.Name;

            // --------------------------------------------------------
            // REQUIREMENT 7 - ERROR HANDLING: Empty input check
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(msg))
            {
                return $"Please type something so I can help you, {name}! 😊";
            }

            // --------------------------------------------------------
            // REQUIREMENT 6 - SENTIMENT DETECTION
            // Check for emotional keywords FIRST, before topic keywords.
            // This way the bot responds empathetically when the user
            // expresses a feeling, and then also gives a relevant tip.
            // --------------------------------------------------------
            string sentimentResponse = DetectSentiment(msg, name);
            if (sentimentResponse != null)
            {
                // After responding to the sentiment, we also try to give
                // a relevant tip so the user doesn't have to ask again
                string topicTip = GetTopicTipFromSentimentContext(msg, user);
                return sentimentResponse + topicTip;
            }

            // --------------------------------------------------------
            // REQUIREMENT 4 - CONVERSATION FLOW
            // Handle follow-up phrases like "tell me more", "give me another tip",
            // "explain more" — continue on the last topic without restarting
            // --------------------------------------------------------
            if (msg.Contains("tell me more") || msg.Contains("explain more") ||
                msg.Contains("more info") || msg.Contains("give me another tip") ||
                msg.Contains("another tip") || msg.Contains("more details"))
            {
                if (!string.IsNullOrEmpty(user.LastTopic))
                {
                    return $"Sure {name}! Here's another tip on {user.LastTopic}:\n\n" +
                           GetRandomTip(user.LastTopic) +
                           $"\n\nWant to know even more? Just ask! 😊";
                }
                else
                {
                    return $"Sure {name}! We haven't discussed a specific topic yet. " +
                           "Try asking about phishing, passwords, privacy, scams, or viruses!";
                }
            }

            // --------------------------------------------------------
            // REQUIREMENT 5 - MEMORY & RECALL
            // If the user says "I'm interested in X" or "my favourite topic is X",
            // we store it and reference it later.
            // --------------------------------------------------------
            if (msg.Contains("i'm interested in") || msg.Contains("im interested in") ||
                msg.Contains("i am interested in") || msg.Contains("my favourite topic is") ||
                msg.Contains("my favorite topic is") || msg.Contains("i like"))
            {
                return HandleInterestStatement(msg, user);
            }

            // --------------------------------------------------------
            // SHOW MENU
            // --------------------------------------------------------
            if (msg == "menu" || msg == "help")
            {
                return GetMenu();
            }

            // --------------------------------------------------------
            // HOW ARE YOU / BOT FEELINGS
            // --------------------------------------------------------
            if (msg.Contains("how are you") || msg.Contains("how you feeling") ||
                msg.Contains("what is your purpose") || msg.Contains("you okay"))
            {
                string[] feelings = {
                    $"I'm doing great {name}! 😊 Thanks for asking! I love helping people learn about cybersecurity.",
                    $"Fantastic {name}! My purpose is to help you stay safe online. What would you like to learn today?",
                    $"I'm super excited {name}! Every time someone learns about cybersecurity, the internet gets a little safer. You're awesome for being here!"
                };
                Random rand = new Random();
                return feelings[rand.Next(feelings.Length)];
            }

            // --------------------------------------------------------
            // THANK YOU
            // --------------------------------------------------------
            if (msg.Contains("thank") || msg.Contains("thanks"))
            {
                return $"You're welcome {name}! 😊 Stay safe online! Anything else you'd like to learn?";
            }

            // --------------------------------------------------------
            // REQUIREMENT 2 - KEYWORD RECOGNITION (Enhanced)
            // The chatbot identifies cybersecurity keywords and responds.
            // Each block updates user.LastTopic for the conversation flow feature.
            // --------------------------------------------------------

            // PHISHING keywords
            if (msg.Contains("phish") || msg.Contains("scam email") || msg.Contains("fake email"))
            {
                user.LastTopic = "phishing";
                return GetPhishingResponse(msg, name);
            }

            // PASSWORD keywords
            if (msg.Contains("password") || msg.Contains("login") ||
                msg.Contains("passphrase") || msg.Contains("2fa") ||
                msg.Contains("two factor") || msg.Contains("two-factor"))
            {
                user.LastTopic = "password";
                return GetPasswordResponse(msg, name);
            }

            // PRIVACY keywords (NEW keyword for Part 2 - Requirement 2)
            if (msg.Contains("privacy") || msg.Contains("private") ||
                msg.Contains("personal info") || msg.Contains("personal data"))
            {
                user.LastTopic = "privacy";
                return GetPrivacyResponse(msg, name, user);
            }

            // SCAM keywords (NEW keyword for Part 2 - Requirement 2)
            if (msg.Contains("scam") || msg.Contains("fraud") || msg.Contains("trick"))
            {
                user.LastTopic = "scam";
                return GetScamResponse(msg, name);
            }

            // VIRUS / MALWARE keywords (NEW keyword for Part 2 - Requirement 2)
            if (msg.Contains("virus") || msg.Contains("malware") ||
                msg.Contains("ransomware") || msg.Contains("antivirus"))
            {
                user.LastTopic = "virus";
                return GetVirusResponse(msg, name);
            }

            // SUSPICIOUS LINKS keywords
            if (msg.Contains("link") || msg.Contains("url") || msg.Contains("click"))
            {
                user.LastTopic = "phishing";
                return GetLinkResponse(msg, name);
            }

            // SAFE BROWSING keywords
            if (msg.Contains("browse") || msg.Contains("internet") ||
                msg.Contains("website") || msg.Contains("web"))
            {
                user.LastTopic = "privacy";
                return GetBrowsingResponse(msg, name);
            }

            // REPORTING keywords
            if (msg.Contains("report") || msg.Contains("saps") || msg.Contains("police"))
            {
                user.LastTopic = "scam";
                return GetReportingResponse(msg, name);
            }

            // CYBERSECURITY general keywords
            if (msg.Contains("cyber") || msg.Contains("security") ||
                msg.Contains("what is") || msg.Contains("protect"))
            {
                user.LastTopic = "phishing";
                return GetCyberResponse(msg, name);
            }

            // --------------------------------------------------------
            // REQUIREMENT 5 - MEMORY RECALL
            // If we know the user's favourite topic, reference it in
            // the fallback response to personalise the experience
            // --------------------------------------------------------
            if (!string.IsNullOrEmpty(user.FavouriteTopic))
            {
                return $"Hmm {name}, I'm not sure I understood that. 🤔\n\n" +
                       $"Since you're interested in {user.FavouriteTopic}, here's a tip:\n" +
                       GetRandomTip(user.FavouriteTopic) +
                       "\n\nOr type 'menu' to see all topics I can help with!";
            }

            // --------------------------------------------------------
            // REQUIREMENT 7 - ERROR HANDLING: Default fallback
            // If nothing matched, give a helpful fallback message
            // --------------------------------------------------------
            return $"Hmm {name}, I'm not sure I understood that. 🤔 " +
                   "Try asking about phishing, passwords, privacy, scams, links, browsing, or reporting. " +
                   "Type 'menu' to see all topics!";
        }

        // ============================================================
        // REQUIREMENT 6 - SENTIMENT DETECTION
        // Detects emotional keywords and returns an empathetic response.
        // Returns null if no sentiment detected (so the main flow continues).
        // ============================================================
        private string DetectSentiment(string msg, string name)
        {
            // WORRIED / SCARED / NERVOUS
            if (msg.Contains("worried") || msg.Contains("scared") ||
                msg.Contains("nervous") || msg.Contains("afraid") || msg.Contains("fear"))
            {
                return $"It's completely understandable to feel that way, {name}. " +
                       "Scammers can be very convincing. Let me share some tips to help you stay safe.\n\n";
            }

            // CURIOUS / INTERESTED / WANT TO KNOW
            if (msg.Contains("curious") || msg.Contains("want to know") ||
                msg.Contains("wondering") || msg.Contains("interested"))
            {
                return $"I love your curiosity, {name}! 🌟 That's exactly the right mindset for staying safe online. Here's something useful:\n\n";
            }

            // FRUSTRATED / ANGRY / ANNOYED / CONFUSED
            if (msg.Contains("frustrated") || msg.Contains("angry") ||
                msg.Contains("annoyed") || msg.Contains("confused") ||
                msg.Contains("don't understand") || msg.Contains("lost"))
            {
                return $"I hear you, {name} — cybersecurity can feel overwhelming at first. " +
                       "Take a breath! Let me break it down simply for you:\n\n";
            }

            // HAPPY / EXCITED / GREAT
            if (msg.Contains("happy") || msg.Contains("excited") || msg.Contains("great"))
            {
                return $"Love the energy, {name}! 🎉 Let's keep that momentum going. Here's something to add to your knowledge:\n\n";
            }

            // TIRED / OVERWHELMED
            if (msg.Contains("tired") || msg.Contains("overwhelmed") || msg.Contains("exhausted"))
            {
                return $"Take it easy, {name}. You don't have to learn everything at once! " +
                       "Here's just one quick tip for now:\n\n";
            }

            // No sentiment detected
            return null;
        }

        // ============================================================
        // HELPER: After detecting a sentiment, try to also give a relevant tip
        // so the user doesn't have to ask again (Requirement 6 requirement:
        // "does not make the user enter again for something on the topic")
        // ============================================================
        private string GetTopicTipFromSentimentContext(string msg, User user)
        {
            if (msg.Contains("phish") || msg.Contains("email") || msg.Contains("scam"))
            {
                user.LastTopic = "phishing";
                return GetRandomTip("phishing");
            }
            if (msg.Contains("password") || msg.Contains("login"))
            {
                user.LastTopic = "password";
                return GetRandomTip("password");
            }
            if (msg.Contains("privacy") || msg.Contains("private"))
            {
                user.LastTopic = "privacy";
                return GetRandomTip("privacy");
            }
            if (msg.Contains("scam") || msg.Contains("fraud"))
            {
                user.LastTopic = "scam";
                return GetRandomTip("scam");
            }
            if (msg.Contains("virus") || msg.Contains("malware"))
            {
                user.LastTopic = "virus";
                return GetRandomTip("virus");
            }

            // If no specific topic in the sentiment message, give a general tip
            return "Always think before you click — when in doubt, don't! 🛡️\n\n" +
                   "Type 'menu' to explore all cybersecurity topics I can help with.";
        }

        // ============================================================
        // REQUIREMENT 5 - MEMORY: Handle "I'm interested in X" statements
        // ============================================================
        private string HandleInterestStatement(string msg, User user)
        {
            // Try to figure out which topic they mentioned
            string detectedTopic = "";

            if (msg.Contains("phish") || msg.Contains("email")) detectedTopic = "phishing";
            else if (msg.Contains("password")) detectedTopic = "password";
            else if (msg.Contains("privacy") || msg.Contains("private")) detectedTopic = "privacy";
            else if (msg.Contains("scam") || msg.Contains("fraud")) detectedTopic = "scam";
            else if (msg.Contains("virus") || msg.Contains("malware")) detectedTopic = "virus";
            else if (msg.Contains("link") || msg.Contains("url")) detectedTopic = "phishing";
            else if (msg.Contains("browse") || msg.Contains("web")) detectedTopic = "privacy";

            if (!string.IsNullOrEmpty(detectedTopic))
            {
                // Store the favourite topic in memory
                user.FavouriteTopic = detectedTopic;
                user.LastTopic = detectedTopic;

                return $"Great! I'll remember that you're interested in {detectedTopic}, {user.Name}. " +
                       $"It's a crucial part of staying safe online. 🛡️\n\n" +
                       $"Here's a tip on {detectedTopic} to get you started:\n\n" +
                       GetRandomTip(detectedTopic) +
                       $"\n\nWant to learn more? Just ask or type 'menu'!";
            }
            else
            {
                return $"That sounds interesting, {user.Name}! 😊 " +
                       "I specialise in cybersecurity topics. " +
                       "Try saying 'I'm interested in phishing' or 'I'm interested in passwords' " +
                       "and I'll remember it for our whole conversation!";
            }
        }

        // ============================================================
        // GET MENU - Shows all available topics
        // ============================================================
        private string GetMenu()
        {
            return "═══ Here's what I can teach you: ═══\n\n" +
                   "🎣 PHISHING      - Spot fake emails and scams\n" +
                   "🔑 PASSWORDS     - Create strong, safe passwords\n" +
                   "🔒 PRIVACY       - Protect your personal information\n" +
                   "⚠️ SCAMS         - Recognise and avoid scams\n" +
                   "🦠 VIRUSES       - Protect against malware\n" +
                   "🔗 LINKS         - Don't get tricked by fake links\n" +
                   "🌐 BROWSING      - Stay safe on the internet\n" +
                   "📋 REPORTING     - Where to report cybercrime in SA\n" +
                   "🛡️ CYBERSECURITY - What it is and why it matters\n\n" +
                   "Just type what you want to learn!\n" +
                   "Example: 'what is phishing' or 'give me a phishing tip'";
        }

        // ============================================================
        // NEW: PRIVACY RESPONSES (Requirement 2 - new keyword)
        // ============================================================
        private string GetPrivacyResponse(string msg, string name, User user)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! 🔒\n\nPRIVACY online means controlling who can see your personal information. " +
                       "It includes your name, ID number, location, photos, and browsing habits. " +
                       "Protecting your privacy means being careful what you share and with whom.\n\n" +
                       "Want a quick privacy tip? Just ask!";
            }

            if (msg.Contains("tip") || msg.Contains("how to") || msg.Contains("protect") || msg.Contains("safe"))
            {
                return $"Here's a privacy tip for you, {name}:\n\n" +
                       GetRandomTip("privacy") +
                       "\n\nType 'tell me more' for another tip!";
            }

            // Default privacy response with a random tip
            return $"Privacy is super important, {name}! 🔒\n\n" +
                   GetRandomTip("privacy") +
                   "\n\nYou can ask: 'What is privacy?', 'How to protect my privacy?', " +
                   "or 'Give me a privacy tip'";
        }

        // ============================================================
        // NEW: SCAM RESPONSES (Requirement 2 - new keyword)
        // ============================================================
        private string GetScamResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! ⚠️\n\nA SCAM is a dishonest scheme designed to trick you " +
                       "into giving away money or personal information. Common types include prize scams, " +
                       "romance scams, job scams, and fake investment opportunities.\n\n" +
                       "Want to know how to spot and avoid them?";
            }

            if (msg.Contains("tip") || msg.Contains("how to") || msg.Contains("avoid") || msg.Contains("spot"))
            {
                return $"Here's a scam awareness tip for you, {name}:\n\n" +
                       GetRandomTip("scam") +
                       "\n\nType 'tell me more' for another tip!";
            }

            return $"Scam awareness is essential, {name}! ⚠️\n\n" +
                   GetRandomTip("scam") +
                   "\n\nYou can ask: 'What is a scam?', 'How to spot a scam?', or 'Give me a scam tip'";
        }

        // ============================================================
        // NEW: VIRUS/MALWARE RESPONSES (Requirement 2 - new keyword)
        // ============================================================
        private string GetVirusResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! 🦠\n\nA COMPUTER VIRUS (or malware) is malicious software " +
                       "designed to damage your device, steal your data, or give hackers access to your system. " +
                       "Types include viruses, ransomware, spyware, and trojans.\n\n" +
                       "Want tips on how to protect yourself?";
            }

            if (msg.Contains("tip") || msg.Contains("protect") || msg.Contains("prevent"))
            {
                return $"Here's a virus protection tip for you, {name}:\n\n" +
                       GetRandomTip("virus") +
                       "\n\nType 'tell me more' for another tip!";
            }

            return $"Protecting against viruses is vital, {name}! 🦠\n\n" +
                   GetRandomTip("virus") +
                   "\n\nYou can ask: 'What is a virus?', 'How to protect against viruses?'";
        }

        // ============================================================
        // PHISHING RESPONSES (from Part 1, kept and enhanced)
        // ============================================================
        private string GetPhishingResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define") || msg.Contains("meaning"))
            {
                return $"Great question {name}! 🎣\n\nPHISHING is when scammers send fake emails pretending " +
                       "to be from real companies like banks, Netflix, or PayPal. They want you to click " +
                       "dangerous links or give away your passwords.\n\n" +
                       "Want to learn how to SPOT phishing emails? Just ask!";
            }

            if (msg.Contains("tip") || msg.Contains("give me"))
            {
                return $"Here's a phishing tip for you, {name}:\n\n" +
                       GetRandomTip("phishing") +
                       "\n\nType 'tell me more' for another tip!";
            }

            if (msg.Contains("spot") || msg.Contains("identify") || msg.Contains("recognize"))
            {
                return $"You're going to be a pro at this {name}! 🌟\n\nHOW TO SPOT PHISHING EMAILS:\n\n" +
                       "1️⃣ Check the sender's email — paypa1.com is FAKE, paypal.com is REAL\n" +
                       "2️⃣ Look for spelling mistakes — scammers have bad grammar\n" +
                       "3️⃣ Watch for URGENT words like 'ACT NOW!' or 'Account Suspended'\n" +
                       "4️⃣ Hover over links without clicking to see where they go\n" +
                       "5️⃣ If it sounds too good to be true — it's a scam!\n\n" +
                       "Real companies NEVER ask for passwords via email!";
            }

            if (msg.Contains("protect") || msg.Contains("prevent") || msg.Contains("avoid"))
            {
                return $"Smart question {name}! 🛡️\n\nHOW TO PROTECT YOURSELF FROM PHISHING:\n" +
                       "✅ NEVER click links in suspicious emails — type the address yourself\n" +
                       "✅ Use TWO-FACTOR AUTHENTICATION (2FA) for extra security\n" +
                       "✅ Report phishing to report@cybersecurity.gov.za\n" +
                       "✅ When in doubt, CALL the company on their official number";
            }

            if (msg.Contains("example") || msg.Contains("show me"))
            {
                return $"Here are real phishing examples {name}:\n\n" +
                       "🚨 Fake Bank: 'URGENT! Your account is locked! Click here to verify'\n" +
                       "🚨 Prize Winner: 'You won R10,000! Send your bank details'\n" +
                       "🚨 Fake Delivery: 'Your package is waiting. Track here: bit.ly/...'\n" +
                       "🚨 Netflix: 'Your account is suspended. Update payment now'\n\n" +
                       "Red flags: Urgent language, asks to click links, asks for personal info!";
            }

            // Default: random phishing tip
            return $"I'd love to teach you about phishing {name}! 🎣\n\n" +
                   "Quick tip:\n" + GetRandomTip("phishing") +
                   "\n\nYou can ask:\n• 'What is phishing?'\n• 'How to spot phishing?'\n" +
                   "• 'Give me a phishing tip'\n• 'Examples of phishing'";
        }

        // ============================================================
        // PASSWORD RESPONSES (from Part 1, kept and enhanced)
        // ============================================================
        private string GetPasswordResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! 🔑\n\nA PASSWORD is like a digital key to your accounts. " +
                       "Password SAFETY means creating keys that hackers can't guess. " +
                       "A weak password is like leaving your front door wide open!\n\n" +
                       "Want to learn how to create a strong one?";
            }

            if (msg.Contains("tip") || msg.Contains("give me"))
            {
                return $"Here's a password tip for you, {name}:\n\n" +
                       GetRandomTip("password") +
                       "\n\nType 'tell me more' for another tip!";
            }

            if (msg.Contains("create") || msg.Contains("make") || msg.Contains("strong") || msg.Contains("how to"))
            {
                return $"Let's make you a password superhero {name}! 🦸\n\nHOW TO CREATE A STRONG PASSWORD:\n\n" +
                       "✅ Make it LONG — 12+ characters minimum\n" +
                       "✅ Mix it UP — CAPITALS, lowercase, numbers, and symbols\n" +
                       "✅ Use a PASSPHRASE — 'Blue-Horse-Sunshine-Rain!'\n" +
                       "✅ Make it UNIQUE — different password for every account\n\n" +
                       "❌ BAD: 'password123' (hackers guess this in seconds)\n" +
                       "✅ GOOD: 'MyD0g!sC00l2024@'";
            }

            if (msg.Contains("2fa") || msg.Contains("two factor") || msg.Contains("two-factor"))
            {
                return $"Excellent question {name}! 🔐\n\nTWO-FACTOR AUTHENTICATION (2FA) adds an extra " +
                       "layer of security. Even if someone steals your password, they still can't log in " +
                       "without the second factor (usually a code sent to your phone).\n\n" +
                       "Enable 2FA on your email, banking, and social media — it's one of the best things you can do!";
            }

            return $"I'd love to teach you about passwords {name}! 🔑\n\n" +
                   "Quick tip:\n" + GetRandomTip("password") +
                   "\n\nYou can ask:\n• 'What is password safety?'\n• 'How to create a strong password?'\n" +
                   "• 'Give me a password tip'\n• 'What is 2FA?'";
        }

        // ============================================================
        // SUSPICIOUS LINKS RESPONSES (from Part 1, kept)
        // ============================================================
        private string GetLinkResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! 🔗\n\nA SUSPICIOUS LINK looks real but takes you to a FAKE " +
                       "website. Scammers use them to steal passwords or install viruses. " +
                       "It's like a sign that says 'BANK' but leads somewhere dangerous!\n\nWant to learn how to SPOT them?";
            }

            if (msg.Contains("spot") || msg.Contains("fake") || msg.Contains("identify"))
            {
                return $"You're going to be a link detective {name}! 🕵️\n\nHOW TO SPOT FAKE LINKS:\n\n" +
                       "1️⃣ HOVER over the link (DON'T click!) — see where it really goes\n" +
                       "2️⃣ Look for spelling tricks — faceb00k.com is FAKE\n" +
                       "3️⃣ Check for 'https://' — the 's' means secure\n" +
                       "4️⃣ Be suspicious of shortened links — bit.ly hides the real address\n" +
                       "5️⃣ Trust your gut — if something feels wrong, DON'T CLICK!";
            }

            return $"I'd love to teach you about suspicious links {name}! 🔗\n\n" +
                   "You can ask:\n• 'What are suspicious links?'\n• 'How to spot fake links?'\n" +
                   "• 'How to check if a link is safe?'";
        }

        // ============================================================
        // SAFE BROWSING RESPONSES (from Part 1, kept)
        // ============================================================
        private string GetBrowsingResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define"))
            {
                return $"Great question {name}! 🌐\n\nSAFE BROWSING means protecting yourself while using " +
                       "the internet. It's about knowing which websites are trustworthy and keeping " +
                       "your personal information private. Think of it like crossing a road — look both ways first!\n\n" +
                       "Want to learn HOW to browse safely?";
            }

            if (msg.Contains("check") || msg.Contains("safe") || msg.Contains("verify"))
            {
                return $"Let me teach you the website safety check {name}! ✅\n\nSAFETY CHECKLIST:\n" +
                       "🔒 Look for the PADLOCK in the address bar\n" +
                       "🔒 Check for 'https://' — not 'http://'\n" +
                       "🔒 Read the URL carefully — amaz0n.com is FAKE\n" +
                       "🔒 Does it look professional with no spelling errors?\n" +
                       "🔒 Google the website name + 'scam' or 'reviews'";
            }

            return $"I'd love to teach you about safe browsing {name}! 🌐\n\n" +
                   "You can ask:\n• 'What is safe browsing?'\n• 'How to check if a website is safe?'\n" +
                   "• 'What browser settings should I use?'";
        }

        // ============================================================
        // REPORTING RESPONSES (from Part 1, kept)
        // ============================================================
        private string GetReportingResponse(string msg, string name)
        {
            if (msg.Contains("where") || (msg.Contains("report") && msg.Contains("phish")))
            {
                return $"You're doing the right thing {name}! 📋\n\nWHERE TO REPORT IN SOUTH AFRICA:\n" +
                       "📧 Government: report@cybersecurity.gov.za\n" +
                       "🏦 Your Bank — call their fraud department immediately\n" +
                       "📞 SABRIC (banking scams): 0861 022 339\n\n" +
                       "Keep the evidence — don't delete the emails!";
            }

            if (msg.Contains("saps") || msg.Contains("police"))
            {
                return $"Reporting to SAPS is important {name}! 🚔\n\n" +
                       "📞 Crime Stop: 0860 010 111 (24/7, can be anonymous)\n" +
                       "🏢 Visit your local police station — ask for the Cybercrime Unit\n" +
                       "📂 Bring ALL evidence: emails, screenshots, bank statements\n\n" +
                       "They'll give you a case number. Don't be scared — they're there to help!";
            }

            if (msg.Contains("scammed") || msg.Contains("victim") || msg.Contains("what to do"))
            {
                return $"I'm sorry if this happened to you {name}. 💙\n\nWHAT TO DO IF SCAMMED:\n" +
                       "1️⃣ DON'T PANIC — stay calm\n" +
                       "2️⃣ CHANGE all passwords immediately\n" +
                       "3️⃣ CALL YOUR BANK — freeze accounts, report fraud\n" +
                       "4️⃣ REPORT TO SAPS — get a case number\n" +
                       "5️⃣ RUN ANTIVIRUS scan on your device\n\n" +
                       "Acting fast is what matters most. You've got this!";
            }

            return $"I'd love to teach you about reporting {name}! 📋\n\n" +
                   "You can ask:\n• 'Where to report phishing?'\n• 'How to report to SAPS?'\n" +
                   "• 'What to do after being scammed?'";
        }

        // ============================================================
        // CYBERSECURITY GENERAL RESPONSES (from Part 1, kept)
        // ============================================================
        private string GetCyberResponse(string msg, string name)
        {
            if (msg.Contains("what is") || msg.Contains("define") || msg.Contains("meaning"))
            {
                return $"Great question {name}! 🛡️\n\nCYBERSECURITY is protecting yourself, your devices, " +
                       "and your information from online threats. It's like having:\n" +
                       "🔒 A lock on your digital door (passwords)\n" +
                       "👮 A security guard (antivirus)\n" +
                       "📚 Training to spot danger (awareness — like what you're doing now!)\n\n" +
                       "Want to learn WHY it's so important?";
            }

            if (msg.Contains("why") || msg.Contains("important") || msg.Contains("matter"))
            {
                return $"This is so important {name}! 🌍\n\nIn South Africa, cyber attacks increased 200% recently!\n\n" +
                       "Cybersecurity protects:\n" +
                       "💰 YOUR MONEY — bank accounts and cards\n" +
                       "🪪 YOUR IDENTITY — personal information\n" +
                       "📱 YOUR DEVICES — computer and phone\n" +
                       "📸 YOUR PRIVACY — photos and messages\n\n" +
                       "By learning this, you're already safer than most people!";
            }

            if (msg.Contains("common") || msg.Contains("threats") || msg.Contains("types"))
            {
                return $"Let me show you what to watch for {name}! 👀\n\nCOMMON CYBER THREATS:\n" +
                       "🎣 PHISHING — Fake emails that steal info\n" +
                       "🦠 MALWARE — Viruses on your device\n" +
                       "💰 RANSOMWARE — Hackers lock your files and demand payment\n" +
                       "🪪 IDENTITY THEFT — Stealing your personal info\n" +
                       "🎭 SOCIAL ENGINEERING — Psychologically tricking you\n\n" +
                       "Now you know what to watch for!";
            }

            if (msg.Contains("protect") || msg.Contains("safe") || msg.Contains("habits"))
            {
                return $"Ready to be a cybersecurity superhero {name}? 🦸\n\nDAILY SAFETY HABITS:\n" +
                       "1️⃣ Use STRONG, UNIQUE passwords everywhere\n" +
                       "2️⃣ Turn on TWO-FACTOR AUTHENTICATION (2FA)\n" +
                       "3️⃣ Keep EVERYTHING UPDATED — updates fix security holes\n" +
                       "4️⃣ THINK before you click — if suspicious, don't click!\n" +
                       "5️⃣ BACK UP important files regularly\n" +
                       "6️⃣ SHARE what you learn with family and friends!";
            }

            return $"I'd love to teach you about cybersecurity {name}! 🛡️\n\n" +
                   "You can ask:\n• 'What is cybersecurity?'\n• 'Why is cybersecurity important?'\n" +
                   "• 'What are common cyber threats?'\n• 'How to stay protected?'";
        }
    }
}
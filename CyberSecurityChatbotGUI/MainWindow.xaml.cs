using System;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CyberSecurityChatbot_1
{
    // ================================================================
    // MAINWINDOW CODE-BEHIND
    //
    // ANIMATIONS ADDED IN THIS VERSION:
    //   1. Spinning "thinking" bubble — shows ◐◓◑◒ while bot prepares reply
    //   2. 3-second delay  — bot waits before answering (feels natural)
    //   3. Typing effect   — response revealed one character at a time
    //   4. Input is locked while the bot is thinking or typing
    //
    // HOW IT WORKS (simple explanation):
    //   - DispatcherTimer  = a clock that fires code repeatedly on the UI thread
    //   - async / await    = "wait here WITHOUT freezing the window"
    //   - Task.Delay(3000) = wait 3 seconds (non-blocking)
    // ================================================================
    public partial class MainWindow : Window
    {
        // --------------------------------------------------------
        // FIELDS
        // --------------------------------------------------------
        private User _user;
        private Chatbot _chatbot;

        // Fires every 1 second — updates session clock in header
        private DispatcherTimer _sessionTimer;

        // Fires every 200 ms — cycles the spinner character ◐ ◓ ◑ ◒
        private DispatcherTimer _spinnerTimer;

        // Fires every 25 ms — reveals the next character of the bot reply
        private DispatcherTimer _typingTimer;

        private bool _nameEntered = false;

        // True while bot is thinking or typing — blocks new input
        private bool _isBotBusy = false;

        // ── TYPING STATE ──
        private string _fullResponseText = "";  // Complete text to type out
        private int _typingIndex = 0;   // How many chars revealed so far
        private TextBlock _typingTextBlock = null; // The TextBlock we type into

        // ── SPINNER STATE ──
        private TextBlock _spinnerLabel = null;  // Updated by spinner timer
        private Border _thinkingBubble = null;  // Removed after 3-second wait
        private int _spinnerFrame = 0;

        // The 4 frames of the spinner animation — cycles round and round
        private static readonly string[] SpinnerFrames = { "◐", "◓", "◑", "◒" };

        // ================================================================
        // CONSTRUCTOR
        // ================================================================
        public MainWindow()
        {
            InitializeComponent();

            _chatbot = new Chatbot();
            _user = new User("");

            // Session clock — fires every 1 second
            _sessionTimer = new DispatcherTimer();
            _sessionTimer.Interval = TimeSpan.FromSeconds(1);
            _sessionTimer.Tick += SessionTimer_Tick;
            _sessionTimer.Start();

            // Spinner timer — fires every 200 ms, only active during "thinking"
            _spinnerTimer = new DispatcherTimer();
            _spinnerTimer.Interval = TimeSpan.FromMilliseconds(200);
            _spinnerTimer.Tick += SpinnerTimer_Tick;

            // Typing timer — fires every 25 ms, only active while typing
            _typingTimer = new DispatcherTimer();
            _typingTimer.Interval = TimeSpan.FromMilliseconds(25);
            _typingTimer.Tick += TypingTimer_Tick;

            PlayVoiceGreeting();
            ShowWelcomeMessage();
        }

        // ================================================================
        // VOICE GREETING — plays WAV file on startup (Requirement 1)
        // ================================================================
        private void PlayVoiceGreeting()
        {
            try
            {
                SoundPlayer player = new SoundPlayer(
                    @"C:\Users\Student\source\repos\CyberSecurityChatbot_1\CyberSecurityChatbot_1\Chatbot.wav");
                player.Play(); // Play() is async so it doesn't freeze the window
            }
            catch { /* Non-critical — program continues if file not found */ }
        }

        // ================================================================
        // WELCOME MESSAGE — shown instantly on startup (no typing delay)
        // ================================================================
        private void ShowWelcomeMessage()
        {
            AddBotMessageInstant(
                "╔════════════════════════════════════╗\n" +
                "║     SIYA'S CYBER CHATBOT           ║\n" +
                "║     Stay Safe Online! 🛡️            ║\n" +
                "╚════════════════════════════════════╝\n\n" +
                "Welcome! I'm here to help you stay safe online.\n\n" +
                "Before we start — what's your name?"
            );
        }

        // ================================================================
        // SESSION TIMER TICK — updates header stats every second
        // ================================================================
        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            if (_nameEntered)
            {
                txtSessionTime.Text = $"⏱  Session: {_user.GetSessionTime()}";
                txtQuestionCount.Text = $"💬 Questions: {_user.QuestionCount}";

                if (!string.IsNullOrEmpty(_user.FavouriteTopic))
                    txtMemoryIndicator.Text = $"🧠 Remembering: {_user.FavouriteTopic}";
            }
        }

        // ================================================================
        // SPINNER TIMER TICK — cycles ◐ ◓ ◑ ◒ every 200 ms
        //
        // The modulo (%) operator wraps the frame back to 0 after frame 3.
        // Example: 0 → 1 → 2 → 3 → 0 → 1 → 2 → 3 → ...
        // ================================================================
        private void SpinnerTimer_Tick(object sender, EventArgs e)
        {
            _spinnerFrame = (_spinnerFrame + 1) % SpinnerFrames.Length;

            if (_spinnerLabel != null)
                _spinnerLabel.Text = $"{SpinnerFrames[_spinnerFrame]}  Bot is thinking...";
        }

        // ================================================================
        // TYPING TIMER TICK — reveals one more character every 25 ms
        //
        // _typingIndex tracks how many characters have been shown.
        // Each tick we increase it by 1 and update the TextBlock text.
        // When _typingIndex reaches the end of the string, we stop.
        // ================================================================
        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            if (_typingIndex < _fullResponseText.Length)
            {
                // Reveal one more character using Substring
                _typingIndex++;
                _typingTextBlock.Text = _fullResponseText.Substring(0, _typingIndex);
                ScrollToBottom();
            }
            else
            {
                // All characters shown — stop timer and re-enable input
                _typingTimer.Stop();
                _isBotBusy = false;
                txtInput.IsEnabled = true;
                btnSend.IsEnabled = true;
                txtInput.Focus();
                ScrollToBottom();
            }
        }

        // ================================================================
        // BUTTON & KEYBOARD HANDLERS
        // ================================================================
        private void btnSend_Click(object sender, RoutedEventArgs e) => ProcessInput();

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ProcessInput();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            // Stop any running animation before wiping the chat
            _spinnerTimer.Stop();
            _typingTimer.Stop();
            _isBotBusy = false;
            ChatPanel.Children.Clear();
            EnableInput();
            AddBotMessageInstant("Chat cleared! 🗑️  How can I help you, " +
                                 (_nameEntered ? _user.Name : "friend") + "?");
        }

        private void TopicButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            string query = "";
            if (btn == btnPhishing) query = "Give me a phishing tip";
            if (btn == btnPasswords) query = "Give me a password tip";
            if (btn == btnPrivacy) query = "Give me a privacy tip";
            if (btn == btnScams) query = "Give me a scam tip";
            if (btn == btnViruses) query = "Give me a virus tip";
            if (btn == btnLinks) query = "How to spot fake links?";
            if (btn == btnMenu) query = "menu";

            if (!string.IsNullOrEmpty(query))
            {
                txtInput.Text = query;
                ProcessInput();
            }
        }

        // ================================================================
        // PROCESS INPUT — the heart of the chatbot interaction
        //
        // ANIMATION FLOW:
        //   User sends message
        //     → Show user bubble immediately
        //     → Lock input (_isBotBusy = true)
        //     → Show spinning "◐ Bot is thinking..." bubble
        //     → await Task.Delay(3000)  ← 3-second pause, window stays live
        //     → Remove thinking bubble
        //     → Create empty bot bubble
        //     → Start typing timer → reveals response char by char
        //     → When done: unlock input
        // ================================================================
        private async void ProcessInput()
        {
            // Ignore clicks/Enter while bot is already busy
            if (_isBotBusy) return;

            string input = txtInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                txtStatus.Text = "⚠️  Please type a message before sending!";
                return;
            }

            txtStatus.Text = "💡 Tip: Type 'menu' for all topics | Say 'tell me more' for follow-up tips";

            // Show the user bubble right away
            AddUserMessage(input);
            txtInput.Clear();

            // ── NAME COLLECTION (first message only) ──
            if (!_nameEntered)
            {
                if (!IsValidName(input))
                {
                    AddBotMessageInstant(
                        "Please enter a real name using only letters — no numbers or symbols! 😊\n\nWhat's your name?");
                    return;
                }

                string cleanName = CapitaliseName(input);
                _user = new User(cleanName);
                _nameEntered = true;

                txtSessionTime.Text = $"⏱  Session: {_user.GetSessionTime()}";
                txtQuestionCount.Text = "💬 Questions: 0";

                // Name confirmation gets the full thinking + typing animation
                await ShowBotResponseAnimated(
                    $"Hello {_user.Name}! 👋 Welcome to the Cybersecurity Chatbot!\n\n" +
                    "I can teach you about:\n" +
                    "🎣 PHISHING      — Spot fake emails and scams\n" +
                    "🔑 PASSWORDS     — Create strong passwords\n" +
                    "🔒 PRIVACY       — Protect your personal information\n" +
                    "⚠️ SCAMS         — Recognise and avoid scams\n" +
                    "🦠 VIRUSES       — Protect against malware\n" +
                    "🔗 LINKS         — Don't get tricked by fake links\n" +
                    "📋 REPORTING     — Report cybercrime in South Africa\n\n" +
                    "Type 'menu' for the full list, or just ask me anything!\n" +
                    "You can also say 'I'm interested in privacy' and I'll remember it! 🧠"
                );
                return;
            }

            // ── EXIT COMMAND ──
            if (input.ToLower() == "exit" || input.ToLower() == "quit" || input.ToLower() == "bye")
            {
                await ShowBotResponseAnimated(
                    $"Goodbye {_user.Name}! 👋 Stay safe online! 🛡️\n\n" +
                    $"Session summary:\n" +
                    $"⏱  Time spent: {_user.GetSessionTime()}\n" +
                    $"💬 Questions asked: {_user.QuestionCount}\n" +
                    (string.IsNullOrEmpty(_user.FavouriteTopic) ? "" :
                        $"🧠 Favourite topic: {_user.FavouriteTopic}\n") +
                    "\nCome back anytime! Knowledge is your best defence. 🔐"
                );
                txtInput.IsEnabled = false;
                btnSend.IsEnabled = false;
                _sessionTimer.Stop();
                return;
            }

            // ── NORMAL QUESTION ──
            _user.IncrementQuestions();
            string response = _chatbot.GetResponse(input, _user);
            await ShowBotResponseAnimated(response);
        }

        // ================================================================
        // SHOW BOT RESPONSE ANIMATED
        //
        // This method has 3 phases:
        //   Phase 1 — ShowThinkingBubble()    : spinner appears instantly
        //   Phase 2 — await Task.Delay(3000)  : 3-second non-blocking wait
        //   Phase 3 — StartTypingResponse()   : typing timer takes over
        //
        // 'async Task' means this method can be awaited by ProcessInput.
        // 'await Task.Delay' means "pause here for 3 seconds, but keep
        //  the window alive so the spinner and scrolling still work".
        // ================================================================
        private async Task ShowBotResponseAnimated(string response)
        {
            // Lock input — user cannot send while bot is busy
            _isBotBusy = true;
            txtInput.IsEnabled = false;
            btnSend.IsEnabled = false;

            // PHASE 1: Show the thinking bubble with spinner
            ShowThinkingBubble();

            // PHASE 2: Non-blocking 3-second wait
            // During this time the spinner timer is cycling the ◐◓◑◒ icon
            await Task.Delay(3000);

            // PHASE 3: Remove the thinking bubble and start typing
            RemoveThinkingBubble();
            StartTypingResponse(response);

            // Input stays locked — TypingTimer_Tick unlocks it when typing finishes
        }

        // ================================================================
        // SHOW THINKING BUBBLE
        //
        // Creates a chat bubble with a spinner TextBlock inside it,
        // then starts the spinner timer to animate it.
        // ================================================================
        private void ShowThinkingBubble()
        {
            _thinkingBubble = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#161B22")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                BorderThickness = new Thickness(3, 0, 0, 0),
                CornerRadius = new CornerRadius(0, 8, 8, 8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 6, 80, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 400
            };

            StackPanel content = new StackPanel();

            // Static "🤖 Bot" label at the top
            TextBlock botLabel = new TextBlock
            {
                Text = "🤖  Bot",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                Margin = new Thickness(0, 0, 0, 4)
            };

            // This is the label the SpinnerTimer_Tick updates every 200 ms
            // It starts on frame 0 (◐) and cycles through all 4 frames
            _spinnerLabel = new TextBlock
            {
                Text = "◐  Bot is thinking...",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B949E")),
            };

            content.Children.Add(botLabel);
            content.Children.Add(_spinnerLabel);
            _thinkingBubble.Child = content;

            ChatPanel.Children.Add(_thinkingBubble);
            ScrollToBottom();

            // Reset frame counter and start the spinner animation
            _spinnerFrame = 0;
            _spinnerTimer.Start();
        }

        // ================================================================
        // REMOVE THINKING BUBBLE — called after the 3-second wait ends
        // Stops the spinner timer and removes the bubble from the chat
        // ================================================================
        private void RemoveThinkingBubble()
        {
            _spinnerTimer.Stop();

            if (_thinkingBubble != null && ChatPanel.Children.Contains(_thinkingBubble))
                ChatPanel.Children.Remove(_thinkingBubble);

            _thinkingBubble = null;
            _spinnerLabel = null;
        }

        // ================================================================
        // START TYPING RESPONSE
        //
        // Creates a bot bubble with an EMPTY TextBlock (_typingTextBlock).
        // The TypingTimer_Tick method fills it in one character at a time.
        //
        // Why start empty?
        //   Because we need a reference to the TextBlock so the timer
        //   can update it. We store it in _typingTextBlock and set
        //   _fullResponseText so the timer knows what to type.
        // ================================================================
        private void StartTypingResponse(string response)
        {
            // Store the full text and reset the index to 0
            _fullResponseText = response;
            _typingIndex = 0;

            // Build the bot bubble
            Border bubble = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#161B22")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                BorderThickness = new Thickness(3, 0, 0, 0),
                CornerRadius = new CornerRadius(0, 8, 8, 8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 6, 80, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 680
            };

            StackPanel content = new StackPanel();

            TextBlock label = new TextBlock
            {
                Text = "🤖  Bot",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                Margin = new Thickness(0, 0, 0, 4)
            };

            // STARTS EMPTY — TypingTimer_Tick fills this in char by char
            _typingTextBlock = new TextBlock
            {
                Text = "",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6EDF3")),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            content.Children.Add(label);
            content.Children.Add(_typingTextBlock);
            bubble.Child = content;

            ChatPanel.Children.Add(bubble);

            // Hand off to the typing timer — it will reveal the text
            _typingTimer.Start();
        }

        // ================================================================
        // ADD BOT MESSAGE INSTANT
        // For the welcome message and clear confirmation — no animation
        // ================================================================
        private void AddBotMessageInstant(string message)
        {
            Border bubble = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#161B22")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                BorderThickness = new Thickness(3, 0, 0, 0),
                CornerRadius = new CornerRadius(0, 8, 8, 8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 6, 80, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 680
            };

            StackPanel content = new StackPanel();

            TextBlock label = new TextBlock
            {
                Text = "🤖  Bot",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00C8FF")),
                Margin = new Thickness(0, 0, 0, 4)
            };

            TextBlock text = new TextBlock
            {
                Text = message,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6EDF3")),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            content.Children.Add(label);
            content.Children.Add(text);
            bubble.Child = content;

            ChatPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        // ================================================================
        // ADD USER MESSAGE — right-aligned red-border bubble
        // ================================================================
        private void AddUserMessage(string message)
        {
            string displayName = _nameEntered ? _user.Name : "You";

            Border bubble = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#21262D")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7B72")),
                BorderThickness = new Thickness(0, 0, 3, 0),
                CornerRadius = new CornerRadius(8, 0, 8, 8),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(80, 6, 0, 6),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 680
            };

            StackPanel content = new StackPanel();

            TextBlock label = new TextBlock
            {
                Text = $"👤  {displayName}",
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7B72")),
                Margin = new Thickness(0, 0, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            TextBlock text = new TextBlock
            {
                Text = message,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6EDF3")),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            content.Children.Add(label);
            content.Children.Add(text);
            bubble.Child = content;

            ChatPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        // ================================================================
        // HELPERS
        // ================================================================
        private void EnableInput()
        {
            _isBotBusy = false;
            txtInput.IsEnabled = true;
            btnSend.IsEnabled = true;
            txtInput.Focus();
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToEnd();
        }

        private bool IsValidName(string name)
        {
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name)) return false;
            foreach (char c in name)
                if (!char.IsLetter(c) && c != ' ') return false;
            return true;
        }

        private string CapitaliseName(string name)
        {
            string[] words = name.Trim().Split(' ');
            for (int i = 0; i < words.Length; i++)
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            return string.Join(" ", words);
        }
    }
}
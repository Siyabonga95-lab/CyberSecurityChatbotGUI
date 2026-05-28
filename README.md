# Siya's Cyber Security Chatbot GUI

A **Windows Presentation Foundation (WPF)** cybersecurity awareness chatbot built in C# (.NET Framework). The chatbot educates users about common online threats through an interactive, animated GUI — helping South Africans stay safe online.

---

## 📸 Preview

```
╔════════════════════════════════════╗
║     SIYA'S CYBER CHATBOT           ║
║     Stay Safe Online! 🛡️            ║
╚════════════════════════════════════╝
```

---

##  Features

###  GUI Design (WPF)
- Dark cybersecurity-themed interface (navy, cyan, green accents)
- Chat bubble layout — bot messages on the left, user messages on the right
- Live session timer and question counter in the header
- Quick-access topic buttons for one-click learning
- ASCII art header translated from the original console app

###  Animated Responses
- **Spinning thinking indicator** — `◐ ◓ ◑ ◒` cycles while the bot prepares a reply
- **3-second thinking delay** — makes the conversation feel natural
- **Typing effect** — the bot's response is revealed one character at a time
- Input is locked while the bot is busy to prevent double-sends

###  Keyword Recognition
Recognises **6 cybersecurity topics** from natural language input:

| Keyword | Topic |
|---|---|
| `phish`, `scam email`, `fake email` | Phishing |
| `password`, `login`, `2fa` | Passwords & Authentication |
| `privacy`, `personal info` | Online Privacy |
| `scam`, `fraud`, `trick` | Scam Awareness |
| `virus`, `malware`, `ransomware` | Viruses & Malware |
| `link`, `url`, `click` | Suspicious Links |

###  Random Responses
Each topic has **5 different tip variations** stored in a `Dictionary<string, string[]>`. The bot randomly selects one each time so responses never feel repetitive.

###  Conversation Flow
Follow-up phrases keep the conversation going without restarting:
- `"tell me more"` → gives another tip on the last topic
- `"give me another tip"` → same topic, different response
- `"explain more"` / `"more details"` → continues current subject

###  Memory & Recall
Say `"I'm interested in privacy"` and the bot **remembers your favourite topic** for the whole session. It shows a memory indicator in the header (`🧠 Remembering: privacy`) and personalises future responses around it.

###  Sentiment Detection
The bot detects emotions and responds empathetically **before** giving a tip — so the user never has to ask twice:

| Emotion | Trigger words | Bot response style |
|---|---|---|
| Worried | `worried`, `scared`, `afraid` | Reassuring + tip |
| Curious | `curious`, `wondering` | Enthusiastic + tip |
| Frustrated | `frustrated`, `confused`, `annoyed` | Calming + simple tip |
| Happy | `happy`, `excited` | Energetic + tip |
| Tired | `tired`, `overwhelmed` | Gentle + one quick tip |

###  Error Handling
- Empty input is blocked at both the GUI and chatbot level
- Unrecognised input returns a helpful fallback with suggested topics
- Voice greeting failure is caught silently — the app always continues
- No crashes on unexpected input

---

##  Project Structure

```
CyberSecurityChatbot_1/
│
├── User.cs                 # Stores user name, question count, favourite topic, last topic
├── Chatbot.cs              # All response logic — keywords, random tips, sentiment, memory
├── MainWindow.xaml         # WPF GUI layout — colours, bubbles, buttons, input bar
├── MainWindow.xaml.cs      # Code-behind — animations, timers, input handling
└── Chatbot.wav             # Voice greeting played on startup
```

---

##  Getting Started

### Prerequisites
- Windows OS
- Visual Studio 2019 or later
- .NET Framework 4.7.2 or later
- WPF workload installed in Visual Studio

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Siyabonga95-lab/CyberSecurityChatbotGUI.git
   ```

2. **Open in Visual Studio**
   - Open `CyberSecurityChatbot_1.sln`

3. **Check the WAV file path**
   In `MainWindow.xaml.cs`, update this line to match your machine if needed:
   ```csharp
   SoundPlayer player = new SoundPlayer(
       @"C:\Users\Student\source\repos\CyberSecurityChatbot_1\CyberSecurityChatbot_1\Chatbot.wav");
   ```

4. **Run the project**
   - Press `F5` or click **Start** in Visual Studio

---

##  How to Use

| Action | What to do |
|---|---|
| Start chatting | Type your name and press **Enter** or click **SEND** |
| Ask about a topic | Type naturally, e.g. `"what is phishing?"` or `"give me a password tip"` |
| Quick topics | Click any of the shortcut buttons at the bottom |
| Follow up | Type `"tell me more"` or `"give me another tip"` |
| Set favourite topic | Say `"I'm interested in privacy"` |
| See all topics | Type `"menu"` |
| Exit | Type `"exit"`, `"quit"`, or `"bye"` |
| Clear chat | Click the **CLEAR** button |

---

##  Cybersecurity Topics Covered

-  **Phishing** — How to spot and avoid fake emails
-  **Passwords** — Creating strong passwords and using 2FA
-  **Privacy** — Protecting personal information online
-  **Scams** — Recognising common scam tactics
-  **Viruses & Malware** — Protecting your devices
-  **Suspicious Links** — How to check URLs before clicking
-  **Safe Browsing** — Staying safe on the internet
-  **Reporting** — Where to report cybercrime in South Africa (SAPS, SABRIC, cybersecurity.gov.za)

---

##  Technical Highlights

- **OOP design** — `User` and `Chatbot` are separate classes with single responsibilities
- **`async`/`await`** — 3-second thinking delay that keeps the UI fully responsive
- **`DispatcherTimer`** — drives the spinner animation (every 200ms) and typing effect (every 25ms)
- **`Dictionary<string, string[]>`** — organises random tips by topic for clean, expandable code
- **WPF data binding** — UI elements updated live via named controls (`txtSessionTime`, `txtMemoryIndicator`)

---

## 🇿🇦 South African Context

This chatbot is built with South African users in mind:
- Reports scams to **SABRIC**: `0861 022 339`
- Refers cybercrime to **SAPS Crime Stop**: `0860 010 111`
- Uses the national cybersecurity email: `report@cybersecurity.gov.za`
- References local statistics on the rise of cyber attacks in South Africa

---

##  License

This project was created for educational purposes as part of a software development assignment.

---

## Author

**Siyabonga** — [@Siyabonga95-lab](https://github.com/Siyabonga95-lab)

> *"Knowledge is your best defence online."* 

# Outlook AI Assistant

## AI-powered email productivity for Microsoft Outlook

Outlook AI Assistant is a VSTO add-in for Outlook (2016+ / Microsoft 365) that dramatically reduces time spent reading, categorizing, composing, and responding to emails using AI. It works with multiple AI providers — including on-premises options — and gives users full control over their data and API keys.

---

## ✨ Features

### Core Capabilities
- **AI Summarization** — Get concise 2-3 sentence summaries of any email
- **Smart Reply Drafting** — AI generates professional, context-aware replies
- **Email Classification** — Auto-categorize emails (Work, Personal, Meeting, Support, etc.)
- **Translation** — Translate emails between languages
- **Proofreading** — Grammar and style checking for outgoing emails

### Advanced Features (Pro / Business)
- **Smart Rules Engine** — Create rules: "If email is from X and contains Y, then Z"
- **Automation Pipeline** — Auto-categorize, auto-reply, and auto-route emails
- **Trust Scoring** — Sender reputation-based automation decisions
- **Multiple Accounts** — Manage multiple Outlook accounts

### Privacy & Security
- **Multiple AI Providers** — OpenAI, DeepSeek, Gemini, Anthropic, Ollama, LM Studio, OpenRouter, any OpenAI-compatible API
- **On-Premises AI** — Use local models via Ollama or LM Studio (data never leaves your network)
- **PII Anonymization** — Optional PII stripping before cloud API calls
- **DPAPI Encryption** — API keys stored encrypted with Windows DPAPI

---

## 🏗️ Solution Architecture

```
OutlookAIAssistant.sln
├── OutlookAIAssistant.Core/           # Shared interfaces, models, enums
├── OutlookAIAssistant.AIEngine/        # AI providers, caching, prompt management
├── OutlookAIAssistant.OutlookModule/  # VSTO add-in, Ribbon, event handlers, sidebar
├── OutlookAIAssistant.Rules/          # Rules engine, templates, automation pipeline
├── OutlookAIAssistant.Security/       # Encryption, licensing, privacy management
├── OutlookAIAssistant.UI/            # WinForms dialogs for configuration
├── OutlookAIAssistant.Tests/         # NUnit unit tests
└── OutlookAIAssistant.Setup/         # MSI installer project
```

### Project Dependencies
```
OutlookAIAssistant.OutlookModule
  ├── OutlookAIAssistant.Core
  ├── OutlookAIAssistant.AIEngine
  ├── OutlookAIAssistant.Rules
  └── OutlookAIAssistant.Security

OutlookAIAssistant.AIEngine
  └── OutlookAIAssistant.Core

OutlookAIAssistant.Rules
  └── OutlookAIAssistant.Core

OutlookAIAssistant.UI
  └── OutlookAIAssistant.Core

OutlookAIAssistant.Tests
  ├── OutlookAIAssistant.Core
  ├── OutlookAIAssistant.AIEngine
  ├── OutlookAIAssistant.Rules
  └── OutlookAIAssistant.Security
```

---

## 🛠️ Prerequisites

- **Visual Studio 2022** (or 2019) with Office/SharePoint development workload
- **.NET Framework 4.8** Developer Pack
- **Outlook 2016, 2019, 2021, or Microsoft 365 Desktop**
- NuGet packages (restored automatically):
  - `Newtonsoft.Json` 13.0.3
  - `NUnit` 4.0.1 (tests only)

---

## 🚀 Getting Started

1. **Clone or open the solution**
   ```
   git clone <repo-url>
   cd OutlookAIAssistant
   ```

2. **Open in Visual Studio**
   - Open `OutlookAIAssistant.sln`

3. **Restore NuGet packages**
   - Right-click solution → "Restore NuGet Packages"

4. **Build the solution**
   - Build → Build Solution (Ctrl+Shift+B)

5. **Run the add-in**
   - Set `OutlookAIAssistant.OutlookModule` as startup project
   - Press F5 to run with Outlook

6. **Configure an AI provider**
   - In Outlook, click the "AI Assistant" tab
   - Click "Settings" → "AI Providers" tab → "Add Provider"
   - Available providers: OpenAI, DeepSeek, Gemini, Anthropic, Ollama, LM Studio, OpenRouter

---

## 🔌 AI Providers

| Provider | Type | Default Endpoint | Models |
|----------|------|------------------|--------|
| OpenAI | Cloud | api.openai.com | gpt-4o, gpt-4, gpt-3.5-turbo |
| DeepSeek | Cloud | api.deepseek.com | deepseek-chat, deepseek-coder |
| Gemini | Cloud | generativelanguage.googleapis.com | gemini-pro, gemini-1.5-pro |
| Anthropic | Cloud | api.anthropic.com | claude-3-opus, claude-3-sonnet |
| Ollama | On-Premises | localhost:11434 | llama3, mistral, any local model |
| LM Studio | On-Premises | localhost:1234 | Any loaded model |
| OpenRouter | Cloud | openrouter.ai | Multiple models via one API |
| Custom | Any | Any OpenAI-compatible endpoint | Any compatible model |

---

## 💰 Licensing Plans

| Feature | Free | Personal ($49/yr) | Pro ($99/yr) | Business ($299+/yr) |
|---------|------|-------------------|--------------|---------------------|
| AI Summaries | 10/day | 100/day | Unlimited | Unlimited |
| AI Replies | ✓ | ✓ | ✓ | ✓ |
| Translation | - | ✓ | ✓ | ✓ |
| Proofreading | ✓ | ✓ | ✓ | ✓ |
| Classification | ✓ | ✓ | ✓ | ✓ |
| Smart Rules | - | - | ✓ | ✓ |
| Automation Pipeline | - | - | ✓ | ✓ |
| Multiple Accounts | - | - | ✓ | ✓ |
| Multiple Providers | - | - | ✓ | ✓ |
| CRM/ERP Integration | - | - | - | ✓ |
| Custom Tools | - | - | - | ✓ |
| Dedicated Support | - | - | - | ✓ |

---

## 🧪 Running Tests

Tests use NUnit. Run from Visual Studio:
- Test → Run All Tests (Ctrl+R, A)
- Or via console: `dotnet test`

---

## 📁 Project Structure Reference

| Directory | Purpose |
|-----------|---------|
| `Core/Interfaces/` | Service contracts (IAIEngine, IRulesEngine, ILicensingService, etc.) |
| `Core/Models/` | Data models (EmailMessage, AIRequest, AIResponse, etc.) |
| `Core/Enums/` | Enumerations (ClassificationType, AutomationLevel) |
| `AIEngine/Providers/` | AI provider implementations |
| `AIEngine/Cache/` | In-memory response caching |
| `AIEngine/Prompts/` | AI prompt templates |
| `AIEngine/Scoring/` | Sender trust scoring |
| `AIEngine/Privacy/` | PII filtering and privacy controls |
| `OutlookModule/EventHandlers/` | Outlook event wiring |
| `OutlookModule/Sidebar/` | WPF sidebar for AI interactions |
| `Rules/` | Rules engine, templates, automation pipeline |
| `Security/` | DPAPI encryption, licensing, privacy management |
| `UI/Forms/` | WinForms settings dialogs |
| `UI/Controls/` | Reusable UI controls |

---

## 🔒 Security & Privacy

- **API keys** are encrypted with Windows DPAPI and stored locally
- **On-premises mode** processes everything locally — no data sent to cloud
- **Anonymized mode** strips PII before sending to cloud providers
- **Privacy modes**: Standard, Anonymized, On-Premises
- All encryption uses Windows built-in `ProtectedData` API

---

## 🤝 Contributing

1. Create a feature branch from `main`
2. Follow the existing code style and project structure
3. Add tests for new functionality
4. Submit a pull request for review

---

## 📄 License

Proprietary — see licensing documentation for details.

---

## ⚙️ Tech Stack

- **Language:** VB.NET (.NET Framework 4.8)
- **Framework:** VSTO (Visual Studio Tools for Office)
- **UI:** WinForms (settings), WPF (sidebar)
- **Testing:** NUnit
- **Packages:** Newtonsoft.Json, System.Data.SQLite
- **Target:** Outlook 2016+ / Microsoft 365 Desktop
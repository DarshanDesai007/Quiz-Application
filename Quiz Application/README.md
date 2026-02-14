# 🎯 Quiz Application

A full-stack web-based quiz application built with **ASP.NET Core 8 MVC**, **Entity Framework Core**, and **SQL Server**. Features a premium dark-themed UI with glassmorphism effects, randomized questions, real-time validation, and automatic scoring.

---

## 📸 Features

- **5-Question Quiz** — Randomly selects 5 questions from a pool of 20 for each session
- **Random Order** — Questions are shuffled every time using Fisher-Yates algorithm
- **All Questions Mandatory** with type-specific validation (client + server)
- **Upsert Answers** — Navigate freely, change answers anytime
- **Auto-Scoring** — Instant results only for the attempted questions
- **Reliable Saving** — Navigation blocked until answer is successfully saved
- **Session-Based** — Each quiz attempt gets a unique session ID
- **Premium Dark UI** — Glassmorphism, gradient accents, animated backgrounds
- **Secure API** — Basic Authentication on all endpoints, XSS prevention

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (Express) |
| Frontend | HTML, CSS, JavaScript, jQuery |
| Styling | Bootstrap 5 + Custom Dark Theme |
| Auth | HTTP Basic Authentication |
| Cloud | Azure App Service + Azure SQL Database (Free Tier) |

---

## 📁 Project Structure

```
Quiz Application/
├── Auth/
│   └── BasicAuthHandler.cs          # Custom Basic Auth handler
├── Controllers/
│   ├── HomeController.cs            # MVC — serves pages
│   └── Api/
│       ├── QuestionsController.cs   # GET questions & details
│       ├── ResponsesController.cs   # POST/GET user answers
│       └── SummaryController.cs     # GET quiz results
├── Data/
│   └── QuizDbContext.cs             # EF Core context + seed data
├── Migrations/                      # EF Core migrations
├── Models/
│   ├── Dtos/
│   │   └── Dtos.cs                  # All DTOs (records)
│   ├── Entities/
│   │   ├── Question.cs
│   │   ├── QuestionOption.cs
│   │   ├── UserResponse.cs
│   │   └── UserSession.cs
│   └── Enums/
│       └── QuestionType.cs          # SingleChoice, MultipleChoice, etc.
├── Repositories/
│   ├── QuestionRepo.cs              # Question data access
│   ├── ResponseRepo.cs              # Response upsert logic
│   └── SessionRepo.cs              # Session lazy creation
├── Services/
│   ├── QuestionSvc.cs               # Question retrieval + shuffle
│   ├── ResponseSvc.cs               # Save pipeline (sanitize → validate → persist)
│   ├── ValidatorSvc.cs              # Type-specific validation rules
│   └── SummarySvc.cs                # Scoring & results builder
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml             # Landing page
│   │   ├── Quiz.cshtml              # Quiz page
│   │   ├── Grid.cshtml              # Question grid
│   │   └── Summary.cshtml           # Results page
│   └── Shared/
│       └── _Layout.cshtml           # Main layout
├── wwwroot/
│   ├── css/
│   │   └── site.css                 # Premium dark theme (600+ lines)
│   └── js/
│       ├── api.js                   # AJAX fetcher with Basic Auth
│       ├── quiz.js                  # Quiz logic, caching, navigation
│       ├── grid.js                  # Question grid renderer
│       └── summary.js               # Results renderer
├── Program.cs                       # App startup & DI config
├── appsettings.json                 # Connection string & auth credentials
├── FUNCTIONS.md                     # Full functions reference doc
└── FUNCTIONS.docx                   # Word version of the above
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or any SQL Server instance)
- `dotnet-ef` global tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd "Quiz Application/Quiz Application"
```

### 2. Configure the Database

Edit `appsettings.json` with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER\\SQLEXPRESS; Database=QuizAppDb; User Id=sa; Password=YOUR_PASSWORD; TrustServerCertificate=True;"
  },
  "Auth": {
    "Username": "admin",
    "Password": "quiz@123"
  }
}
```

### 3. Apply Migrations

```bash
dotnet ef database update
```

This creates the `QuizAppDb` database and seeds it with 20 questions.

### 4. Run the Application

```bash
dotnet run
```

Open your browser at **http://localhost:5243** (or the URL shown in console).

---

---

## ☁️ Azure Deployment (Free Tier)

The application is deployed to Azure using the **F1 Free App Service Plan** and **Serverless SQL Database**.

**Live URL:** [https://quiz-app-darshan.azurewebsites.net](https://quiz-app-darshan.azurewebsites.net)

### Architecture
- **App Service:** Hosts the ASP.NET Core Web Application.
- **SQL Database:** Serverless tier (auto-pauses after 1 hour of inactivity).
- **CI/CD:** Manual deployment via Azure CLI (zip deploy).

---

## 📋 Question Types & Validation

| Type | UI Control | Validation Rule |
|---|---|---|
| **SingleChoice** | Radio Buttons | One option must be selected |
| **MultipleChoice** | Checkboxes | At least one option must be selected |
| **ShortAnswer** | Textbox | Cannot be blank |
| **PhoneNumber** | Textbox (numeric) | Exactly 10 digits, numeric only |
| **LongAnswer** | Textarea | Minimum 10 characters |

All validation is enforced both **client-side** (JavaScript) and **server-side** (ValidatorSvc).

---

## 🔌 API Endpoints

All endpoints require **Basic Authentication** (`admin:quiz@123`).

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/questions` | All questions (grid view, no options) |
| `GET` | `/api/questions/detail` | **5 random questions** with options (shuffled) |
| `GET` | `/api/questions/{orderNo}` | Single question by order number |
| `POST` | `/api/responses` | Save/update a user answer |
| `GET` | `/api/responses/{sessionId}` | All answers for a session |
| `GET` | `/api/summary/{sessionId}` | Quiz results with scoring |

### Example: Save a Response

```bash
curl -X POST http://localhost:5243/api/responses \
  -H "Authorization: Basic YWRtaW46cXVpekAxMjM=" \
  -H "Content-Type: application/json" \
  -d '{"sessionId": "guid-here", "questionId": 1, "answerText": "1"}'
```

---

## 🗄️ Database Schema

```
Questions (Id, OrderNo↑, Text, Type↑, CorrectAnswer)
    │
    ├──→ QuestionOptions (Id, QuestionId↑, Text)
    │
UserSessions (Id [GUID], StartedAt)
    │
    └──→ UserResponses (Id, SessionId↑, QuestionId↑, AnswerText)
                         └── UNIQUE(SessionId, QuestionId)
```

**Indexes:** `OrderNo` (unique), `Type`, `QuestionId` (on options), `SessionId`, `QuestionId` (on responses), `(SessionId, QuestionId)` (unique composite).

---

## 🔒 Security

| Feature | Implementation |
|---|---|
| API Authentication | HTTP Basic Auth via custom handler |
| XSS Prevention | `WebUtility.HtmlEncode()` on all text inputs |
| Security Headers | `X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection`, `Referrer-Policy` |
| HTTPS | Built-in ASP.NET Core HSTS middleware |
| Input Validation | Server-side + client-side, type-specific rules |
| Unique Constraints | Database-enforced one-answer-per-question-per-session |

---

## 📄 Documentation

- **[FUNCTIONS.md](FUNCTIONS.md)** — Detailed reference of every class, method, and logic decision
- **FUNCTIONS.docx** — Word version of the same documentation

---

## 📝 License

This project is for educational purposes.

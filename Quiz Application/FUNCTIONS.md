# Quiz Application — Functions Reference Document

## Table of Contents

1. [Project Architecture](#1-project-architecture)
2. [Entities & Enums](#2-entities--enums)
3. [DTOs (Data Transfer Objects)](#3-dtos-data-transfer-objects)
4. [Database Context](#4-database-context)
5. [Repositories (Data Access Layer)](#5-repositories-data-access-layer)
6. [Services (Business Logic Layer)](#6-services-business-logic-layer)
7. [Authentication](#7-authentication)
8. [API Controllers](#8-api-controllers)
9. [MVC Controller & Views](#9-mvc-controller--views)
10. [Frontend JavaScript](#10-frontend-javascript)
11. [Configuration & Startup](#11-configuration--startup)
12. [Security Measures](#12-security-measures)

---

## 1. Project Architecture

```
┌──────────────────────────────────────────────────────┐
│                   Browser (jQuery)                     │
│   api.js → quiz.js / grid.js / summary.js             │
├──────────────────────────────────────────────────────┤
│              Basic Auth Header (admin:quiz@123)        │
├──────────────────────────────────────────────────────┤
│   MVC Controller          │     API Controllers        │
│   (HomeController)        │  Questions / Responses      │
│   Returns .cshtml views   │  Summary  [Authorize]       │
├──────────────────────────────────────────────────────┤
│                  Services (Business Logic)              │
│   QuestionSvc │ ResponseSvc │ ValidatorSvc │ SummarySvc │
├──────────────────────────────────────────────────────┤
│                  Repositories (Data Access)             │
│   QuestionRepo  │  ResponseRepo  │  SessionRepo         │
├──────────────────────────────────────────────────────┤
│              QuizDbContext (Entity Framework Core)      │
├──────────────────────────────────────────────────────┤
│                SQL Server Database                     │
└──────────────────────────────────────────────────────┘
```

**Tech Stack:** ASP.NET Core 8 MVC, Entity Framework Core, SQL Server, jQuery, Bootstrap 5

---

## 2. Entities & Enums

### QuestionType Enum
**File:** `Models/Enums/QuestionType.cs`

```csharp
public enum QuestionType
{
    SingleChoice,    // Radio buttons — one option must be selected
    MultipleChoice,  // Checkboxes — at least one must be selected
    ShortAnswer,     // Textbox — cannot be blank
    PhoneNumber,     // Textbox — exactly 10 digits, numeric only
    LongAnswer       // Textarea — minimum 10 characters
}
```

**Logic:** Stored as a `string` in the database via EF Core `HasConversion<string>()`, making SQL queries readable.

---

### Question Entity
**File:** `Models/Entities/Question.cs`

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key (auto-increment) |
| `OrderNo` | `int` | Display order (unique index for fast lookup) |
| `Text` | `string` | The question prompt (max 500 chars) |
| `Type` | `QuestionType` | Enum — determines input rendering & validation |
| `CorrectAnswer` | `string?` | For SingleChoice: option ID; for MultipleChoice: comma-separated IDs; null for text types |
| `Options` | `List<QuestionOption>` | Navigation property — child options |

**Logic:** `CorrectAnswer` stores option IDs as strings (e.g., `"1"` or `"5,6,7"`) for choice-based questions. Text-based questions have `null` because there's no single correct answer.

---

### QuestionOption Entity
**File:** `Models/Entities/QuestionOption.cs`

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key — also used as the answer value |
| `QuestionId` | `int` | FK → Question |
| `Text` | `string` | Display text (max 200 chars) |
| `Question` | `Question` | Navigation property |

---

### UserSession Entity
**File:** `Models/Entities/UserSession.cs`

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Session ID (generated client-side via `crypto.randomUUID()`) |
| `StartedAt` | `DateTime` | Timestamp when first response was saved |

**Logic:** Sessions are created lazily — only when the first response is saved, not when the quiz page loads.

---

### UserResponse Entity
**File:** `Models/Entities/UserResponse.cs`

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | PK |
| `SessionId` | `Guid` | FK → UserSession |
| `QuestionId` | `int` | FK → Question |
| `AnswerText` | `string` | Stored answer (HTML-encoded for XSS prevention) |

**Constraint:** `(SessionId, QuestionId)` has a **unique index** — one answer per question per session. Subsequent saves are updates (upsert).

---

## 3. DTOs (Data Transfer Objects)

**File:** `Models/Dtos/Dtos.cs`

All DTOs defined as C# `record` types (immutable, concise syntax).

| Record | Used By | Purpose |
|---|---|---|
| `QuestionGridDto(QuestionId, OrderNo, QuestionText, QuestionType, CorrectAnswer?)` | Grid page | Displays all questions without options |
| `QuestionDetailDto(QuestionId, OrderNo, QuestionText, QuestionType, Options?)` | Quiz page | Full question with its answer options |
| `OptionDto(OptionId, OptionText)` | Quiz page | Single option for a choice question |
| `UserResponseDto(ResponseId, QuestionId, AnswerText)` | Quiz page | Previously saved answer |
| `SaveResponseReq(SessionId, QuestionId, AnswerText)` | API POST body | Request payload for saving a response |
| `SummaryDto(QuestionSummaries, Stats)` | Summary page | Complete quiz results |
| `QuestionSummaryItem(QuestionText, QuestionType, UserAnswer?, CorrectAnswer?, IsCorrect?)` | Summary page | One row of results |
| `SummaryStats(Total, Attempted, Correct, Percentage)` | Summary page | Aggregate score data |

**Why records?** They provide value equality, immutability, and a compact one-line syntax — ideal for API data transfer.

---

## 4. Database Context

**File:** `Data/QuizDbContext.cs`

### Class: `QuizDbContext`

**Constructor:** Uses Primary Constructor syntax — `QuizDbContext(DbContextOptions<QuizDbContext> options)`

**DbSets:**
- `Questions` → maps to `Questions` table
- `QuestionOptions` → maps to `QuestionOptions` table
- `UserSessions` → maps to `UserSessions` table
- `UserResponses` → maps to `UserResponses` table

### Method: `OnModelCreating(ModelBuilder mb)`

Configures Fluent API rules:

| Entity | Configuration | Logic |
|---|---|---|
| `Question` | `HasIndex(OrderNo).IsUnique()` | Ensures no duplicate ordering |
| `Question` | `HasIndex(Type)` | Speeds up filtering by question type |
| `Question` | `HasConversion<string>()` on Type | Stores enum as readable string, not int |
| `QuestionOption` | `HasIndex(QuestionId)` | Speeds up eager-loading `.Include(q => q.Options)` |
| `UserResponse` | `HasIndex(SessionId, QuestionId).IsUnique()` | Enforces one answer per question per session |
| `UserResponse` | `HasIndex(SessionId)` | Fast lookup of all answers in a session |
| `UserResponse` | `HasIndex(QuestionId)` | Fast lookup by question |

### Method: `SeedQuestions(ModelBuilder mb)` (private static)

Pre-populates the database with **20 questions** across all 5 types:
- 5 SingleChoice, 4 MultipleChoice, 4 ShortAnswer, 3 PhoneNumber, 4 LongAnswer
- Also seeds 40 QuestionOptions for choice-based questions.

**Logic:** Uses `HasData()` — EF Core's seed mechanism that runs once during migration. IDs are hardcoded to maintain referential integrity.

---

## 5. Repositories (Data Access Layer)

All repositories follow the **Repository Pattern**: interface + implementation. They wrap EF Core queries and are registered as **Scoped** services in DI.

---

### QuestionRepo
**File:** `Repositories/QuestionRepo.cs`

| Method | Signature | Logic |
|---|---|---|
| `GetAllAsync()` | `Task<List<Question>>` | Fetches all questions with options, ordered by `OrderNo`. Uses `.Include(q => q.Options)` for eager loading and `.AsNoTracking()` for read-only performance. |
| `GetByOrderAsync(orderNo)` | `Task<Question?>` | Fetches a single question by its `OrderNo` with options. Uses `.AsNoTracking()`. Returns `null` if not found. |
| `GetCountAsync()` | `Task<int>` | Returns total count of questions in the database. Used by the API to validate order number range. |

**Performance:** `AsNoTracking()` skips EF Core change tracking — faster for read-only queries where we never modify the entities.

---

### ResponseRepo
**File:** `Repositories/ResponseRepo.cs`

| Method | Signature | Logic |
|---|---|---|
| `GetAsync(sessionId, questionId)` | `Task<UserResponse?>` | Finds an existing response by session + question composite key. Used internally by `UpsertAsync`. |
| `UpsertAsync(sessionId, questionId, answerText)` | `Task<int>` | **Upsert pattern:** Calls `GetAsync()` first. If a response exists → updates `AnswerText`. If not → creates a new record. Returns the response ID. This allows users to change answers freely. |
| `GetAllBySessionAsync(sessionId)` | `Task<List<UserResponse>>` | Returns all responses for a session. Uses `.AsNoTracking()`. Used by the summary page and quiz page to restore saved answers. |

**Why Upsert?** The quiz allows free navigation — users can go back and change answers. Without upsert, duplicate entries would violate the unique constraint.

---

### SessionRepo
**File:** `Repositories/SessionRepo.cs`

| Method | Signature | Logic |
|---|---|---|
| `EnsureExistsAsync(sessionId)` | `Task` | Checks if a session with the given GUID exists using `.AnyAsync()`. If not, creates a new `UserSession` with `DateTime.UtcNow`. This is a **lazy creation** pattern — sessions are only created when the first answer is saved. |

**Why Lazy?** The session ID is generated client-side (`crypto.randomUUID()`). We don't create a DB record until the user actually submits their first answer.

---

## 6. Services (Business Logic Layer)

Services contain the core application logic. They are registered as **Scoped** in DI and injected into controllers.

---

### QuestionSvc
**File:** `Services/QuestionSvc.cs`

| Method | Signature | Logic |
|---|---|---|
| `GetAllAsync()` | `Task<IEnumerable<QuestionGridDto>>` | Maps entities to `QuestionGridDto`. Only includes `CorrectAnswer` for choice-based types (SingleChoice, MultipleChoice). Text types return `null` for correct answer since they're subjective. |
| `GetAllDetailAsync()` | `Task<IEnumerable<QuestionDetailDto>>` | Fetches all questions, **shuffles them randomly** using Fisher-Yates algorithm, then maps with sequential `OrderNo` (1, 2, 3...). Returns options for each question so the quiz can render inputs. |
| `GetByOrderAsync(orderNo)` | `Task<QuestionDetailDto?>` | Returns a single detailed question by order number with sorted options. Options are sorted by `Id` for consistent display. |
| `GetCountAsync()` | `Task<int>` | Delegates to `QuestionRepo.GetCountAsync()`. |

**Randomization Logic (Fisher-Yates Shuffle):**
```csharp
var rng = Random.Shared;
for (int i = list.Count - 1; i > 0; i--)
{
    int j = rng.Next(i + 1);
    (list[i], list[j]) = (list[j], list[i]);
}
```
Each element is swapped with a random earlier element, producing an unbiased permutation. `Random.Shared` is thread-safe in .NET 8.

---

### ResponseSvc
**File:** `Services/ResponseSvc.cs`

| Method | Signature | Logic |
|---|---|---|
| `SaveAsync(sessionId, questionId, answerText)` | `Task<(bool, int?, List<string>)>` | **Pipeline:** 1) HTML-encode the answer via `WebUtility.HtmlEncode()` to prevent XSS. 2) Call `ValidatorSvc.ValidateAsync()` — if errors, return early. 3) Call `SessionRepo.EnsureExistsAsync()` — create session if needed. 4) Call `ResponseRepo.UpsertAsync()` — insert or update the response. Returns a tuple of (success, responseId, errors). |
| `GetBySessionAsync(sessionId)` | `Task<Dictionary<int, string>>` | Returns all saved answers as a `Dictionary<QuestionId, AnswerText>` for restoring quiz state when navigating back. |

**Why HTML-encode?** If a user types `<script>alert('xss')</script>` in a text answer, it gets stored as `&lt;script&gt;...` — preventing stored XSS attacks.

---

### ValidatorSvc
**File:** `Services/ValidatorSvc.cs`

| Method | Signature | Logic |
|---|---|---|
| `ValidateAsync(questionId, answerText)` | `Task<List<string>>` | Fetches the question from the DB to determine its type, then dispatches to the appropriate type-specific validator. Returns a list of error messages (empty = valid). |

**Type-specific private validators:**

| Method | Validation Rules | Error Message |
|---|---|---|
| `ValidateSingleChoice(answer, validIds)` | Must not be blank. Must parse to `int`. The parsed ID must exist in the question's option IDs. | "Please select an option." / "Invalid option value." / "Selected option is not valid." |
| `ValidateMultipleChoice(answer, validIds)` | Must not be blank. Split by comma. Each part must parse to `int` and exist in valid IDs. | "Please select at least one option." / "Invalid option ID: {id}" |
| `ValidateShortAnswer(answer)` | Must not be blank or whitespace-only. | "Answer cannot be blank." |
| `ValidatePhoneNumber(answer)` | Must match regex `^\d{10}$` — exactly 10 numeric digits. | "Phone number must be exactly 10 digits." |
| `ValidateLongAnswer(answer)` | Trimmed length must be ≥ 10 characters. | "Answer must be at least 10 characters." |

**Logic:** Uses a `switch` expression on `q.Type` to dispatch. Each validator returns `List<string>` — an empty list means validation passed.

---

### SummarySvc
**File:** `Services/SummarySvc.cs`

| Method | Signature | Logic |
|---|---|---|
| `BuildAsync(sessionId)` | `Task<SummaryDto>` | Builds the complete quiz results summary. |

**BuildAsync detailed logic:**

1. **Fetch data:** Gets all questions from `QuestionRepo` and all user responses from `ResponseRepo`.
2. **Build response map:** `Dictionary<QuestionId, AnswerText>` for O(1) lookup.
3. **Iterate questions (ordered):** For each question:
   - Look up the user's answer from the response map.
   - **Scoring logic:**
     - **SingleChoice:** Compare user's answer (option ID) with `CorrectAnswer`. Exact string match → correct.
     - **MultipleChoice:** Split both user answer and correct answer by comma, sort both lists, then `SequenceEqual()` — order-independent comparison.
     - **ShortAnswer / PhoneNumber / LongAnswer:** `isCorrect` stays `null` — no auto-grading for text answers.
   - **Display text mapping:** Convert stored option IDs back to human-readable text (e.g., `"11"` → `"Mars"`).
4. **Compute stats:** Total questions, attempted (non-empty answers), correct count, percentage.

---

## 7. Authentication

### BasicAuthHandler
**File:** `Auth/BasicAuthHandler.cs`

Extends `AuthenticationHandler<AuthenticationSchemeOptions>` from ASP.NET Core.

| Method | Logic |
|---|---|
| `HandleAuthenticateAsync()` | 1) Check for `Authorization` header — fail if missing. 2) Parse the header as `Basic {base64}`. 3) Decode Base64 → `username:password`. 4) Compare against `Auth:Username` and `Auth:Password` from `appsettings.json`. 5) If match → create a `ClaimsPrincipal` with the username and return `AuthenticateResult.Success`. If no match → return `AuthenticateResult.Fail`. |
| `HandleChallengeAsync()` | Sets response status to `401` and adds `WWW-Authenticate: Basic realm="QuizApp"` header. |

**How it works in the flow:**
- The frontend JS (`api.js`) hardcodes `Authorization: Basic YWRtaW46cXVpekAxMjM=` (base64 of `admin:quiz@123`) in every AJAX request.
- The handler decodes and validates this silently — no user login prompt.
- This verifies API calls originate from the app, not from random external requests.

---

## 8. API Controllers

All API controllers use:
- `[ApiController]` → automatic model validation, JSON responses
- `[Authorize]` → requires Basic Auth on every endpoint
- Route prefix `api/`

---

### QuestionsController
**File:** `Controllers/Api/QuestionsController.cs` — Route: `api/questions`

| Endpoint | Method | Logic |
|---|---|---|
| `GET /api/questions` | `GetAll()` | Returns all questions as `QuestionGridDto[]` (no options). Used by the Grid page. |
| `GET /api/questions/detail` | `GetAllDetail()` | Returns all questions as `QuestionDetailDto[]` WITH options, shuffled randomly. Used by the Quiz page. |
| `GET /api/questions/{orderNo}` | `GetByOrder(int)` | Returns a single question by order number. Validates `orderNo >= 1` and `<= total`. Returns 404 if not found. |

---

### ResponsesController
**File:** `Controllers/Api/ResponsesController.cs` — Route: `api/responses`

| Endpoint | Method | Logic |
|---|---|---|
| `POST /api/responses` | `Save(SaveResponseReq)` | Validates `SessionId != Guid.Empty`. Calls `ResponseSvc.SaveAsync()`. Returns `{ success, responseId }` or `{ errors }`. |
| `GET /api/responses/{sessionId}` | `GetBySession(Guid)` | Returns all saved answers for a session as `UserResponseDto[]`. Returns `204 No Content` if no answers found. |

---

### SummaryController
**File:** `Controllers/Api/SummaryController.cs` — Route: `api/summary`

| Endpoint | Method | Logic |
|---|---|---|
| `GET /api/summary/{sessionId}` | `Get(Guid)` | Validates session ID is not empty. Calls `SummarySvc.BuildAsync()`. Returns the full `SummaryDto` with scoring. |

---

## 9. MVC Controller & Views

### HomeController
**File:** `Controllers/HomeController.cs`

| Action | View File | Purpose |
|---|---|---|
| `Index()` | `Views/Home/Index.cshtml` | Landing page — hero section with "Start Quiz" and "View Questions" buttons |
| `Quiz()` | `Views/Home/Quiz.cshtml` | Quiz card — question text, input area, validation errors, navigation buttons |
| `Grid()` | `Views/Home/Grid.cshtml` | Table view of all questions fetched from API |
| `Summary()` | `Views/Home/Summary.cshtml` | Results page — stat cards + detailed results table |

**Note:** MVC views are **public** (no `[Authorize]`). Only API endpoints require auth.

---

## 10. Frontend JavaScript

### api.js — AJAX Fetcher
**File:** `wwwroot/js/api.js`

**Pattern:** Module pattern (`IIFE` returning a public API object).

```javascript
const QuizApi = (() => {
    const AUTH_HEADER = 'Basic ' + btoa('admin:quiz@123');
    // ... internal request function ...
    return { fetchAllQuestions, fetchAllQuestionsDetail, ... };
})();
```

| Function | HTTP Call | Logic |
|---|---|---|
| `request(url, options)` | (internal) | Adds `Authorization` and `Content-Type` headers to every request. Handles 401 (auth fail), 204 (no content), and error JSON parsing. Returns `{ ok, data, error }`. |
| `fetchAllQuestions()` | `GET /api/questions` | For Grid page |
| `fetchAllQuestionsDetail()` | `GET /api/questions/detail` | For Quiz page — includes options |
| `fetchQuestionByOrder(orderNo)` | `GET /api/questions/{orderNo}` | For individual question fetch |
| `postResponse(sessionId, questionId, answerText)` | `POST /api/responses` | Saves a user answer |
| `fetchSessionResponses(sessionId)` | `GET /api/responses/{sessionId}` | Restores saved answers |
| `fetchSummary(sessionId)` | `GET /api/summary/{sessionId}` | Gets quiz results |

---

### quiz.js — Quiz Page Logic
**File:** `wwwroot/js/quiz.js`

**Key Variables:**
- `questions[]` — all questions cached in memory after initial load
- `currentIndex` — index into the cached array (0-based)
- `responseCache{}` — map of `questionId → answerText` for instant navigation

**Init Flow:**
```
Page Load
  → getOrCreateSessionId()        // Creates/reads GUID from localStorage
  → Promise.all([                  // Parallel fetch
       fetchAllQuestionsDetail(),  // All 20 questions with options (shuffled)
       fetchSessionResponses()     // Previously saved answers
     ])
  → renderCurrent()               // Instant render from cache
```

| Function | Logic |
|---|---|
| `getOrCreateSessionId()` | Reads `quiz_session_id` from `localStorage`. If missing, generates a new `crypto.randomUUID()` and stores it. |
| `renderCurrent()` | Reads `questions[currentIndex]`, updates question text, progress badge, progress bar, and renders the appropriate input type. Restores saved answer from `responseCache`. |
| `renderRadioGroup(options, saved)` | Creates radio buttons inside `.form-check-custom` divs. Pre-selects the saved option if present. |
| `renderCheckboxGroup(options, saved)` | Creates checkboxes. Splits saved answer by comma to pre-check multiple options. |
| `renderTextbox(saved)` | Creates a text input with saved value. |
| `renderPhoneInput(saved)` | Creates a text input with `inputmode="numeric"` and `maxlength="10"`. |
| `renderTextarea(saved)` | Creates a textarea with 4 rows. |
| `getCurrentAnswer()` | Reads the current input value based on question type — selected radio, checked checkboxes (joined by comma), or text content. |
| `validateCurrentInput()` | **Client-side validation** mirroring backend rules. Shows error message via `#validation-error` div. Returns `true/false`. |
| `onNextClick()` | Validates → saves to API → advances `currentIndex` → renders next question. |
| `onPreviousClick()` | Decrements `currentIndex` → renders immediately from cache (no API call). |
| `onSubmitClick()` | Validates → saves → redirects to `/Home/Summary?sessionId={guid}`. |
| `updateNavButtons()` | Hides "Next" and shows "Submit" on the last question. Disables "Previous" on the first. |
| `updateProgress()` | Updates the gradient progress bar width as a percentage. |

---

### grid.js — Question Grid Page
**File:** `wwwroot/js/grid.js`

| Function | Logic |
|---|---|
| `loadGrid()` | Calls `fetchAllQuestions()`, hides spinner, populates the table. Handles API errors gracefully. |
| `getBadgeClass(type)` | Maps question type to CSS badge class: `single`, `multiple`, `short`, `phone`, `long`. |
| `buildGridRow(q)` | Generates a `<tr>` with order number, question text, colored type badge, and correct answer (or "N/A" for text types). |

---

### summary.js — Summary Page
**File:** `wwwroot/js/summary.js`

| Function | Logic |
|---|---|
| `loadSummary(sessionId)` | Reads `sessionId` from URL query string. Calls `fetchSummary()`. Hides spinner. Renders stats + table. |
| `renderStats(stats)` | Creates 4 stat cards: Total, Attempted, Correct, Score%. Each card has a gradient background and hover lift effect. |
| `renderTable(items)` | Populates the results table with question text, type badge, user answer, correct answer, and result badge (✓ Correct / ✗ Wrong / — N/A). |

---

## 11. Configuration & Startup

### Program.cs

**Execution order:**

1. **EF Core:** `AddDbContext<QuizDbContext>` with SQL Server connection string
2. **DI Registration:** Repositories and Services as `Scoped`
3. **Authentication:** Basic Auth scheme via `BasicAuthHandler`
4. **Authorization:** Added to the pipeline
5. **CORS:** Configured to allow same-origin requests
6. **MVC + API Routing:** `MapControllerRoute` + `MapControllers`
7. **Security Headers Middleware:** Custom inline middleware that adds:
   - `X-Content-Type-Options: nosniff`
   - `X-Frame-Options: DENY`
   - `X-XSS-Protection: 1; mode=block`
   - `Referrer-Policy: strict-origin-when-cross-origin`

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LAPTOP-2FNLU40I\\SQLEXPRESS; Database=QuizAppDb; ..."
  },
  "Auth": {
    "Username": "admin",
    "Password": "quiz@123"
  }
}
```

---

## 12. Security Measures

| Measure | Implementation | Purpose |
|---|---|---|
| Basic Authentication | `BasicAuthHandler` + `[Authorize]` on API controllers | Verifies API calls come from the app |
| HTML Encoding | `WebUtility.HtmlEncode()` in `ResponseSvc` | Prevents stored XSS in text answers |
| Security Headers | Custom middleware in `Program.cs` | Protects against clickjacking, MIME sniffing, XSS |
| HTTPS + HSTS | Built-in ASP.NET Core middleware | Encrypts traffic, forces HTTPS |
| Route Constraints | `{orderNo:int}`, `{sessionId:guid}` | Prevents invalid parameter types |
| Model Validation | `[ApiController]` attribute | Auto-returns 400 for invalid request bodies |
| Unique Constraints | DB indexes on `(SessionId, QuestionId)` | Prevents duplicate answers |
| AsNoTracking() | All read-only repository queries | Improves query performance |
| DB Indexes | `OrderNo`, `Type`, `QuestionId`, `SessionId` | Faster query execution |

using Microsoft.EntityFrameworkCore;
using Quiz_Application.Models.Entities;
using Quiz_Application.Models.Enums;

namespace Quiz_Application.Data;

public class QuizDbContext(DbContextOptions<QuizDbContext> options) : DbContext(options)
{
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserResponse> UserResponses => Set<UserResponse>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ----- Question -----
        mb.Entity<Question>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.Text).IsRequired().HasMaxLength(500);
            e.Property(q => q.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(q => q.CorrectAnswer).HasMaxLength(200);
            e.HasIndex(q => q.OrderNo).IsUnique();
            e.HasIndex(q => q.Type);
            e.HasMany(q => q.Options).WithOne(o => o.Question).HasForeignKey(o => o.QuestionId);
        });

        // ----- QuestionOption -----
        mb.Entity<QuestionOption>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Text).IsRequired().HasMaxLength(200);
            e.HasIndex(o => o.QuestionId);
        });

        // ----- UserSession -----
        mb.Entity<UserSession>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.StartedAt).IsRequired();
        });

        // ----- UserResponse -----
        mb.Entity<UserResponse>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.AnswerText).IsRequired().HasMaxLength(1000);
            e.HasIndex(r => new { r.SessionId, r.QuestionId }).IsUnique();
            e.HasIndex(r => r.SessionId);
            e.HasIndex(r => r.QuestionId);
            e.HasOne(r => r.Session).WithMany().HasForeignKey(r => r.SessionId);
            e.HasOne(r => r.Question).WithMany().HasForeignKey(r => r.QuestionId);
        });

        // ===== Seed Data =====
        SeedQuestions(mb);
    }

    private static void SeedQuestions(ModelBuilder mb)
    {
        mb.Entity<Question>().HasData(
            // ===== SingleChoice (Radio Button) — 5 questions =====
            new Question { Id = 1, OrderNo = 1, Text = "What is the capital of France?", Type = QuestionType.SingleChoice, CorrectAnswer = "1" },
            new Question { Id = 6, OrderNo = 6, Text = "Which planet is known as the Red Planet?", Type = QuestionType.SingleChoice, CorrectAnswer = "11" },
            new Question { Id = 9, OrderNo = 9, Text = "What is the largest ocean on Earth?", Type = QuestionType.SingleChoice, CorrectAnswer = "21" },
            new Question { Id = 13, OrderNo = 13, Text = "Who painted the Mona Lisa?", Type = QuestionType.SingleChoice, CorrectAnswer = "31" },
            new Question { Id = 17, OrderNo = 17, Text = "What is the speed of light approximately?", Type = QuestionType.SingleChoice, CorrectAnswer = "39" },

            // ===== MultipleChoice (Checkbox) — 4 questions =====
            new Question { Id = 2, OrderNo = 2, Text = "Which of the following are programming languages?", Type = QuestionType.MultipleChoice, CorrectAnswer = "5,6,7" },
            new Question { Id = 7, OrderNo = 7, Text = "Select all prime numbers from the list below.", Type = QuestionType.MultipleChoice, CorrectAnswer = "14,15,17" },
            new Question { Id = 10, OrderNo = 10, Text = "Which of these are JavaScript frameworks?", Type = QuestionType.MultipleChoice, CorrectAnswer = "25,26,27" },
            new Question { Id = 14, OrderNo = 14, Text = "Which of these are database systems?", Type = QuestionType.MultipleChoice, CorrectAnswer = "33,34,35" },

            // ===== ShortAnswer (Textbox) — 4 questions =====
            new Question { Id = 3, OrderNo = 3, Text = "What does HTML stand for?", Type = QuestionType.ShortAnswer, CorrectAnswer = null },
            new Question { Id = 8, OrderNo = 8, Text = "What is the chemical symbol for water?", Type = QuestionType.ShortAnswer, CorrectAnswer = null },
            new Question { Id = 11, OrderNo = 11, Text = "What is the square root of 144?", Type = QuestionType.ShortAnswer, CorrectAnswer = null },
            new Question { Id = 15, OrderNo = 15, Text = "What does CSS stand for?", Type = QuestionType.ShortAnswer, CorrectAnswer = null },

            // ===== PhoneNumber — 3 questions =====
            new Question { Id = 4, OrderNo = 4, Text = "Enter your phone number", Type = QuestionType.PhoneNumber, CorrectAnswer = null },
            new Question { Id = 12, OrderNo = 12, Text = "Enter your emergency contact number", Type = QuestionType.PhoneNumber, CorrectAnswer = null },
            new Question { Id = 18, OrderNo = 18, Text = "Enter your alternate mobile number", Type = QuestionType.PhoneNumber, CorrectAnswer = null },

            // ===== LongAnswer (Textarea) — 4 questions =====
            new Question { Id = 5, OrderNo = 5, Text = "Explain the concept of Object-Oriented Programming in detail.", Type = QuestionType.LongAnswer, CorrectAnswer = null },
            new Question { Id = 16, OrderNo = 16, Text = "Describe the difference between SQL and NoSQL databases.", Type = QuestionType.LongAnswer, CorrectAnswer = null },
            new Question { Id = 19, OrderNo = 19, Text = "What are the advantages of using cloud computing?", Type = QuestionType.LongAnswer, CorrectAnswer = null },
            new Question { Id = 20, OrderNo = 20, Text = "Explain the MVC architecture pattern and its benefits.", Type = QuestionType.LongAnswer, CorrectAnswer = null }
        );

        mb.Entity<QuestionOption>().HasData(
            // Q1 — Capital of France (SingleChoice)
            new QuestionOption { Id = 1, QuestionId = 1, Text = "Paris" },
            new QuestionOption { Id = 2, QuestionId = 1, Text = "London" },
            new QuestionOption { Id = 3, QuestionId = 1, Text = "Berlin" },
            new QuestionOption { Id = 4, QuestionId = 1, Text = "Madrid" },

            // Q2 — Programming languages (MultipleChoice)
            new QuestionOption { Id = 5, QuestionId = 2, Text = "C#" },
            new QuestionOption { Id = 6, QuestionId = 2, Text = "Python" },
            new QuestionOption { Id = 7, QuestionId = 2, Text = "JavaScript" },
            new QuestionOption { Id = 8, QuestionId = 2, Text = "Photoshop" },

            // Q6 — Red Planet (SingleChoice)
            new QuestionOption { Id = 9, QuestionId = 6, Text = "Venus" },
            new QuestionOption { Id = 10, QuestionId = 6, Text = "Jupiter" },
            new QuestionOption { Id = 11, QuestionId = 6, Text = "Mars" },
            new QuestionOption { Id = 12, QuestionId = 6, Text = "Saturn" },

            // Q7 — Prime numbers (MultipleChoice)
            new QuestionOption { Id = 13, QuestionId = 7, Text = "4" },
            new QuestionOption { Id = 14, QuestionId = 7, Text = "7" },
            new QuestionOption { Id = 15, QuestionId = 7, Text = "11" },
            new QuestionOption { Id = 16, QuestionId = 7, Text = "9" },
            new QuestionOption { Id = 17, QuestionId = 7, Text = "13" },

            // Q9 — Largest ocean (SingleChoice)
            new QuestionOption { Id = 18, QuestionId = 9, Text = "Atlantic Ocean" },
            new QuestionOption { Id = 19, QuestionId = 9, Text = "Indian Ocean" },
            new QuestionOption { Id = 20, QuestionId = 9, Text = "Arctic Ocean" },
            new QuestionOption { Id = 21, QuestionId = 9, Text = "Pacific Ocean" },

            // Q10 — JS frameworks (MultipleChoice)
            new QuestionOption { Id = 22, QuestionId = 10, Text = "Django" },
            new QuestionOption { Id = 23, QuestionId = 10, Text = "Laravel" },
            new QuestionOption { Id = 24, QuestionId = 10, Text = "Flask" },
            new QuestionOption { Id = 25, QuestionId = 10, Text = "React" },
            new QuestionOption { Id = 26, QuestionId = 10, Text = "Angular" },
            new QuestionOption { Id = 27, QuestionId = 10, Text = "Vue.js" },

            // Q13 — Mona Lisa (SingleChoice)
            new QuestionOption { Id = 28, QuestionId = 13, Text = "Vincent van Gogh" },
            new QuestionOption { Id = 29, QuestionId = 13, Text = "Pablo Picasso" },
            new QuestionOption { Id = 30, QuestionId = 13, Text = "Michelangelo" },
            new QuestionOption { Id = 31, QuestionId = 13, Text = "Leonardo da Vinci" },

            // Q14 — Database systems (MultipleChoice)
            new QuestionOption { Id = 32, QuestionId = 14, Text = "Photoshop" },
            new QuestionOption { Id = 33, QuestionId = 14, Text = "MySQL" },
            new QuestionOption { Id = 34, QuestionId = 14, Text = "PostgreSQL" },
            new QuestionOption { Id = 35, QuestionId = 14, Text = "MongoDB" },
            new QuestionOption { Id = 36, QuestionId = 14, Text = "Excel" },

            // Q17 — Speed of light (SingleChoice)
            new QuestionOption { Id = 37, QuestionId = 17, Text = "150,000 km/s" },
            new QuestionOption { Id = 38, QuestionId = 17, Text = "500,000 km/s" },
            new QuestionOption { Id = 39, QuestionId = 17, Text = "300,000 km/s" },
            new QuestionOption { Id = 40, QuestionId = 17, Text = "1,000,000 km/s" }
        );
    }
}

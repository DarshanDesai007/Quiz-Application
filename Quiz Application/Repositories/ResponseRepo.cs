using Microsoft.EntityFrameworkCore;
using Quiz_Application.Data;
using Quiz_Application.Models.Entities;

namespace Quiz_Application.Repositories;

public interface IResponseRepo
{
    Task<UserResponse?> GetAsync(Guid sessionId, int questionId);
    Task<int> UpsertAsync(Guid sessionId, int questionId, string answerText);
    Task<List<UserResponse>> GetAllBySessionAsync(Guid sessionId);
}

public class ResponseRepo(QuizDbContext db) : IResponseRepo
{
    public Task<UserResponse?> GetAsync(Guid sessionId, int questionId)
        => db.UserResponses
              .FirstOrDefaultAsync(r => r.SessionId == sessionId && r.QuestionId == questionId);

    public async Task<int> UpsertAsync(Guid sessionId, int questionId, string answerText)
    {
        var existing = await GetAsync(sessionId, questionId);

        if (existing is not null)
        {
            existing.AnswerText = answerText;
            db.UserResponses.Update(existing);
        }
        else
        {
            existing = new UserResponse
            {
                SessionId = sessionId,
                QuestionId = questionId,
                AnswerText = answerText
            };
            db.UserResponses.Add(existing);
        }

        await db.SaveChangesAsync();
        return existing.Id;
    }

    public Task<List<UserResponse>> GetAllBySessionAsync(Guid sessionId)
        => db.UserResponses
              .Where(r => r.SessionId == sessionId)
              .AsNoTracking()
              .ToListAsync();
}

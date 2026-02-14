using Microsoft.EntityFrameworkCore;
using Quiz_Application.Data;
using Quiz_Application.Models.Entities;

namespace Quiz_Application.Repositories;

public interface ISessionRepo
{
    Task EnsureExistsAsync(Guid sessionId);
}

public class SessionRepo(QuizDbContext db) : ISessionRepo
{
    public async Task EnsureExistsAsync(Guid sessionId)
    {
        bool exists = await db.UserSessions.AnyAsync(s => s.Id == sessionId);
        if (!exists)
        {
            db.UserSessions.Add(new UserSession
            {
                Id = sessionId,
                StartedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }
}

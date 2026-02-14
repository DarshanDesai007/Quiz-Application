using Microsoft.EntityFrameworkCore;
using Quiz_Application.Data;
using Quiz_Application.Models.Entities;

namespace Quiz_Application.Repositories;

public interface IQuestionRepo
{
    Task<List<Question>> GetAllAsync();
    Task<Question?> GetByOrderAsync(int orderNo);
    Task<int> GetCountAsync();
}

public class QuestionRepo(QuizDbContext db) : IQuestionRepo
{
    public Task<List<Question>> GetAllAsync()
        => db.Questions
              .Include(q => q.Options)
              .OrderBy(q => q.OrderNo)
              .AsNoTracking()
              .ToListAsync();

    public Task<Question?> GetByOrderAsync(int orderNo)
        => db.Questions
              .Include(q => q.Options)
              .AsNoTracking()
              .FirstOrDefaultAsync(q => q.OrderNo == orderNo);

    public Task<int> GetCountAsync()
        => db.Questions.CountAsync();
}

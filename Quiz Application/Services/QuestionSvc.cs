using Quiz_Application.Models.Dtos;
using Quiz_Application.Models.Enums;
using Quiz_Application.Repositories;

namespace Quiz_Application.Services;

public interface IQuestionSvc
{
    Task<IEnumerable<QuestionGridDto>> GetAllAsync();
    Task<IEnumerable<QuestionDetailDto>> GetAllDetailAsync();
    Task<QuestionDetailDto?> GetByOrderAsync(int orderNo);
    Task<int> GetCountAsync();
}

public class QuestionSvc(IQuestionRepo repo) : IQuestionSvc
{
    public async Task<IEnumerable<QuestionGridDto>> GetAllAsync()
    {
        var questions = await repo.GetAllAsync();

        return questions.Select(q => new QuestionGridDto(
            q.Id,
            q.OrderNo,
            q.Text,
            q.Type.ToString(),
            q.Type is QuestionType.SingleChoice or QuestionType.MultipleChoice
                ? q.CorrectAnswer
                : null
        ));
    }

    public async Task<IEnumerable<QuestionDetailDto>> GetAllDetailAsync()
    {
        var questions = await repo.GetAllAsync();

        // Shuffle questions randomly (Fisher-Yates)
        var list = questions.ToList();
        var rng = Random.Shared;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        // Map to DTOs with sequential order numbers
        return list.Take(5).Select((q, idx) =>
        {
            var options = q.Options
                .OrderBy(o => o.Id)
                .Select(o => new OptionDto(o.Id, o.Text))
                .ToList();

            return new QuestionDetailDto(
                q.Id,
                idx + 1,         // shuffled position: 1, 2, 3...
                q.Text,
                q.Type.ToString(),
                options.Count > 0 ? options : null
            );
        });
    }

    public async Task<QuestionDetailDto?> GetByOrderAsync(int orderNo)
    {
        var q = await repo.GetByOrderAsync(orderNo);
        if (q is null) return null;

        var options = q.Options
            .OrderBy(o => o.Id)
            .Select(o => new OptionDto(o.Id, o.Text))
            .ToList();

        return new QuestionDetailDto(
            q.Id,
            q.OrderNo,
            q.Text,
            q.Type.ToString(),
            options.Count > 0 ? options : null
        );
    }

    public Task<int> GetCountAsync() => repo.GetCountAsync();
}

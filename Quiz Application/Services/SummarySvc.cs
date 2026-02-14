using Quiz_Application.Models.Dtos;
using Quiz_Application.Models.Enums;
using Quiz_Application.Repositories;

namespace Quiz_Application.Services;

public interface ISummarySvc
{
    Task<SummaryDto> BuildAsync(Guid sessionId);
}

public class SummarySvc(IQuestionRepo questionRepo, IResponseRepo responseRepo) : ISummarySvc
{
    public async Task<SummaryDto> BuildAsync(Guid sessionId)
    {
        var questions = await questionRepo.GetAllAsync();
        var responses = await responseRepo.GetAllBySessionAsync(sessionId);
        var responseMap = responses.ToDictionary(r => r.QuestionId, r => r.AnswerText);

        // Filter: Only include questions that have a response (since we serve random 5)
        var relevantQuestions = questions
            .Where(q => responseMap.ContainsKey(q.Id))
            .OrderBy(q => q.OrderNo)
            .ToList();

        var summaries = new List<QuestionSummaryItem>();
        int attempted = 0, correct = 0;

        foreach (var q in relevantQuestions)
        {
            responseMap.TryGetValue(q.Id, out var userAnswer);
            bool hasAnswer = !string.IsNullOrWhiteSpace(userAnswer);
            if (hasAnswer) attempted++;

            bool? isCorrect = null;

            if (q.Type is QuestionType.SingleChoice)
            {
                if (hasAnswer && q.CorrectAnswer is not null)
                {
                    isCorrect = userAnswer?.Trim() == q.CorrectAnswer.Trim();
                    if (isCorrect == true) correct++;
                }
            }
            else if (q.Type is QuestionType.MultipleChoice)
            {
                if (hasAnswer && q.CorrectAnswer is not null)
                {
                    var userIds = userAnswer!.Split(',').Select(s => s.Trim()).OrderBy(s => s).ToList();
                    var correctIds = q.CorrectAnswer.Split(',').Select(s => s.Trim()).OrderBy(s => s).ToList();
                    isCorrect = userIds.SequenceEqual(correctIds);
                    if (isCorrect == true) correct++;
                }
            }
            // ShortAnswer, PhoneNumber, LongAnswer → isCorrect stays null

            // Map correct answer display text
            string? correctDisplay = q.Type switch
            {
                QuestionType.SingleChoice =>
                    q.Options.FirstOrDefault(o => o.Id.ToString() == q.CorrectAnswer?.Trim())?.Text,
                QuestionType.MultipleChoice =>
                    string.Join(", ", q.CorrectAnswer?.Split(',')
                        .Select(id => q.Options.FirstOrDefault(o => o.Id.ToString() == id.Trim())?.Text ?? id)
                        ?? []),
                _ => null
            };

            // Map user answer display text for choice types
            string? userDisplay = userAnswer;
            if (hasAnswer && q.Type is QuestionType.SingleChoice)
            {
                userDisplay = q.Options.FirstOrDefault(o => o.Id.ToString() == userAnswer?.Trim())?.Text ?? userAnswer;
            }
            else if (hasAnswer && q.Type is QuestionType.MultipleChoice)
            {
                userDisplay = string.Join(", ", userAnswer!.Split(',')
                    .Select(id => q.Options.FirstOrDefault(o => o.Id.ToString() == id.Trim())?.Text ?? id));
            }

            summaries.Add(new QuestionSummaryItem(
                q.Text,
                q.Type.ToString(),
                userDisplay,
                correctDisplay,
                isCorrect
            ));
        }



        int total = relevantQuestions.Count;
        double pct = total > 0 ? Math.Round((double)correct / total * 100, 1) : 0;

        return new SummaryDto(summaries, new SummaryStats(total, attempted, correct, pct));
    }
}

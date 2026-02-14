using System.Net;
using Quiz_Application.Models.Dtos;
using Quiz_Application.Repositories;

namespace Quiz_Application.Services;

public interface IResponseSvc
{
    Task<(bool Success, int? ResponseId, List<string> Errors)> SaveAsync(Guid sessionId, int questionId, string answerText);
    Task<Dictionary<int, string>> GetBySessionAsync(Guid sessionId);
}

public class ResponseSvc(
    IResponseRepo responseRepo,
    ISessionRepo sessionRepo,
    IValidatorSvc validator) : IResponseSvc
{
    public async Task<(bool Success, int? ResponseId, List<string> Errors)> SaveAsync(
        Guid sessionId, int questionId, string answerText)
    {
        // Sanitise input — HTML-encode to prevent XSS
        var sanitised = WebUtility.HtmlEncode(answerText?.Trim() ?? "");

        // Validate before persisting
        var errors = await validator.ValidateAsync(questionId, sanitised);
        if (errors.Count > 0)
            return (false, null, errors);

        // Ensure the session exists
        await sessionRepo.EnsureExistsAsync(sessionId);

        // Upsert the response
        var responseId = await responseRepo.UpsertAsync(sessionId, questionId, sanitised);
        return (true, responseId, []);
    }

    public async Task<Dictionary<int, string>> GetBySessionAsync(Guid sessionId)
    {
        var responses = await responseRepo.GetAllBySessionAsync(sessionId);
        return responses.ToDictionary(r => r.QuestionId, r => r.AnswerText);
    }
}

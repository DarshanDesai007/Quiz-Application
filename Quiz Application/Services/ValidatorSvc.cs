using System.Text.RegularExpressions;
using Quiz_Application.Models.Enums;
using Quiz_Application.Repositories;

namespace Quiz_Application.Services;

public interface IValidatorSvc
{
    Task<List<string>> ValidateAsync(int questionId, string answerText);
}

public class ValidatorSvc(IQuestionRepo repo) : IValidatorSvc
{
    public async Task<List<string>> ValidateAsync(int questionId, string answerText)
    {
        var allQuestions = await repo.GetAllAsync();
        var q = allQuestions.FirstOrDefault(x => x.Id == questionId);

        if (q is null)
            return ["Question not found."];

        var validOptionIds = q.Options.Select(o => o.Id).ToList();

        return q.Type switch
        {
            QuestionType.SingleChoice => ValidateSingleChoice(answerText, validOptionIds),
            QuestionType.MultipleChoice => ValidateMultipleChoice(answerText, validOptionIds),
            QuestionType.ShortAnswer => ValidateShortAnswer(answerText),
            QuestionType.PhoneNumber => ValidatePhoneNumber(answerText),
            QuestionType.LongAnswer => ValidateLongAnswer(answerText),
            _ => ["Unknown question type."]
        };
    }

    private static List<string> ValidateSingleChoice(string answer, List<int> validIds)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(answer))
        {
            errors.Add("Please select an option.");
            return errors;
        }
        if (!int.TryParse(answer.Trim(), out int selected))
        {
            errors.Add("Invalid option value.");
            return errors;
        }
        if (!validIds.Contains(selected))
            errors.Add("Selected option is not valid for this question.");
        return errors;
    }

    private static List<string> ValidateMultipleChoice(string answer, List<int> validIds)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(answer))
        {
            errors.Add("Please select at least one option.");
            return errors;
        }

        var parts = answer.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            errors.Add("Please select at least one option.");
            return errors;
        }

        foreach (var part in parts)
        {
            if (!int.TryParse(part.Trim(), out int id) || !validIds.Contains(id))
            {
                errors.Add($"Invalid option ID: {part.Trim()}");
            }
        }
        return errors;
    }

    private static List<string> ValidateShortAnswer(string answer)
    {
        return string.IsNullOrWhiteSpace(answer)
            ? ["Answer cannot be blank."]
            : [];
    }

    private static List<string> ValidatePhoneNumber(string answer)
    {
        return Regex.IsMatch(answer?.Trim() ?? "", @"^\d{10}$")
            ? []
            : ["Phone number must be exactly 10 digits."];
    }

    private static List<string> ValidateLongAnswer(string answer)
    {
        return (answer?.Trim().Length ?? 0) >= 10
            ? []
            : ["Answer must be at least 10 characters."];
    }
}

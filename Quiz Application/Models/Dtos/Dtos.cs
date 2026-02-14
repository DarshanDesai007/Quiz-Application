namespace Quiz_Application.Models.Dtos;

public record QuestionGridDto(
    int QuestionId,
    int OrderNo,
    string QuestionText,
    string QuestionType,
    string? CorrectAnswer
);

public record QuestionDetailDto(
    int QuestionId,
    int OrderNo,
    string QuestionText,
    string QuestionType,
    List<OptionDto>? Options
);

public record OptionDto(int OptionId, string OptionText);

public record UserResponseDto(int ResponseId, int QuestionId, string AnswerText);

public record SaveResponseReq(Guid SessionId, int QuestionId, string AnswerText);

public record SummaryDto(List<QuestionSummaryItem> QuestionSummaries, SummaryStats Stats);

public record QuestionSummaryItem(
    string QuestionText,
    string QuestionType,
    string? UserAnswer,
    string? CorrectAnswer,
    bool? IsCorrect
);

public record SummaryStats(int Total, int Attempted, int Correct, double Percentage);

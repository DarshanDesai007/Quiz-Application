using Quiz_Application.Models.Enums;

namespace Quiz_Application.Models.Entities;

public class Question
{
    public int Id { get; set; }
    public int OrderNo { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string? CorrectAnswer { get; set; }

    public List<QuestionOption> Options { get; set; } = [];
}

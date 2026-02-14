namespace Quiz_Application.Models.Entities;

public class QuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;

    public Question Question { get; set; } = null!;
}

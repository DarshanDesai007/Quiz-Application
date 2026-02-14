namespace Quiz_Application.Models.Entities;

public class UserResponse
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public int QuestionId { get; set; }
    public string AnswerText { get; set; } = string.Empty;

    public UserSession Session { get; set; } = null!;
    public Question Question { get; set; } = null!;
}

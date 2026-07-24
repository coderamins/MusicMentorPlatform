namespace MusicMentor.Application.DTOs.Auth;

public class RegisterStudentRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? District { get; set; }
    public string? LearningGoal { get; set; }
}

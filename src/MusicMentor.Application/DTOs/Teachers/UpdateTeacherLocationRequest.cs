namespace MusicMentor.Application.DTOs.Teachers;

public class UpdateTeacherLocationRequest
{
    /// <summary>عرض جغرافیایی، بین ۹۰- تا ۹۰ (معمولاً از GPS مرورگر گرفته می‌شه)</summary>
    public double Latitude { get; set; }

    /// <summary>طول جغرافیایی، بین ۱۸۰- تا ۱۸۰</summary>
    public double Longitude { get; set; }
}

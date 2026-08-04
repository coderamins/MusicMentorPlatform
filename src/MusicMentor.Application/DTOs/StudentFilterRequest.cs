namespace MusicMentor.Application.DTOs.Students;

public enum StudentSortOption
{
    /// <summary>جدیدترین هنرجوهای ثبت‌نام‌شده اول</summary>
    Newest = 0,

    /// <summary>ترتیب الفبایی نام</summary>
    NameAsc = 1,
}

/// <summary>
/// پارامترهای فیلتر لیست هنرجوها. از QueryString در Controller بایند می‌شود.
/// </summary>
public class StudentFilterRequest
{
    /// <summary>جستجوی آزاد در نام هنرجو</summary>
    public string? Search { get; set; }

    public string? City { get; set; }
    public string? District { get; set; }

    public StudentSortOption SortBy { get; set; } = StudentSortOption.Newest;

    private const int MaxPageSize = 50;
    private int _pageSize = 12;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 12 : Math.Min(value, MaxPageSize);
    }
}

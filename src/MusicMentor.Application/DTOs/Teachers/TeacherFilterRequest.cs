namespace MusicMentor.Application.DTOs.Teachers;

/// <summary>
/// پارامترهای فیلتر لیست اساتید. از QueryString در Controller بایند می‌شود.
/// </summary>
public class TeacherFilterRequest
{
    /// <summary>جستجوی آزاد در نام و بیوگرافی استاد</summary>
    public string? Search { get; set; }

    public string? City { get; set; }
    public string? District { get; set; }

    /// <summary>فیلتر بر اساس یک یا چند حوزه/ساز (شناسه MusicCategory)</summary>
    public List<int>? MusicCategoryIds { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public int? MinExperienceYears { get; set; }

    /// <summary>فقط اساتید تاییدشده توسط ادمین</summary>
    public bool? OnlyVerified { get; set; }

    public double? Latitude { get; set; }

    /// <summary>طول جغرافیایی هنرآموز</summary>
    public double? Longitude { get; set; }

    /// <summary>حداکثر شعاع جستجو به کیلومتر (فقط وقتی Latitude/Longitude داده شده اعمال می‌شه)</summary>
    public double? RadiusKm { get; set; }


    public TeacherSortOption SortBy { get; set; } = TeacherSortOption.MostPopular;

    private const int MaxPageSize = 50;
    private int _pageSize = 12;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 12 : Math.Min(value, MaxPageSize);
    }
}

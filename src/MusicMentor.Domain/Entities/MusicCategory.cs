namespace MusicMentor.Domain.Entities;

/// <summary>
/// حوزه/سازی که استاد در آن تدریس می‌کند، مثل «گیتار»، «پیانو»، «آواز کلاسیک».
/// این جدول به صورت مرجع (Lookup) توسط ادمین مدیریت می‌شود.
/// </summary>
public class MusicCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public ICollection<TeacherMusicCategory> TeacherCategories { get; set; } = new List<TeacherMusicCategory>();
}

/// <summary>
/// جدول واسط چند-به-چند بین استاد و حوزه‌های تدریس (یک استاد می‌تواند چند ساز/سبک تدریس کند)
/// </summary>
public class TeacherMusicCategory
{
    public Guid TeacherProfileId { get; set; }
    public TeacherProfile TeacherProfile { get; set; } = default!;

    public int MusicCategoryId { get; set; }
    public MusicCategory MusicCategory { get; set; } = default!;
}

namespace MusicMentor.Application.DTOs.Teachers;

public enum TeacherSortOption
{
    /// <summary>محبوب‌ترین‌ها اول (بر اساس امتیاز و تعداد نظرات)</summary>
    MostPopular = 0,

    /// <summary>بیشترین سابقه اول</summary>
    MostExperienced = 1,

    /// <summary>ارزان‌ترین شهریه اول</summary>
    PriceLowToHigh = 2,

    /// <summary>گران‌ترین شهریه اول</summary>
    PriceHighToLow = 3,

    /// <summary>جدیدترین اساتید ثبت‌نام‌شده</summary>
    Newest = 4,

    NearestFirst,
}

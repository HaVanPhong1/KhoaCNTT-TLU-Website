using KhoaCNTT.Domain.Enums;

namespace KhoaCNTT.Application.DTOs.Lecturer
{
    /// <summary>Tham số tìm kiếm giảng viên (cho Sinh viên / Khách).</summary>
    public class LecturerSearchParams
    {
        public string? Name { get; set; }
        public DegreeType? Degree { get; set; }
        public string? Position { get; set; }
        /// <summary>Mã môn học hoặc tên môn (tìm theo môn giảng dạy).</summary>
        public string? SubjectCodeOrName { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedLecturerResult
    {
        public List<LecturerResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

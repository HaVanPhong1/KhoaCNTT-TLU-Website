using KhoaCNTT.Domain.Enums;

namespace KhoaCNTT.Application.DTOs.Lecturer
{
    public class LecturerResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DegreeType Degree { get; set; }
        public string Position { get; set; } = string.Empty;
        public DateTime? Birthdate { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        /// <summary>Danh sách môn học giảng dạy (mã + tên).</summary>
        public List<SubjectBriefDto> Subjects { get; set; } = new();
    }

    public class SubjectBriefDto
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
    }
}

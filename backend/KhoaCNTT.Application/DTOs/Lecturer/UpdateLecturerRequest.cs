using KhoaCNTT.Domain.Enums;

namespace KhoaCNTT.Application.DTOs.Lecturer
{
    public class UpdateLecturerRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DegreeType Degree { get; set; }
        public string Position { get; set; } = string.Empty;
        public DateTime? Birthdate { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        /// <summary>Danh sách mã môn học giảng dạy.</summary>
        public List<string> SubjectCodes { get; set; } = new();
    }
}

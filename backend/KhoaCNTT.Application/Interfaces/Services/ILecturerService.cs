using KhoaCNTT.Application.DTOs.Lecturer;

namespace KhoaCNTT.Application.Interfaces.Services
{
    public interface ILecturerService
    {
        /// <summary>Lấy danh sách giảng viên có phân trang và tìm kiếm (dùng cho Sinh viên / Khách).</summary>
        Task<PagedLecturerResult> GetListAsync(LecturerSearchParams searchParams);
        /// <summary>Lấy chi tiết một giảng viên theo Id.</summary>
        Task<LecturerResponse?> GetByIdAsync(int id);
        Task CreateLecturerAsync(CreateLecturerRequest request);
        Task UpdateLecturerAsync(int id, UpdateLecturerRequest request);
        Task DeleteLecturerAsync(int id);
    }
}

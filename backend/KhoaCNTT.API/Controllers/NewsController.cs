using KhoaCNTT.Application.DTOs;
using KhoaCNTT.Application.DTOs.News;
using KhoaCNTT.Application.Interfaces.Repositories;
using KhoaCNTT.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KhoaCNTT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly IAdminRepository _adminRepo;

        public NewsController(INewsService newsService, IAdminRepository adminRepo)
        {
            _newsService = newsService;
            _adminRepo = adminRepo;
        }

        // ── Helpers phân quyền ────────────────────────────────────

        /// <summary>Lấy cấp độ admin từ JWT claim</summary>
        private int GetAdminLevel()
        {
            var levelStr = User.FindFirst("Level")?.Value;
            return int.TryParse(levelStr, out int level) ? level : 0;
        }

        /// <summary>
        /// Cấp 1 (Root Admin) và Cấp 2 (Manager):
        /// Tạo/Sửa tin tức → tự duyệt luôn, Xóa tin tức, Duyệt tin tức
        /// </summary>
        private bool IsLevel1Or2() => GetAdminLevel() is 1 or 2;

        /// <summary>
        /// Cấp 1, 2, 3 đều có quyền:
        /// Tạo/Sửa tin tức (cấp 3 → vào hàng chờ duyệt), Xóa bình luận
        /// </summary>
        private bool IsAnyAdminLevel() => GetAdminLevel() is 1 or 2 or 3;

        /// <summary>Lấy AdminId thực từ DB qua username trong JWT</summary>
        private async Task<int> GetCurrentAdminIdAsync()
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(username)) return 0;
            var admin = await _adminRepo.GetByUsernameAsync(username);
            return admin?.Id ?? 0;
        }

        // ── Public (không cần đăng nhập) ─────────────────────────

        // GET: api/News
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _newsService.GetAllNewsAsync();
            return Ok(result);
        }

        // GET: api/News/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _newsService.GetNewsByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message, Detail = ex.InnerException?.Message });
            }
        }

        // ── Tạo tin tức ───────────────────────────────────────────
        // Tác nhân: Cấp 1, 2, 3
        // Cấp 1 & 2 → tự duyệt luôn (isSenior = true)
        // Cấp 3     → vào hàng chờ duyệt (isSenior = false)
        // POST: api/News/requests/create
        [HttpPost("requests/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SubmitCreate([FromBody] CreateNewsRequest dto)
        {
            if (!IsAnyAdminLevel()) return Forbid();

            try
            {
                var adminId = await GetCurrentAdminIdAsync();
                var result = await _newsService.SubmitCreateRequestAsync(dto, adminId, IsLevel1Or2());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message, Detail = ex.InnerException?.Message });
            }
        }

        // ── Sửa tin tức ───────────────────────────────────────────
        // Tác nhân: Cấp 1, 2, 3
        // Cấp 1 & 2 → tự duyệt luôn (isSenior = true)
        // Cấp 3     → vào hàng chờ duyệt (isSenior = false)
        // POST: api/News/requests/update
        [HttpPost("requests/update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SubmitUpdate([FromBody] UpdateNewsRequest dto)
        {
            if (!IsAnyAdminLevel()) return Forbid();

            try
            {
                var adminId = await GetCurrentAdminIdAsync();
                var result = await _newsService.SubmitReplaceRequestAsync(dto, adminId, IsLevel1Or2());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message, Detail = ex.InnerException?.Message });
            }
        }

        // ── Xóa tin tức ───────────────────────────────────────────
        // Tác nhân: Cấp 1, 2 (cấp 3 KHÔNG được xóa)
        // DELETE: api/News/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLevel1Or2())
                return Forbid();

            try
            {
                await _newsService.DeleteNewsAsync(id);
                return Ok(new { Message = "Đã xóa tin tức" });
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message, Detail = ex.InnerException?.Message });
            }
        }

        // ── Xóa bình luận ─────────────────────────────────────────
        // Tác nhân: Cấp 1, 2, 3 đều được xóa bình luận vi phạm
        // DELETE: api/News/comments/{commentId}
        [HttpDelete("comments/{commentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            if (!IsAnyAdminLevel())
                return Forbid();

            try
            {
                await _newsService.DeleteCommentAsync(commentId);
                return Ok(new { Message = "Đã xóa bình luận" });
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message, Detail = ex.InnerException?.Message });
            }
        }

        // ── Phê duyệt ─────────────────────────────────────────────
        // Tác nhân: Cấp 1, 2 (cấp 3 KHÔNG được duyệt)

        // GET: api/News/requests/pending
        [HttpGet("requests/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingRequests()
        {
            // Cấp 3 được xem danh sách (chỉ không duyệt/từ chối — kiểm soát ở FE và endpoint Approve)
            if (!IsAnyAdminLevel())
                return Forbid();

            var result = await _newsService.GetPendingRequestsAsync();
            return Ok(result);
        }

        // PUT: api/News/requests/{id}/approve
        [HttpPut("requests/{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveNewsRequest dto)
        {
            if (!IsLevel1Or2())
                return Forbid();

            try
            {
                dto.NewsRequestID = id;
                var approverId = await GetCurrentAdminIdAsync();
                var result = await _newsService.ProcessApprovalAsync(dto, approverId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message, Detail = ex.InnerException?.Message });
            }
        }
    }
}
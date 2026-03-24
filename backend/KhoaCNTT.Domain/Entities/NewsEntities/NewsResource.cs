using KhoaCNTT.Domain.Common;
using KhoaCNTT.Domain.Entities;

namespace KhoaCNTT.Domain.Entities.NewsEntities
{
    public class NewsResource : BaseEntity
    {
        /// <summary>Nội dung chi tiết bài viết (HTML hoặc plain text)</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>Mô tả ngắn hiển thị ở danh sách/trang chủ</summary>
        public string ResourceContent { get; set; } = string.Empty;

        /// <summary>Ảnh đại diện bài viết (URL hoặc base64)</summary>
        public string ImageUrl { get; set; } = string.Empty;

        public long Size { get; set; }

        // Foreign key
        public int CreatedBy { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Admin Admin { get; set; } = null!;
    }
}
using KhoaCNTT.Domain.Common;
using KhoaCNTT.Domain.Enums;

namespace KhoaCNTT.Domain.Entities.NewsEntities
{
    public class News : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public int CurrentResourceId { get; set; }
        public NewsResource CurrentResource { get; set; } = null!;
        public NewsType NewsType { get; set; }
        public int ViewCount { get; set; } = 0;

        // Khóa ngoại Admin
        public int CreatedById { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Admin CreatedBy { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
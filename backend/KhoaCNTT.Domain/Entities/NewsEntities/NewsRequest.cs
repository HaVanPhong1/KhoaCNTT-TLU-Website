using KhoaCNTT.Domain.Entities.NewsEntities;
using KhoaCNTT.Domain.Enums;
using KhoaCNTT.Domain.Common;

namespace KhoaCNTT.Domain.Entities.NewsEntities
{
    public class NewsRequest : BaseEntity
    {
        public RequestType RequestType { get; set; }
        public bool IsProcessed { get; set; } = false;

        public string Title { get; set; } = string.Empty;
        public NewsType NewsType { get; set; }

        public int? TargetNewsId { get; set; }
        public News? TargetNews { get; set; }

        // Resource mới (được upload lên)
        public int NewResourceId { get; set; }
        public NewsResource NewResource { get; set; } = null!;
        // AdminID từ new resource là người tạo yêu cầu này

        // Resource cũ (nếu là replace)
        public int? OldResourceId { get; set; }
        public NewsResource? OldResource { get; set; }
    }
}
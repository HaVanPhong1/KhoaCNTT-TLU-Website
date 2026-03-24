using KhoaCNTT.Domain.Entities.NewsEntities;
using KhoaCNTT.Domain.Common;

namespace KhoaCNTT.Domain.Entities.NewsEntities
{
    public class NewsApproval : BaseEntity
    {
        public int NewsRequestId { get; set; }
        public NewsRequest NewsRequest { get; set; } = null!;

        public int ApproverId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Admin Admin { get; set; } = null!;

        public ApprovalDecision Decision { get; set; }
        public string? Reason { get; set; }
    }
}
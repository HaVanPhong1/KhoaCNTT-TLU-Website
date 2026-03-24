using KhoaCNTT.Domain.Enums;

namespace KhoaCNTT.Application.DTOs.News;

public class NewsResourceResponse
{
    public int NewsResourceID { get; set; }
    public string Content { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class NewsRequestResponse
{
    public int NewsRequestID { get; set; }
    public int? TargetNewsID { get; set; }
    public string Title { get; set; } = string.Empty;
    public NewsType NewsType { get; set; }
    public string Content { get; set; } = string.Empty;
    public RequestType RequestType { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class NewsApprovalResponse
{
    public int NewsApprovalID { get; set; }
    public int ApproverID { get; set; }
    public int NewsRequestID { get; set; }
    public ApprovalDecision Decision { get; set; }
    public string? RejectReason { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class ApproveNewsRequest
{
    public int NewsRequestID { get; set; }
    public ApprovalDecision Decision { get; set; }
    public string? RejectReason { get; set; }
}

public class DeleteNewsRequest
{
    public int NewsID { get; set; }
}
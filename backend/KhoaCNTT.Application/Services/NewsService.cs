using KhoaCNTT.Application.Common.Exceptions;
using KhoaCNTT.Application.DTOs;
using KhoaCNTT.Application.DTOs.News;
using KhoaCNTT.Application.Interfaces.Repositories;
using KhoaCNTT.Application.Interfaces.Repositories.INewsRepositories;
using KhoaCNTT.Application.Interfaces.Services;
using KhoaCNTT.Domain.Entities.NewsEntities;
using KhoaCNTT.Domain.Enums;

namespace KhoaCNTT.Application.Services;

public class NewsService : INewsService
{
    private readonly INewsRepository _newsRepo;
    private readonly INewsResourceRepository _newsResourceRepo;
    private readonly INewsRequestRepository _newsRequestRepo;
    private readonly INewsApprovalRepository _newsApprovalRepo;
    private readonly ICommentRepository _commentRepo;

    public NewsService(
        INewsRepository newsRepo,
        INewsResourceRepository newsResourceRepo,
        INewsRequestRepository newsRequestRepo,
        INewsApprovalRepository newsApprovalRepo,
        ICommentRepository commentRepo)
    {
        _newsRepo = newsRepo;
        _newsResourceRepo = newsResourceRepo;
        _newsRequestRepo = newsRequestRepo;
        _newsApprovalRepo = newsApprovalRepo;
        _commentRepo = commentRepo;
    }

    // ── Read ─────────────────────────────────────────────────────

    public async Task<IEnumerable<NewsResponse>> GetAllNewsAsync()
    {
        var news = await _newsRepo.GetAllWithResourceAsync();
        return news.Select(MapToResponse);
    }

    private static readonly Dictionary<string, DateTime> _viewCooldown = new();

    public async Task<NewsResponse> GetNewsByIdAsync(int id)
    {
        var news = await _newsRepo.GetByIdWithResourceAsync(id)
            ?? throw new NotFoundException(nameof(Domain.Entities.NewsEntities.News), id);

        // Cooldown 5 giây — tránh React Strict Mode gọi 2 lần tăng 2 view
        var cooldownKey = $"view_{id}";
        var now = DateTime.UtcNow;
        if (!_viewCooldown.TryGetValue(cooldownKey, out var lastView) || (now - lastView).TotalSeconds > 5)
        {
            _viewCooldown[cooldownKey] = now;
            await _newsRepo.IncrementViewCountAsync(id);
            news.ViewCount++;
        }

        return MapToResponse(news);
    }

    public async Task<IEnumerable<NewsRequestResponse>> GetPendingRequestsAsync()
    {
        var requests = await _newsRequestRepo.GetPendingAsync();
        return requests.Select(MapRequestToResponse);
    }

    // ── Tạo tin tức ──────────────────────────────────────────────

    public async Task<NewsRequestResponse> SubmitCreateRequestAsync(
        CreateNewsRequest dto, int submitterId, bool isSenior)
    {
        var ct = CancellationToken.None;

        var resource = new NewsResource
        {
            Content = dto.Content,
            ResourceContent = dto.ResourceContent,
            ImageUrl = dto.ImageUrl ?? string.Empty,
            CreatedBy = submitterId,
            CreatedAt = DateTime.UtcNow
        };
        await _newsResourceRepo.AddAsync(resource);

        var request = new NewsRequest
        {
            TargetNewsId = null,
            NewResourceId = resource.Id,
            OldResourceId = null,
            Title = dto.Title,
            NewsType = dto.NewsType,
            RequestType = RequestType.CreateNew,
            IsProcessed = false,
            CreatedAt = DateTime.UtcNow
        };
        await _newsRequestRepo.AddAsync(request);

        if (isSenior)
            await ApproveRequestInternalAsync(submitterId, request.Id, ApprovalDecision.Approved, null);

        return MapRequestToResponse(request);
    }

    // ── Sửa tin tức ──────────────────────────────────────────────

    public async Task<NewsRequestResponse> SubmitReplaceRequestAsync(
        UpdateNewsRequest dto, int submitterId, bool isSenior)
    {
        var ct = CancellationToken.None;

        var existingNews = await _newsRepo.GetByIdWithResourceAsync(dto.TargetNewsID)
            ?? throw new NotFoundException(nameof(Domain.Entities.NewsEntities.News), dto.TargetNewsID);

        var newResource = new NewsResource
        {
            Content = dto.Content,
            ResourceContent = dto.ResourceContent,
            ImageUrl = dto.ImageUrl ?? string.Empty,
            CreatedBy = submitterId,
            CreatedAt = DateTime.UtcNow
        };
        await _newsResourceRepo.AddAsync(newResource);

        var request = new NewsRequest
        {
            TargetNewsId = dto.TargetNewsID,
            NewResourceId = newResource.Id,
            OldResourceId = existingNews.CurrentResourceId,
            Title = dto.Title,
            NewsType = dto.NewsType,
            RequestType = RequestType.Replace,
            IsProcessed = false,
            CreatedAt = DateTime.UtcNow
        };
        await _newsRequestRepo.AddAsync(request);

        if (isSenior)
            await ApproveRequestInternalAsync(submitterId, request.Id, ApprovalDecision.Approved, null);

        return MapRequestToResponse(request);
    }

    // ── Xóa tin tức ──────────────────────────────────────────────

    public async Task DeleteNewsAsync(int newsId)
    {
        var news = await _newsRepo.GetByIdWithResourceAsync(newsId)
            ?? throw new NotFoundException(nameof(Domain.Entities.NewsEntities.News), newsId);

        await _newsRepo.DeleteAsync(news);
    }

    // ── Phê duyệt ────────────────────────────────────────────────

    public async Task<NewsApprovalResponse> ProcessApprovalAsync(
        ApproveNewsRequest dto, int approverId)
    {
        return await ApproveRequestInternalAsync(
            approverId, dto.NewsRequestID, dto.Decision, dto.RejectReason);
    }

    // ── Private helpers ──────────────────────────────────────────

    private async Task<NewsApprovalResponse> ApproveRequestInternalAsync(
        int approverId, int requestId, ApprovalDecision decision, string? rejectReason)
    {
        var ct = CancellationToken.None;

        var newsRequest = await _newsRequestRepo.GetByIdWithDetailsAsync(requestId)
            ?? throw new NotFoundException(nameof(NewsRequest), requestId);

        if (newsRequest.IsProcessed)
            throw new BusinessRuleException("Yêu cầu này đã được xử lý.");

        var approval = new NewsApproval
        {
            ApproverId = approverId,
            NewsRequestId = requestId,
            Decision = decision,
            Reason = rejectReason,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _newsApprovalRepo.AddAsync(approval);

        if (decision == ApprovalDecision.Approved)
        {
            if (newsRequest.RequestType == RequestType.CreateNew)
            {
                var news = new News
                {
                    Title = newsRequest.Title,
                    CurrentResourceId = newsRequest.NewResourceId,
                    ViewCount = 0,
                    NewsType = newsRequest.NewsType,
                    CreatedById = approverId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _newsRepo.AddAsync(news);
            }
            else // Replace
            {
                var targetNews = await _newsRepo.GetByIdWithResourceAsync(newsRequest.TargetNewsId!.Value)
                    ?? throw new NotFoundException(nameof(Domain.Entities.NewsEntities.News), newsRequest.TargetNewsId);

                targetNews.Title = newsRequest.Title;
                targetNews.NewsType = newsRequest.NewsType;
                targetNews.CurrentResourceId = newsRequest.NewResourceId;
                targetNews.UpdatedAt = DateTime.UtcNow;
                await _newsRepo.UpdateAsync(targetNews);
            }
        }

        newsRequest.IsProcessed = true;
        await _newsRequestRepo.UpdateAsync(newsRequest);

        return new NewsApprovalResponse
        {
            NewsApprovalID = approval.Id,
            ApproverID = approval.ApproverId,
            NewsRequestID = approval.NewsRequestId,
            Decision = approval.Decision,
            RejectReason = approval.Reason,
            CreatedAt = approval.CreatedAt
        };
    }

    // ── Mappers ───────────────────────────────────────────────────

    private static NewsResponse MapToResponse(News n) => new()
    {
        NewsID = n.Id,
        Title = n.Title,
        Content = n.CurrentResource?.Content ?? string.Empty,
        ResourceContent = n.CurrentResource?.ResourceContent ?? string.Empty,
        ImageUrl = n.CurrentResource?.ImageUrl ?? string.Empty,
        ViewCount = n.ViewCount,
        NewsType = n.NewsType,
        CreatedBy = n.CreatedById,
        CreatedAt = n.CreatedAt,
        UpdatedAt = n.UpdatedAt
    };

    private static NewsRequestResponse MapRequestToResponse(NewsRequest r) => new()
    {
        NewsRequestID = r.Id,
        TargetNewsID = r.TargetNewsId,
        Title = r.Title,
        NewsType = r.NewsType,
        Content = r.NewResource?.Content ?? string.Empty,
        RequestType = r.RequestType,
        IsProcessed = r.IsProcessed,
        CreatedAt = r.CreatedAt
    };

    public async Task DeleteCommentAsync(int commentId)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId)
            ?? throw new NotFoundException(nameof(Comment), commentId);

        await _commentRepo.DeleteAsync(comment);
    }
}
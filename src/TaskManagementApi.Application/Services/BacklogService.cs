using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.Backlog;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Services
{
    public class BacklogService : IBacklogService
    {
        private readonly IRepository<BacklogItem> _backlogRepository;
        private readonly IRepository<BacklogItemVersion> _versionRepository;
        private readonly IRepository<BacklogAttachment> _attachmentRepository;
        private readonly IRepository<BacklogItemTicketLink> _ticketLinkRepository;
        private readonly IRepository<BacklogApprovalRequest> _approvalRepository;
        private readonly IFileStorageService _fileStorage;

        public BacklogService(
            IRepository<BacklogItem> backlogRepository,
            IRepository<BacklogItemVersion> versionRepository,
            IRepository<BacklogAttachment> attachmentRepository,
            IRepository<BacklogItemTicketLink> ticketLinkRepository,
            IRepository<BacklogApprovalRequest> approvalRepository,
            IFileStorageService fileStorage)
        {
            _backlogRepository = backlogRepository;
            _versionRepository = versionRepository;
            _attachmentRepository = attachmentRepository;
            _ticketLinkRepository = ticketLinkRepository;
            _approvalRepository = approvalRepository;
            _fileStorage = fileStorage;
        }

        public async Task<BacklogItemDto?> GetByIdAsync(int id)
        {
            var item = await _backlogRepository.Query()
                .Include(i => i.Owner)
                .Include(i => i.Attachments)
                .Include(i => i.TicketLinks).ThenInclude(tl => tl.Ticket)
                .Include(i => i.Versions)
                .FirstOrDefaultAsync(i => i.Id == id);

            return item != null ? MapToDto(item) : null;
        }

        public async Task<IEnumerable<BacklogItemDto>> GetProjectBacklogAsync(int projectId, BacklogFilterRequest filter)
        {
            var query = _backlogRepository.Query()
                .Include(i => i.Owner)
                .Where(i => i.ProjectId == projectId);

            query = ApplyFilter(query, filter);

            var items = await query.OrderBy(i => i.Order).ToListAsync();
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<BacklogItemDto>> GetProductBacklogAsync(int productId, BacklogFilterRequest filter)
        {
            var query = _backlogRepository.Query()
                .Include(i => i.Owner)
                .Where(i => i.ProductId == productId);

            query = ApplyFilter(query, filter);

            var items = await query.OrderBy(i => i.Order).ToListAsync();
            return items.Select(MapToDto);
        }

        public async Task<BacklogItemDto> CreateAsync(CreateBacklogItemRequest request, int createdByUserId)
        {
            var maxOrder = 0;
            if (request.ProjectId.HasValue)
            {
                maxOrder = await _backlogRepository.Query()
                    .Where(b => b.ProjectId == request.ProjectId)
                    .Select(b => (int?)b.Order)
                    .MaxAsync() ?? 0;
            }
            else if (request.ProductId.HasValue)
            {
                maxOrder = await _backlogRepository.Query()
                    .Where(b => b.ProductId == request.ProductId)
                    .Select(b => (int?)b.Order)
                    .MaxAsync() ?? 0;
            }

            var item = new BacklogItem
            {
                Title = request.Title,
                Description = request.Description,
                Type = request.Type,
                Status = BacklogItemStatus.Draft,
                Priority = request.Priority,
                ProjectId = request.ProjectId,
                ProductId = request.ProductId,
                OwnerId = createdByUserId,
                Order = maxOrder + 1,
                AcceptanceCriteria = request.AcceptanceCriteria,
                CreatedAt = DateTime.UtcNow
            };

            await _backlogRepository.AddAsync(item);

            await _versionRepository.AddAsync(new BacklogItemVersion
            {
                BacklogItemId = item.Id,
                VersionNumber = 1,
                Title = item.Title,
                Description = item.Description,
                AcceptanceCriteria = item.AcceptanceCriteria,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                ChangeNote = "Initial version"
            });

            return (await GetByIdAsync(item.Id))!;
        }

        public async Task<BacklogItemDto> UpdateAsync(int id, UpdateBacklogItemRequest request, int updatedByUserId)
        {
            var item = await _backlogRepository.GetByIdAsync(id);
            if (item == null) throw new KeyNotFoundException($"Backlog item {id} not found");

            var latestVersion = await _versionRepository.Query()
                .Where(v => v.BacklogItemId == id)
                .Select(v => (int?)v.VersionNumber)
                .MaxAsync() ?? 0;

            item.Title = request.Title;
            item.Description = request.Description;
            item.Priority = request.Priority;
            item.AcceptanceCriteria = request.AcceptanceCriteria;
            item.UpdatedAt = DateTime.UtcNow;

            if (item.Status == BacklogItemStatus.Approved)
                item.Status = BacklogItemStatus.Draft;

            await _backlogRepository.UpdateAsync(item);

            await _versionRepository.AddAsync(new BacklogItemVersion
            {
                BacklogItemId = id,
                VersionNumber = latestVersion + 1,
                Title = request.Title,
                Description = request.Description,
                AcceptanceCriteria = request.AcceptanceCriteria,
                CreatedByUserId = updatedByUserId,
                CreatedAt = DateTime.UtcNow,
                ChangeNote = request.ChangeNote
            });

            return (await GetByIdAsync(id))!;
        }

        public async Task DeleteAsync(int id, int deletedByUserId)
        {
            var attachments = await _attachmentRepository.Query()
                .Where(a => a.BacklogItemId == id)
                .ToListAsync();

            foreach (var attachment in attachments)
            {
                await _fileStorage.DeleteFileAsync(attachment.FilePath);
            }

            await _backlogRepository.DeleteAsync(id);
        }

        public async Task ReorderAsync(int projectId, ReorderBacklogRequest request)
        {
            foreach (var itemOrder in request.Items)
            {
                var backlogItem = await _backlogRepository.GetByIdAsync(itemOrder.Id);
                if (backlogItem != null && backlogItem.ProjectId == projectId)
                {
                    backlogItem.Order = itemOrder.Order;
                    await _backlogRepository.UpdateAsync(backlogItem);
                }
            }
        }

        public async Task<IEnumerable<BacklogItemVersionDto>> GetVersionHistoryAsync(int id)
        {
            var versions = await _versionRepository.Query()
                .Include(v => v.CreatedBy)
                .Where(v => v.BacklogItemId == id)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();

            return versions.Select(v => new BacklogItemVersionDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                Title = v.Title,
                Description = v.Description,
                AcceptanceCriteria = v.AcceptanceCriteria,
                ChangeNote = v.ChangeNote,
                CreatedAt = v.CreatedAt,
                CreatedBy = MapToUserSummary(v.CreatedBy)
            });
        }

        public async Task<BacklogItemVersionDto?> GetVersionAsync(int id, int versionNumber)
        {
            var v = await _versionRepository.Query()
                .Include(v => v.CreatedBy)
                .FirstOrDefaultAsync(v => v.BacklogItemId == id && v.VersionNumber == versionNumber);

            return v != null ? new BacklogItemVersionDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                Title = v.Title,
                Description = v.Description,
                AcceptanceCriteria = v.AcceptanceCriteria,
                ChangeNote = v.ChangeNote,
                CreatedAt = v.CreatedAt,
                CreatedBy = MapToUserSummary(v.CreatedBy)
            } : null;
        }

        public async Task<BacklogAttachmentDto> AddAttachmentAsync(int id, IFormFile file, int uploadedByUserId)
        {
            if (file.Length > 25 * 1024 * 1024)
                throw new InvalidOperationException("File size cannot exceed 25MB");

            var subPath = Path.Combine("backlog", id.ToString());
            var relativeFilePath = await _fileStorage.SaveFileAsync(file, subPath);

            var attachment = new BacklogAttachment
            {
                BacklogItemId = id,
                FileName = file.FileName,
                FilePath = relativeFilePath,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedByUserId = uploadedByUserId,
                UploadedAt = DateTime.UtcNow
            };

            await _attachmentRepository.AddAsync(attachment);
            return MapToAttachmentDto(attachment);
        }

        public async Task DeleteAttachmentAsync(int id, int attachmentId)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
            if (attachment != null && attachment.BacklogItemId == id)
            {
                await _fileStorage.DeleteFileAsync(attachment.FilePath);
                await _attachmentRepository.DeleteAsync(attachmentId);
            }
        }

        public async Task<BacklogAttachmentDto?> GetAttachmentAsync(int attachmentId)
        {
            var a = await _attachmentRepository.Query().Include(a => a.UploadedBy).FirstOrDefaultAsync(a => a.Id == attachmentId);
            return a != null ? MapToAttachmentDto(a) : null;
        }

        public async Task LinkToTicketAsync(int id, int ticketId, int linkedByUserId)
        {
            var exists = await _ticketLinkRepository.Query().AnyAsync(l => l.BacklogItemId == id && l.TicketId == ticketId);
            if (!exists)
            {
                await _ticketLinkRepository.AddAsync(new BacklogItemTicketLink
                {
                    BacklogItemId = id,
                    TicketId = ticketId,
                    LinkedAt = DateTime.UtcNow,
                    LinkedByUserId = linkedByUserId
                });
            }
        }

        public async Task UnlinkFromTicketAsync(int id, int ticketId)
        {
            var link = await _ticketLinkRepository.Query().FirstOrDefaultAsync(l => l.BacklogItemId == id && l.TicketId == ticketId);
            if (link != null) await _ticketLinkRepository.DeleteAsync(link.Id);
        }

        public async Task<IEnumerable<BacklogItemDto>> GetLinkedItemsForTicketAsync(int ticketId)
        {
            var links = await _ticketLinkRepository.Query()
                .Include(l => l.BacklogItem).ThenInclude(i => i.Owner)
                .Where(l => l.TicketId == ticketId)
                .ToListAsync();

            return links.Select(l => MapToDto(l.BacklogItem));
        }

        public async Task<BacklogApprovalRequestDto> SubmitForApprovalAsync(int id, int requestedByUserId)
        {
            var existing = await _approvalRepository.Query().FirstOrDefaultAsync(a => a.BacklogItemId == id && a.Status == ApprovalRequestStatus.Pending);
            if (existing != null) throw new InvalidOperationException("This item already has a pending approval request");

            var approval = new BacklogApprovalRequest
            {
                BacklogItemId = id,
                RequestedByUserId = requestedByUserId,
                Status = ApprovalRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };
            await _approvalRepository.AddAsync(approval);
            return MapToApprovalDto(approval);
        }

        public async Task<BacklogApprovalRequestDto> ApproveAsync(int id, int approvalRequestId, string? note, int reviewedByUserId)
        {
            var approval = await _approvalRepository.GetByIdAsync(approvalRequestId);
            if (approval == null || approval.BacklogItemId != id) throw new KeyNotFoundException("Approval request not found");
            if (approval.Status != ApprovalRequestStatus.Pending) throw new InvalidOperationException("Only pending approval requests can be approved");

            approval.Status = ApprovalRequestStatus.Approved;
            approval.ReviewNote = note;
            approval.ReviewedByUserId = reviewedByUserId;
            approval.ReviewedAt = DateTime.UtcNow;
            await _approvalRepository.UpdateAsync(approval);

            var item = await _backlogRepository.GetByIdAsync(id);
            if (item != null)
            {
                item.Status = BacklogItemStatus.Approved;
                item.ApprovedByUserId = reviewedByUserId;
                item.ApprovedAt = DateTime.UtcNow;
                await _backlogRepository.UpdateAsync(item);
            }

            return MapToApprovalDto(approval);
        }

        public async Task<BacklogApprovalRequestDto> RejectAsync(int id, int approvalRequestId, string reason, int reviewedByUserId)
        {
            var approval = await _approvalRepository.GetByIdAsync(approvalRequestId);
            if (approval == null || approval.BacklogItemId != id) throw new KeyNotFoundException("Approval request not found");
            if (approval.Status != ApprovalRequestStatus.Pending) throw new InvalidOperationException("Only pending approval requests can be rejected");

            approval.Status = ApprovalRequestStatus.Rejected;
            approval.ReviewNote = reason;
            approval.ReviewedByUserId = reviewedByUserId;
            approval.ReviewedAt = DateTime.UtcNow;
            await _approvalRepository.UpdateAsync(approval);

            var item = await _backlogRepository.GetByIdAsync(id);
            if (item != null)
            {
                item.Status = BacklogItemStatus.Draft;
                item.RejectionReason = reason;
                await _backlogRepository.UpdateAsync(item);
            }

            return MapToApprovalDto(approval);
        }

        public async Task<BacklogApprovalRequestDto> RequestChangesAsync(int id, int approvalRequestId, string note, int reviewedByUserId)
        {
            var approval = await _approvalRepository.GetByIdAsync(approvalRequestId);
            if (approval == null || approval.BacklogItemId != id) throw new KeyNotFoundException("Approval request not found");

            approval.Status = ApprovalRequestStatus.ChangesRequested;
            approval.ReviewNote = note;
            approval.ReviewedByUserId = reviewedByUserId;
            approval.ReviewedAt = DateTime.UtcNow;
            await _approvalRepository.UpdateAsync(approval);

            return MapToApprovalDto(approval);
        }

        private IQueryable<BacklogItem> ApplyFilter(IQueryable<BacklogItem> query, BacklogFilterRequest filter)
        {
            if (filter.Type.HasValue) query = query.Where(i => i.Type == filter.Type.Value);
            if (filter.Status.HasValue) query = query.Where(i => i.Status == filter.Status.Value);
            if (filter.Priority.HasValue) query = query.Where(i => i.Priority == filter.Priority.Value);
            if (filter.OwnerId.HasValue) query = query.Where(i => i.OwnerId == filter.OwnerId.Value);
            if (!string.IsNullOrEmpty(filter.Search)) query = query.Where(i => i.Title.Contains(filter.Search) || i.Description.Contains(filter.Search));
            return query;
        }

        private BacklogItemDto MapToDto(BacklogItem item)
        {
            return new BacklogItemDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Type = item.Type.ToString(),
                Status = item.Status.ToString(),
                Priority = item.Priority.ToString(),
                ProjectId = item.ProjectId,
                ProductId = item.ProductId,
                Order = item.Order,
                AcceptanceCriteria = item.AcceptanceCriteria,
                Owner = MapToUserSummary(item.Owner),
                ApprovedAt = item.ApprovedAt,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                VersionCount = item.Versions.Count,
                AttachmentCount = item.Attachments.Count,
                LinkedTicketCount = item.TicketLinks.Count,
                Attachments = item.Attachments.Select(MapToAttachmentDto).ToList(),
                TicketLinks = item.TicketLinks.Select(tl => new BacklogItemTicketLinkDto
                {
                    TicketId = tl.TicketId,
                    TicketNumber = tl.Ticket.TicketNumber,
                    TicketTitle = tl.Ticket.Title,
                    TicketStatus = tl.Ticket.Status?.Name ?? "Unknown"
                }).ToList()
            };
        }

        private UserSummaryDto MapToUserSummary(User user)
        {
            if (user == null) return new UserSummaryDto();
            return new UserSummaryDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email!,
                AvatarUrl = user.AvatarUrl
            };
        }

        private BacklogAttachmentDto MapToAttachmentDto(BacklogAttachment a)
        {
            return new BacklogAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSizeBytes = a.FileSizeBytes,
                DownloadUrl = _fileStorage.GetRelativeDownloadUrl(a.FilePath),
                UploadedAt = a.UploadedAt,
                UploadedBy = MapToUserSummary(a.UploadedBy)
            };
        }

        private BacklogApprovalRequestDto MapToApprovalDto(BacklogApprovalRequest a)
        {
            return new BacklogApprovalRequestDto
            {
                Id = a.Id,
                BacklogItemId = a.BacklogItemId,
                Status = a.Status.ToString(),
                ReviewNote = a.ReviewNote,
                RequestedAt = a.RequestedAt,
                ReviewedAt = a.ReviewedAt
            };
        }
    }
}

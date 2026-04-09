using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.Services
{
    public class TicketExtraService : ITicketExtraService
    {
        private readonly IRepository<TicketComment> _commentRepository;
        private readonly IRepository<CommentMention> _mentionRepository;
        private readonly IRepository<CommentReaction> _reactionRepository;
        private readonly IRepository<TicketWatcher> _watcherRepository;
        private readonly IRepository<TicketAttachment> _attachmentRepository;
        private readonly IRepository<DailyUpdate> _dailyUpdateRepository;
        private readonly IRepository<DailyUpdateTicketLink> _dailyUpdateTicketLinkRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IFileStorageService _fileStorage;

        public TicketExtraService(
            IRepository<TicketComment> commentRepository,
            IRepository<CommentMention> mentionRepository,
            IRepository<CommentReaction> reactionRepository,
            IRepository<TicketWatcher> watcherRepository,
            IRepository<TicketAttachment> attachmentRepository,
            IRepository<DailyUpdate> dailyUpdateRepository,
            IRepository<DailyUpdateTicketLink> dailyUpdateTicketLinkRepository,
            IRepository<User> userRepository,
            IFileStorageService fileStorage)
        {
            _commentRepository = commentRepository;
            _mentionRepository = mentionRepository;
            _reactionRepository = reactionRepository;
            _watcherRepository = watcherRepository;
            _attachmentRepository = attachmentRepository;
            _dailyUpdateRepository = dailyUpdateRepository;
            _dailyUpdateTicketLinkRepository = dailyUpdateTicketLinkRepository;
            _userRepository = userRepository;
            _fileStorage = fileStorage;
        }

        public async Task<IEnumerable<CommentDto>> GetTicketCommentsAsync(int ticketId, bool includeInternal)
        {
            var comments = await _commentRepository.Query()
                .Include(c => c.Author)
                .Include(c => c.Reactions)
                .Include(c => c.Replies).ThenInclude(r => r.Author)
                .Where(c => c.TicketId == ticketId && c.ParentCommentId == null)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            if (!includeInternal)
                comments = comments.Where(c => !c.IsInternalNote).ToList();

            return comments.Select(MapCommentDto);
        }

        public async Task<CommentDto> PostCommentAsync(int ticketId, CreateCommentRequest request, int userId)
        {
            var comment = new TicketComment
            {
                TicketId = ticketId,
                ParentCommentId = request.ParentCommentId,
                Body = request.Body,
                AuthorId = userId,
                IsInternalNote = request.IsInternalNote
            };

            await _commentRepository.AddAsync(comment);

            // Parse @mentions
            var mentions = ParseMentions(request.Body);
            foreach (var mentionId in mentions)
            {
                await _mentionRepository.AddAsync(new CommentMention { CommentId = comment.Id, UserId = mentionId });
            }

            return (await GetCommentDtoAsync(comment.Id))!;
        }

        public async Task UpdateCommentAsync(int commentId, UpdateCommentRequest request, int userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.AuthorId != userId) return;

            comment.Body = request.Body;
            await _commentRepository.UpdateAsync(comment);
        }

        public async Task DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment != null) await _commentRepository.DeleteAsync(commentId);
        }

        public async Task AddReactionAsync(int commentId, string emoji, int userId)
        {
            var exists = await _reactionRepository.Query().AnyAsync(r => r.CommentId == commentId && r.UserId == userId && r.Emoji == emoji);
            if (!exists)
            {
                await _reactionRepository.AddAsync(new CommentReaction { CommentId = commentId, UserId = userId, Emoji = emoji });
            }
        }

        public async Task RemoveReactionAsync(int commentId, string emoji, int userId)
        {
            var reaction = await _reactionRepository.Query().FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId && r.Emoji == emoji);
            if (reaction != null) await _reactionRepository.DeleteAsync(reaction.Id);
        }

        public async Task WatchTicketAsync(int ticketId, int userId)
        {
            var exists = await _watcherRepository.Query().AnyAsync(w => w.TicketId == ticketId && w.UserId == userId);
            if (!exists)
            {
                await _watcherRepository.AddAsync(new TicketWatcher { TicketId = ticketId, UserId = userId });
            }
        }

        public async Task UnwatchTicketAsync(int ticketId, int userId)
        {
            var watcher = await _watcherRepository.Query().FirstOrDefaultAsync(w => w.TicketId == ticketId && w.UserId == userId);
            if (watcher != null)
            {
                await _watcherRepository.DeleteAsync(watcher.Id);
            }
        }

        public async Task<IEnumerable<int>> GetWatchersAsync(int ticketId)
        {
            return await _watcherRepository.Query().Where(w => w.TicketId == ticketId).Select(w => w.UserId).ToListAsync();
        }

        public async Task<TicketAttachmentDto> UploadAttachmentAsync(int ticketId, IFormFile file, int userId)
        {
            // Validation
            var isImage = file.ContentType.StartsWith("image/");
            var maxSize = isImage ? 10 * 1024 * 1024 : 25 * 1024 * 1024;
            if (file.Length > maxSize) throw new InvalidOperationException("File size exceeds allowed limit");

            var subPath = Path.Combine("tickets", ticketId.ToString());
            var relativePath = await _fileStorage.SaveFileAsync(file, subPath);

            var existing = await _attachmentRepository.Query()
                .Where(a => a.TicketId == ticketId && a.FileName == file.FileName)
                .OrderByDescending(a => a.Version)
                .FirstOrDefaultAsync();

            var attachment = new TicketAttachment
            {
                TicketId = ticketId,
                FileName = file.FileName,
                FilePath = relativePath,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedByUserId = userId,
                Version = (existing?.Version ?? 0) + 1
            };

            await _attachmentRepository.AddAsync(attachment);
            return MapToAttachmentDto(attachment);
        }

        public async Task<TicketAttachmentDto> LinkExternalAttachmentAsync(int ticketId, LinkExternalAttachmentRequest request, int userId)
        {
            var attachment = new TicketAttachment
            {
                TicketId = ticketId,
                FileName = request.ExternalLabel,
                ExternalUrl = request.ExternalUrl,
                ExternalLabel = request.ExternalLabel,
                UploadedByUserId = userId,
                Version = 1
            };
            await _attachmentRepository.AddAsync(attachment);
            return MapToAttachmentDto(attachment);
        }

        public async Task<IEnumerable<TicketAttachmentDto>> GetTicketAttachmentsAsync(int ticketId)
        {
            var attachments = await _attachmentRepository.Query()
                .Include(a => a.UploadedByUser)
                .Where(a => a.TicketId == ticketId)
                .ToListAsync();
            return attachments.Select(MapToAttachmentDto);
        }

        public async Task<TicketAttachmentDto?> GetAttachmentByIdAsync(int attachmentId)
        {
            var a = await _attachmentRepository.Query().Include(a => a.UploadedByUser).FirstOrDefaultAsync(a => a.Id == attachmentId);
            return a != null ? MapToAttachmentDto(a) : null;
        }

        public async Task DeleteAttachmentAsync(int ticketId, int attachmentId, int userId)
        {
            var a = await _attachmentRepository.GetByIdAsync(attachmentId);
            if (a != null && a.TicketId == ticketId)
            {
                if (a.FilePath != null) await _fileStorage.DeleteFileAsync(a.FilePath);
                await _attachmentRepository.DeleteAsync(attachmentId);
            }
        }

        public async Task<DailyUpdateDto> PostDailyUpdateAsync(CreateDailyUpdateRequest request, int userId)
        {
            var today = DateTime.UtcNow.Date;
            var update = await _dailyUpdateRepository.Query()
                .FirstOrDefaultAsync(u => u.UserId == userId && u.SubmittedAt >= today);

            if (update == null)
            {
                update = new DailyUpdate
                {
                    UserId = userId,
                    ProjectId = request.ProjectId,
                    WorkedOn = request.WorkedOn,
                    PlannedNext = request.PlannedNext,
                    Blockers = request.Blockers,
                    SubmittedAt = DateTime.UtcNow
                };
                await _dailyUpdateRepository.AddAsync(update);
            }
            else
            {
                update.WorkedOn = request.WorkedOn;
                update.PlannedNext = request.PlannedNext;
                update.Blockers = request.Blockers;
                await _dailyUpdateRepository.UpdateAsync(update);
            }

            return MapDailyUpdateDto(update);
        }

        public async Task UpdateDailyUpdateAsync(int id, CreateDailyUpdateRequest request, int userId)
        {
            var update = await _dailyUpdateRepository.GetByIdAsync(id);
            if (update == null || update.UserId != userId) return;
            if (update.SubmittedAt.Date != DateTime.UtcNow.Date) throw new InvalidOperationException("Can only edit updates from today");

            update.WorkedOn = request.WorkedOn;
            update.PlannedNext = request.PlannedNext;
            update.Blockers = request.Blockers;
            await _dailyUpdateRepository.UpdateAsync(update);
        }

        public async Task<IEnumerable<DailyUpdateDto>> GetUserDailyUpdatesAsync(int userId)
        {
            var updates = await _dailyUpdateRepository.Query()
                .Include(u => u.TicketLinks)
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.SubmittedAt)
                .ToListAsync();
            return updates.Select(MapDailyUpdateDto);
        }

        public async Task<IEnumerable<DailyUpdateDto>> GetProjectDailyUpdatesAsync(int projectId, int? userId, DateTime? date)
        {
            var query = _dailyUpdateRepository.Query()
                .Include(u => u.User)
                .Include(u => u.TicketLinks)
                .Where(u => u.ProjectId == projectId);

            if (userId.HasValue) query = query.Where(u => u.UserId == userId.Value);
            if (date.HasValue) query = query.Where(u => u.SubmittedAt >= date.Value.Date && u.SubmittedAt < date.Value.Date.AddDays(1));

            var updates = await query.OrderByDescending(u => u.SubmittedAt).ToListAsync();
            return updates.Select(MapDailyUpdateDto);
        }

        public async Task LinkUpdateToTicketAsync(int dailyUpdateId, int ticketId, int userId)
        {
            var update = await _dailyUpdateRepository.GetByIdAsync(dailyUpdateId);
            if (update == null || update.UserId != userId) return;

            var exists = await _dailyUpdateTicketLinkRepository.Query().AnyAsync(l => l.DailyUpdateId == dailyUpdateId && l.TicketId == ticketId);
            if (!exists)
            {
                await _dailyUpdateTicketLinkRepository.AddAsync(new DailyUpdateTicketLink { DailyUpdateId = dailyUpdateId, TicketId = ticketId });
            }
        }

        private async Task<CommentDto?> GetCommentDtoAsync(int commentId)
        {
            var c = await _commentRepository.Query()
                .Include(c => c.Author)
                .Include(c => c.Reactions)
                .Include(c => c.Replies).ThenInclude(r => r.Author)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            return c != null ? MapCommentDto(c) : null;
        }

        private CommentDto MapCommentDto(TicketComment c)
        {
            return new CommentDto
            {
                Id = c.Id,
                ParentCommentId = c.ParentCommentId,
                Body = c.Body,
                AuthorId = c.AuthorId,
                AuthorDisplayName = c.Author?.DisplayName ?? "Unknown",
                IsInternalNote = c.IsInternalNote,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Reactions = c.Reactions.Select(r => new CommentReactionDto { Emoji = r.Emoji, UserId = r.UserId }).ToList(),
                Replies = c.Replies.Select(MapCommentDto).ToList()
            };
        }

        private TicketAttachmentDto MapToAttachmentDto(TicketAttachment a)
        {
            return new TicketAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileSizeBytes = a.FileSizeBytes,
                ContentType = a.ContentType,
                DownloadUrl = a.FilePath != null ? _fileStorage.GetRelativeDownloadUrl(a.FilePath) : null,
                ExternalUrl = a.ExternalUrl,
                ExternalLabel = a.ExternalLabel,
                UploadedByUserId = a.UploadedByUserId,
                UploadedByUserName = a.UploadedByUser?.DisplayName ?? "Unknown",
                UploadedAt = a.CreatedAt,
                Version = a.Version
            };
        }

        private DailyUpdateDto MapDailyUpdateDto(DailyUpdate u)
        {
            return new DailyUpdateDto
            {
                Id = u.Id,
                UserId = u.UserId,
                UserDisplayName = u.User?.DisplayName ?? "Unknown",
                WorkedOn = u.WorkedOn,
                PlannedNext = u.PlannedNext,
                Blockers = u.Blockers,
                SubmittedAt = u.SubmittedAt,
                LinkedTicketIds = u.TicketLinks.Select(l => l.TicketId).ToList()
            };
        }

        private List<int> ParseMentions(string body)
        {
            var userIds = new List<int>();
            // Simplistic regex to find @user_id. Real world would use usernames/display names.
            var matches = Regex.Matches(body, @"@(\d+)");
            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[1].Value, out var id)) userIds.Add(id);
            }
            return userIds.Distinct().ToList();
        }
    }
}

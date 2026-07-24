using LifeRPG.API.DTOs;
using LifeRPG.Core.Helpers;
using LifeRPG.Core.Models;
using LifeRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeRPG.API.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        // ======================================
        // GET TASKS
        // ======================================
        public async Task<List<TaskResponseDto>> GetTasksAsync(Guid userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    CategoryId = t.CategoryId,
                    Title = t.Title,
                    Description = t.Description,
                    XPValue = t.XPValue,
                    IsCompleted = t.IsCompleted,
                    IsRecurring = t.IsRecurring,
                    RecurrencePattern = t.RecurrencePattern,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    DueDate = t.DueDate,
                    Username = t.User != null ? t.User.Username : null,
                    CategoryName = t.Category != null ? t.Category.Name : null,
                    CategoryIcon = t.Category != null ? t.Category.Icon : null,
                    AwardedCharacterId = t.AwardedCharacterId
                })
                .ToListAsync();
        }

        // ======================================
        // CREATE TASK
        // ======================================
        public async Task<(TaskResponseDto? Result, string? ErrorMessage)> CreateTaskAsync(
            CreateTaskDto dto)
        {
            //  validation checks
            if (dto.UserId == Guid.Empty)
                return (null, "UserId is required.");

            if (dto.CategoryId == Guid.Empty)
                return (null, "CategoryId is required.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return (null, "Title is required.");

            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                return (null, "Invalid UserId.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return (null, "Invalid CategoryId.");

            var task = new RpgTask
            {
                UserId = dto.UserId,
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                Description = dto.Description,
                XPValue = dto.XPValue,
                RecurrencePattern = dto.RecurrencePattern,
                IsRecurring = !string.IsNullOrWhiteSpace(dto.RecurrencePattern),
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false,
                AwardedCharacterId = null
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // re-fetch with includes to return full DTO
            var createdTask = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Category)
                .Where(t => t.Id == task.Id)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    CategoryId = t.CategoryId,
                    Title = t.Title,
                    Description = t.Description,
                    XPValue = t.XPValue,
                    IsCompleted = t.IsCompleted,
                    IsRecurring = t.IsRecurring,
                    RecurrencePattern = t.RecurrencePattern,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    DueDate = t.DueDate,
                    Username = t.User != null ? t.User.Username : null,
                    CategoryName = t.Category != null ? t.Category.Name : null,
                    CategoryIcon = t.Category != null ? t.Category.Icon : null,
                    AwardedCharacterId = t.AwardedCharacterId
                })
                .FirstOrDefaultAsync();

            return (createdTask, null);
        }

        // ======================================
        // UPDATE TASK
        // ======================================
        public async Task<(bool Success, string? ErrorMessage)> UpdateTaskAsync(
            Guid id, CreateTaskDto dto)
        {
            // all validation checks
            if (id == Guid.Empty)
                return (false, "Task id is required.");

            if (dto.UserId == Guid.Empty)
                return (false, "UserId is required.");

            if (dto.CategoryId == Guid.Empty)
                return (false, "CategoryId is required.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return (false, "Title is required.");

            var task = await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return (false, "NotFound");

            if (task.User == null)
                return (false, "Task user not found.");

            if (task.UserId != dto.UserId)
                return (false, "You cannot change the owner of this task.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return (false, "Invalid CategoryId.");

            var oldXpValue = task.XPValue;
            var newXpValue = dto.XPValue;
            var xpDelta = newXpValue - oldXpValue;

            task.CategoryId = dto.CategoryId;
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.XPValue = newXpValue;
            task.RecurrencePattern = dto.RecurrencePattern;
            task.IsRecurring = !string.IsNullOrWhiteSpace(dto.RecurrencePattern);
            task.DueDate = dto.DueDate;

            // XP adjustment when editing a completed task
            if (task.IsCompleted && xpDelta != 0)
            {
                var characterId = task.AwardedCharacterId ?? task.User.ActiveCharacterId;

                if (characterId == null)
                    return (false, "No character assigned to this completed task.");

                if (task.AwardedCharacterId == null)
                    task.AwardedCharacterId = characterId.Value;

                var userChar = await GetOrCreateCharacterProgressAsync(
                    task.UserId, characterId.Value);

                if (xpDelta > 0)
                {
                    task.User.TotalLifePoints += xpDelta;

                    var lpResult = ProgressionHelper.AddPoints(
                        task.User.LifeLevel,
                        task.User.CurrentLifePoints,
                        xpDelta
                    );

                    task.User.LifeLevel = lpResult.newLevel;
                    task.User.CurrentLifePoints = lpResult.newCurrentPoints;

                    userChar.TotalXP += xpDelta;

                    var xpResult = ProgressionHelper.AddPoints(
                        userChar.Level,
                        userChar.CurrentXP,
                        xpDelta
                    );

                    userChar.Level = xpResult.newLevel;
                    userChar.CurrentXP = xpResult.newCurrentPoints;
                }
                else
                {
                    var reduction = Math.Abs(xpDelta);

                    task.User.TotalLifePoints -= reduction;
                    if (task.User.TotalLifePoints < 0)
                        task.User.TotalLifePoints = 0;

                    var lpResult = ProgressionHelper.RemovePoints(
                        task.User.LifeLevel,
                        task.User.CurrentLifePoints,
                        reduction
                    );

                    task.User.LifeLevel = lpResult.newLevel;
                    task.User.CurrentLifePoints = lpResult.newCurrentPoints;

                    userChar.TotalXP -= reduction;
                    if (userChar.TotalXP < 0)
                        userChar.TotalXP = 0;

                    var xpResult = ProgressionHelper.RemovePoints(
                        userChar.Level,
                        userChar.CurrentXP,
                        reduction
                    );

                    userChar.Level = xpResult.newLevel;
                    userChar.CurrentXP = xpResult.newCurrentPoints;
                }

                userChar.LastUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ======================================
        // DELETE TASK
        // ======================================
        public async Task<(bool Success, string? ErrorMessage)> DeleteTaskAsync(Guid id)
        {
            if (id == Guid.Empty)
                return (false, "Task id is required.");

            var task = await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return (false, "NotFound");

            if (task.User == null)
                return (false, "Task user not found.");

            // remove XP/LP when deleting a completed task
            if (task.IsCompleted)
            {
                var characterId = task.AwardedCharacterId ?? task.User.ActiveCharacterId;

                if (characterId == null)
                    return (false, "No character assigned to this completed task.");

                var userChar = await GetOrCreateCharacterProgressAsync(
                    task.UserId, characterId.Value);

                task.User.TotalLifePoints -= task.XPValue;
                if (task.User.TotalLifePoints < 0)
                    task.User.TotalLifePoints = 0;

                var lpResult = ProgressionHelper.RemovePoints(
                    task.User.LifeLevel,
                    task.User.CurrentLifePoints,
                    task.XPValue
                );

                task.User.LifeLevel = lpResult.newLevel;
                task.User.CurrentLifePoints = lpResult.newCurrentPoints;

                userChar.TotalXP -= task.XPValue;
                if (userChar.TotalXP < 0)
                    userChar.TotalXP = 0;

                var xpResult = ProgressionHelper.RemovePoints(
                    userChar.Level,
                    userChar.CurrentXP,
                    task.XPValue
                );

                userChar.Level = xpResult.newLevel;
                userChar.CurrentXP = xpResult.newCurrentPoints;
                userChar.LastUpdatedAt = DateTime.UtcNow;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ======================================
        // COMPLETE / UNCOMPLETE TASK
        // ======================================
        public async Task<(CompleteTaskResultDto? Result, string? ErrorMessage)> CompleteTaskAsync(
            Guid id)
        {
            var task = await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return (null, "NotFound");

            if (task.User == null)
                return (null, "Task user not found.");

            // recurring task reset logic
            if (task.IsRecurring &&
                task.IsCompleted &&
                task.CompletedAt.HasValue &&
                task.CompletedAt.Value.Date != DateTime.UtcNow.Date)
            {
                task.IsCompleted = false;
                task.CompletedAt = null;
                task.AwardedCharacterId = null;
            }

            if (!task.IsCompleted)
            {
                // ===== COMPLETING =====
                if (task.User.ActiveCharacterId == null)
                    return (null, "No active character selected.");

                var characterId = task.User.ActiveCharacterId.Value;
                var userChar = await GetOrCreateCharacterProgressAsync(task.UserId, characterId);

                task.IsCompleted = true;
                task.CompletedAt = DateTime.UtcNow;
                task.AwardedCharacterId = characterId;

                // bonus calculation
                var activeCharacterEntity = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == characterId);

                var taskCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == task.CategoryId);

                var bonusMultiplier = 1.0;
                if (activeCharacterEntity?.BonusCategoryName != null &&
                    taskCategory?.Name == activeCharacterEntity.BonusCategoryName)
                {
                    bonusMultiplier = 1.2;
                }

                var actualXp = (int)Math.Round(task.XPValue * bonusMultiplier);
                var actualLp = (int)Math.Round(task.XPValue * bonusMultiplier / 2.0);

                // LP progression
                task.User.TotalLifePoints += actualLp;

                var lpResult = ProgressionHelper.AddPoints(
                    task.User.LifeLevel,
                    task.User.CurrentLifePoints,
                    actualLp
                );

                task.User.LifeLevel = lpResult.newLevel;
                task.User.CurrentLifePoints = lpResult.newCurrentPoints;

                // XP progression
                userChar.TotalXP += actualXp;

                var xpResult = ProgressionHelper.AddPoints(
                    userChar.Level,
                    userChar.CurrentXP,
                    actualXp
                );

                userChar.Level = xpResult.newLevel;
                userChar.CurrentXP = xpResult.newCurrentPoints;
                userChar.LastUpdatedAt = DateTime.UtcNow;

                // streak logic
                var today = DateTime.UtcNow.Date;
                var lastDate = task.User.LastTaskDate?.Date;

                if (lastDate == null || lastDate < today.AddDays(-1))
                    task.User.StreakCount = 1;
                else if (lastDate == today.AddDays(-1))
                    task.User.StreakCount += 1;

                task.User.LastTaskDate = DateTime.UtcNow;

                // achievement check with +1 correction
                var completingResult = await CheckAndUpdateAchievementsAsync(
                    task.UserId,
                    task.User.StreakCount,
                    task.User.LifeLevel,
                    countOffset: +1
                );

                await _context.SaveChangesAsync();

                return (new CompleteTaskResultDto
                {
                    ForcedCharacterSwitch = false,
                    ActualXpAwarded = actualXp,
                    ActualLpAwarded = actualLp,
                    NewlyEarned = completingResult.NewlyEarned,
                    NewlyLost = completingResult.NewlyLost
                }, null);
            }
            else
            {
                // ===== UNCOMPLETING =====
                if (task.AwardedCharacterId == null)
                    return (null, "Task has no awarded character. Data inconsistency.");

                var userChar = await GetOrCreateCharacterProgressAsync(
                    task.UserId, task.AwardedCharacterId.Value);

                //  bonus recalculation on uncomplete
                var awardedCharacter = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == task.AwardedCharacterId.Value);

                var taskCategoryForUndo = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == task.CategoryId);

                var undoMultiplier = 1.0;
                if (awardedCharacter?.BonusCategoryName != null &&
                    taskCategoryForUndo?.Name == awardedCharacter.BonusCategoryName)
                {
                    undoMultiplier = 1.2;
                }

                var xpToRemove = (int)Math.Round(task.XPValue * undoMultiplier);
                var lpToRemove = (int)Math.Round(task.XPValue * undoMultiplier / 2.0);

                task.IsCompleted = false;
                task.CompletedAt = null;

                // Remove the Life Points gained from completing the task and update the user's LP level
                task.User.TotalLifePoints -= lpToRemove;
                if (task.User.TotalLifePoints < 0)
                    task.User.TotalLifePoints = 0;

                var lpResult = ProgressionHelper.RemovePoints(
                    task.User.LifeLevel,
                    task.User.CurrentLifePoints,
                    lpToRemove
                );

                task.User.LifeLevel = lpResult.newLevel;
                task.User.CurrentLifePoints = lpResult.newCurrentPoints;

                // Remove the Character XP gained from completing the task and update the character's XP level
                userChar.TotalXP -= xpToRemove;
                if (userChar.TotalXP < 0)
                    userChar.TotalXP = 0;

                var xpResult = ProgressionHelper.RemovePoints(
                    userChar.Level,
                    userChar.CurrentXP,
                    xpToRemove
                );

                userChar.Level = xpResult.newLevel;
                userChar.CurrentXP = xpResult.newCurrentPoints;
                userChar.LastUpdatedAt = DateTime.UtcNow;

                // forced character switch check
                var forcedSwitch = false;
                string? switchedToEmoji = null;
                string? switchedToName = null;

                if (task.User.ActiveCharacterId.HasValue)
                {
                    var activeChar = await _context.Characters
                        .FirstOrDefaultAsync(c => c.Id == task.User.ActiveCharacterId.Value);

                    if (activeChar != null && activeChar.UnlockLevel > task.User.LifeLevel)
                    {
                        var defaultChar = await _context.Characters
                            .Where(c => c.UnlockLevel == 0)
                            .OrderBy(c => c.Cid)
                            .FirstOrDefaultAsync();

                        if (defaultChar != null)
                        {
                            task.User.ActiveCharacterId = defaultChar.Id;
                            forcedSwitch = true;
                            switchedToEmoji = defaultChar.Emoji;
                            switchedToName = defaultChar.Name;
                        }
                    }
                }

                //  achievement check with -1 correction
                var uncomletingResult = await CheckAndUpdateAchievementsAsync(
                    task.UserId,
                    task.User.StreakCount,
                    task.User.LifeLevel,
                    countOffset: -1
                );

                await _context.SaveChangesAsync();

                return (new CompleteTaskResultDto
                {
                    ForcedCharacterSwitch = forcedSwitch,
                    SwitchedToEmoji = switchedToEmoji,
                    SwitchedToName = switchedToName,
                    ActualXpAwarded = -xpToRemove,
                    ActualLpAwarded = -lpToRemove,
                    NewlyEarned = uncomletingResult.NewlyEarned,
                    NewlyLost = uncomletingResult.NewlyLost
                }, null);
            }
        }

        // ======================================
        // PRIVATE HELPERS
        // ======================================
        private async Task<(List<AchievementDto> NewlyEarned, List<AchievementDto> NewlyLost)>
            CheckAndUpdateAchievementsAsync(
                Guid userId, int streakCount, int lpLevel, int countOffset)
        {
            var allAchievements = await _context.Achievements.ToListAsync();

            // count with offset to account for unsaved state
            var totalTasksCompleted = await _context.Tasks
                .CountAsync(t => t.UserId == userId && t.IsCompleted) + countOffset;

            if (totalTasksCompleted < 0) totalTasksCompleted = 0;

            var earned = AchievementHelper.GetEarned(
                allAchievements, totalTasksCompleted, streakCount, lpLevel);

            var currentAchievementIds = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToHashSetAsync();

            var earnedIds = earned.Select(a => a.Id).ToHashSet();

            var newlyEarned = earned
                .Where(a => !currentAchievementIds.Contains(a.Id))
                .ToList();

            var newlyLost = allAchievements
                .Where(a => currentAchievementIds.Contains(a.Id) && !earnedIds.Contains(a.Id))
                .ToList();

            if (newlyEarned.Count > 0)
            {
                var newRecords = newlyEarned.Select(a => new UserAchievement
                {
                    UserId = userId,
                    AchievementId = a.Id,
                    EarnedAt = DateTime.UtcNow
                }).ToList();

                await _context.UserAchievements.AddRangeAsync(newRecords);
            }

            if (newlyLost.Count > 0)
            {
                var lostIds = newlyLost.Select(a => a.Id).ToList();
                var toRemove = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId && lostIds.Contains(ua.AchievementId))
                    .ToListAsync();

                _context.UserAchievements.RemoveRange(toRemove);
            }

            return (
                newlyEarned.Select(a => new AchievementDto
                {
                    Name = a.Name, Emoji = a.Emoji, Description = a.Description
                }).ToList(),
                newlyLost.Select(a => new AchievementDto
                {
                    Name = a.Name, Emoji = a.Emoji, Description = a.Description
                }).ToList()
            );
        }


        private async Task<UserCharacterProgress> GetOrCreateCharacterProgressAsync(
            Guid userId, Guid characterId)
        {
            var progress = await _context.UserCharacterProgress
                .FirstOrDefaultAsync(cp =>
                    cp.UserId == userId && cp.CharacterId == characterId);

            if (progress == null)
            {
                progress = new UserCharacterProgress
                {
                    UserId = userId,
                    CharacterId = characterId,
                    TotalXP = 0,
                    CurrentXP = 0,
                    Level = 1,
                    IsUnlocked = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserCharacterProgress.Add(progress);
            }

            return progress;
        }
    }
}
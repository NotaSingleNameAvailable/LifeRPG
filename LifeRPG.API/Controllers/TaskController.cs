using LifeRPG.API.DTOs;
using LifeRPG.Core.Helpers;
using LifeRPG.Core.Models;
using LifeRPG.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Category)
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
                    AwardedCharacterId = t.AwardedCharacterId
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            if (dto.UserId == Guid.Empty)
                return BadRequest("UserId is required.");

            if (dto.CategoryId == Guid.Empty)
                return BadRequest("CategoryId is required.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title is required.");

            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                return BadRequest("Invalid UserId.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return BadRequest("Invalid CategoryId.");

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
                    AwardedCharacterId = t.AwardedCharacterId
                })
                .FirstOrDefaultAsync();

            return Ok(createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] CreateTaskDto dto)
        {
            if (id == Guid.Empty)
                return BadRequest("Task id is required.");

            if (dto.UserId == Guid.Empty)
                return BadRequest("UserId is required.");

            if (dto.CategoryId == Guid.Empty)
                return BadRequest("CategoryId is required.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title is required.");

            var task = await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            if (task.User == null)
                return BadRequest("Task user not found.");

            if (task.UserId != dto.UserId)
                return BadRequest("You cannot change the owner of this task.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return BadRequest("Invalid CategoryId.");

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

            if (task.IsCompleted && xpDelta != 0)
            {
                var characterId = task.AwardedCharacterId ?? task.User.ActiveCharacterId;

                if (characterId == null)
                    return BadRequest("No character assigned to this completed task.");

                if (task.AwardedCharacterId == null)
                    task.AwardedCharacterId = characterId.Value;

                var userChar = await GetOrCreateCharacterProgressAsync(task.UserId, characterId.Value);

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
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Task id is required.");

            var task = await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            if (task.User == null)
                return BadRequest("Task user not found.");

            if (task.IsCompleted)
            {
                var characterId = task.AwardedCharacterId ?? task.User.ActiveCharacterId;

                if (characterId == null)
                    return BadRequest("No character assigned to this completed task.");

                var userChar = await GetOrCreateCharacterProgressAsync(task.UserId, characterId.Value);

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

            return Ok();
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteTask(Guid id)
        {
            var task = await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            if (task.User == null)
                return BadRequest("Task user not found.");

            // If this task was already completed on a previous day and is recurring,
            // treat it as incomplete today so it can be completed again.
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
                // Completing now: bind XP to the active character at the moment of completion.
                if (task.User.ActiveCharacterId == null)
                    return BadRequest("No active character selected.");

                var characterId = task.User.ActiveCharacterId.Value;

                var userChar = await GetOrCreateCharacterProgressAsync(task.UserId, characterId);

                task.IsCompleted = true;
                task.CompletedAt = DateTime.UtcNow;
                task.AwardedCharacterId = characterId;

                // ===== USER LP =====
                task.User.TotalLifePoints += task.XPValue;

                var lpResult = ProgressionHelper.AddPoints(
                    task.User.LifeLevel,
                    task.User.CurrentLifePoints,
                    task.XPValue
                );

                task.User.LifeLevel = lpResult.newLevel;
                task.User.CurrentLifePoints = lpResult.newCurrentPoints;

                // ===== CHARACTER XP =====
                userChar.TotalXP += task.XPValue;

                var xpResult = ProgressionHelper.AddPoints(
                    userChar.Level,
                    userChar.CurrentXP,
                    task.XPValue
                );

                userChar.Level = xpResult.newLevel;
                userChar.CurrentXP = xpResult.newCurrentPoints;
                userChar.LastUpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Uncomplete: remove XP from the SAME character that originally earned it.
                if (task.AwardedCharacterId == null)
                    return BadRequest("Task has no awarded character. Data inconsistency.");

                var userChar = await GetOrCreateCharacterProgressAsync(
                    task.UserId,
                    task.AwardedCharacterId.Value
                );

                task.IsCompleted = false;
                task.CompletedAt = null;


                // ===== USER LP =====
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

                // ===== CHARACTER XP =====
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

            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<UserCharacterProgress> GetOrCreateCharacterProgressAsync(Guid userId, Guid characterId)
        {
            var progress = await _context.UserCharacterProgress
                .FirstOrDefaultAsync(cp => cp.UserId == userId && cp.CharacterId == characterId);

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
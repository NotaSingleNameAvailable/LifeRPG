using LifeRPG.API.DTOs;
using LifeRPG.Core.Helpers;
using LifeRPG.Core.Models;
using LifeRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeRPG.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        // Both AppDbContext and JwtService are injected
        // AppDbContext for DB access, JwtService for token generation
        public UserService(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<(UserResponseDto? Result, string? ErrorMessage)> RegisterAsync(
            RegisterRequestDto request)
        {
            // validation checks
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return (null, "Email already exists");

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return (null, "Username already exists");

            // user creation with BCrypt hashing
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                TotalLifePoints = 0,
                CurrentLifePoints = 0,
                LifeLevel = 1,
                StreakCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // auto-unlock magician on register
            var magician = await _context.Characters
                .FirstOrDefaultAsync(c => c.Cid == 1);

            if (magician != null)
            {
                user.ActiveCharacterId = magician.Id;

                var progress = new UserCharacterProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CharacterId = magician.Id,
                    TotalXP = 0,
                    CurrentXP = 0,
                    Level = 1,
                    IsUnlocked = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserCharacterProgress.Add(progress);
                await _context.SaveChangesAsync();
            }

            // token generation on register
            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Username);

            return (new UserResponseDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Token = token,
                TotalLifePoints = user.TotalLifePoints,
                CurrentLifePoints = user.CurrentLifePoints,
                LifeLevel = user.LifeLevel,
                StreakCount = user.StreakCount,
                CreatedAt = user.CreatedAt,
                ActiveCharacterId = user.ActiveCharacterId?.ToString(),
                CharacterXP = 0,
                CharacterLevel = 1
            }, null);
        }

        public async Task<(UserResponseDto? Result, string? ErrorMessage)> LoginAsync(
            LoginRequestDto request)
        {
            // include CharacterProgress for active character XP
            var user = await _context.Users
                .Include(u => u.CharacterProgress)
                    .ThenInclude(cp => cp.Character)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            // BCrypt verification
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return (null, "Invalid credentials");

            var charProgress = user.CharacterProgress
                .FirstOrDefault(cp => cp.CharacterId == user.ActiveCharacterId);

            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Username);

            return (new UserResponseDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Token = token,
                TotalLifePoints = user.TotalLifePoints,
                CurrentLifePoints = user.CurrentLifePoints,
                LifeLevel = user.LifeLevel,
                StreakCount = user.StreakCount,
                CreatedAt = user.CreatedAt,
                CharacterXP = charProgress?.CurrentXP ?? 0,
                CharacterLevel = charProgress?.Level ?? 1
            }, null);
        }

        public async Task<UserStatsDto?> GetStatsAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.CharacterProgress)
                    .ThenInclude(cp => cp.Character)
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null) return null;

            var charProgress = user.CharacterProgress
                .FirstOrDefault(cp => cp.CharacterId == user.ActiveCharacterId);

            return new UserStatsDto
            {
                CharacterXP = charProgress?.CurrentXP ?? 0,
                CharacterLevel = charProgress?.Level ?? 1,
                UserLP = user.CurrentLifePoints,
                LpLevel = user.LifeLevel
            };
        }

        public async Task<bool> UpdateStatsAsync(string userId, UserStatsDto stats)
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null) return false;

            // update LP fields
            user.CurrentLifePoints = stats.UserLP;
            user.LifeLevel = stats.LpLevel;

            var charProgress = await _context.UserCharacterProgress
                .FirstOrDefaultAsync(cp =>
                    cp.UserId == user.Id &&
                    cp.CharacterId == user.ActiveCharacterId);

            // update character XP fields if progress exists
            if (charProgress != null)
            {
                charProgress.CurrentXP = stats.CharacterXP;
                charProgress.Level = stats.CharacterLevel;
                charProgress.LastUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CharacterProgressDto>?> GetUserCharactersAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.CharacterProgress)
                    .ThenInclude(cp => cp.Character)
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null) return null;

            return user.CharacterProgress.Select(cp => new CharacterProgressDto
            {
                Id = cp.Id.ToString(),
                CharacterId = cp.CharacterId.ToString(),
                Cid = cp.Character.Cid,
                CharacterName = cp.Character.Name,
                CharacterEmoji = cp.Character.Emoji,
                TotalXP = cp.TotalXP,
                CurrentXP = cp.CurrentXP,
                Level = cp.Level,
                IsUnlocked = cp.IsUnlocked
            }).ToList();
        }

        public async Task<UserMeDto?> GetUserStateAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.CharacterProgress)
                    .ThenInclude(cp => cp.Character)
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null) return null;

            var active = user.CharacterProgress
                .FirstOrDefault(cp => cp.CharacterId == user.ActiveCharacterId);

            //  load ALL characters, not just ones with progress
            var allCharacters = await _context.Characters
                .AsNoTracking()
                .OrderBy(c => c.Cid)
                .ToListAsync();

            var characters = allCharacters.Select(c =>
            {
                var cp = user.CharacterProgress
                    .FirstOrDefault(p => p.CharacterId == c.Id);

                return new CharacterProgressDto
                {
                    Id = c.Id.ToString(),
                    CharacterId = c.Id.ToString(),
                    Cid = c.Cid,
                    CharacterName = c.Name,
                    CharacterEmoji = c.Emoji,
                    UnlockLevel = c.UnlockLevel,
                    IsUnlocked = user.LifeLevel >= c.UnlockLevel,
                    IsActive = user.ActiveCharacterId == c.Id,
                    TotalXP = cp?.TotalXP ?? 0,
                    CurrentXP = cp?.CurrentXP ?? 0,
                    Level = cp?.Level ?? 1,
                    BonusCategoryName = c.BonusCategoryName
                };
            }).ToList();

            return new UserMeDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                TotalLifePoints = user.TotalLifePoints,
                CurrentLifePoints = user.CurrentLifePoints,
                LifeLevel = user.LifeLevel,
                ActiveCharacterId = user.ActiveCharacterId?.ToString(),
                CharacterXP = active?.CurrentXP ?? 0,
                CharacterLevel = active?.Level ?? 1,
                Characters = characters
            };
        }

        public async Task<(object? Result, string? ErrorMessage)> ApplyTaskDeltaAsync(
            string userId, ApplyTaskDeltaDto dto)
        {
            if (dto.CharacterId == Guid.Empty)
                return (null, "CharacterId is required.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
                return (null, "User not found.");

            var characterProgress = await _context.UserCharacterProgress
                .FirstOrDefaultAsync(cp =>
                    cp.UserId == user.Id &&
                    cp.CharacterId == dto.CharacterId);

            //  create progress row if first time using this character
            if (characterProgress == null)
            {
                characterProgress = new UserCharacterProgress
                {
                    UserId = user.Id,
                    CharacterId = dto.CharacterId,
                    TotalXP = 0,
                    CurrentXP = 0,
                    Level = 1,
                    IsUnlocked = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserCharacterProgress.Add(characterProgress);
            }

            // bonus calculation for today's tasks category matching
            if (!string.IsNullOrEmpty(dto.CategoryName))
            {
                var activeChar = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == dto.CharacterId);

                if (activeChar?.BonusCategoryName == dto.CategoryName)
                {
                    // applies to both positive and negative deltas
                    dto.XpDelta = (int)Math.Round(dto.XpDelta * 1.2);
                    dto.LpDelta = (int)Math.Round(dto.LpDelta * 1.2);
                }
            }

            // XP delta application with progression helper
            if (dto.XpDelta > 0)
            {
                characterProgress.TotalXP += dto.XpDelta;

                var xpResult = ProgressionHelper.AddPoints(
                    characterProgress.Level,
                    characterProgress.CurrentXP,
                    dto.XpDelta
                );

                characterProgress.Level = xpResult.newLevel;
                characterProgress.CurrentXP = xpResult.newCurrentPoints;
            }
            else if (dto.XpDelta < 0)
            {
                var reduction = Math.Abs(dto.XpDelta);

                characterProgress.TotalXP -= reduction;
                if (characterProgress.TotalXP < 0)
                    characterProgress.TotalXP = 0;

                var xpResult = ProgressionHelper.RemovePoints(
                    characterProgress.Level,
                    characterProgress.CurrentXP,
                    reduction
                );

                characterProgress.Level = xpResult.newLevel;
                characterProgress.CurrentXP = xpResult.newCurrentPoints;
            }

            characterProgress.LastUpdatedAt = DateTime.UtcNow;

            //  LP delta application with progression helper
            if (dto.LpDelta > 0)
            {
                user.TotalLifePoints += dto.LpDelta;

                var lpResult = ProgressionHelper.AddPoints(
                    user.LifeLevel,
                    user.CurrentLifePoints,
                    dto.LpDelta
                );

                user.LifeLevel = lpResult.newLevel;
                user.CurrentLifePoints = lpResult.newCurrentPoints;
            }
            else if (dto.LpDelta < 0)
            {
                var reduction = Math.Abs(dto.LpDelta);

                user.TotalLifePoints -= reduction;
                if (user.TotalLifePoints < 0)
                    user.TotalLifePoints = 0;

                var lpResult = ProgressionHelper.RemovePoints(
                    user.LifeLevel,
                    user.CurrentLifePoints,
                    reduction
                );

                user.LifeLevel = lpResult.newLevel;
                user.CurrentLifePoints = lpResult.newCurrentPoints;
            }

            await _context.SaveChangesAsync();

            //  return actual awarded values for frontend popup
            return (new
            {
                user.CurrentLifePoints,
                user.LifeLevel,
                characterProgress.CurrentXP,
                characterProgress.Level,
                ActualXpAwarded = dto.XpDelta,
                ActualLpAwarded = dto.LpDelta
            }, null);
        }
    }
}
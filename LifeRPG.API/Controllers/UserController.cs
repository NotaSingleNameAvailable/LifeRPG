using LifeRPG.API.DTOs;
using LifeRPG.Core.Models;
using LifeRPG.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LifeRPG.Core.Helpers;
using LifeRPG.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace LifeRPG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public UserController(AppDbContext context, JwtService  jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        // Validate
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email already exists");

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username already exists");

        // Create user
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

  
        // Auto-unlock magician character (Cid = 1) AND set as active
        var magician = await _context.Characters.FirstOrDefaultAsync(c => c.Cid == 1);

        if (magician != null)
        {
            // 1. Set active character on user
            user.ActiveCharacterId = magician.Id;

            // 2. Create character progress
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

            // 3. Save BOTH user + progress together
            await _context.SaveChangesAsync();
        }

        // Generate token on register so user is immediately logged in
        var token = _jwtService.GenerateToken(user.Id.ToString(), user.Username);

        return Ok(new UserResponseDto
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
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.CharacterProgress)
                .ThenInclude(cp => cp.Character)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

         // BCrypt.Verify compares plain password against stored hash
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        return Unauthorized("Invalid credentials");

        var charProgress = user.CharacterProgress
            .FirstOrDefault(cp => cp.CharacterId == user.ActiveCharacterId);

        var token = _jwtService.GenerateToken(user.Id.ToString(), user.Username);

        return Ok(new UserResponseDto
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
        });
    }

    [HttpGet("{userId}/stats")]
    [Authorize] 
    public async Task<ActionResult<UserStatsDto>> GetUserStats(string userId)
    {
        var user = await _context.Users
            .Include(u => u.CharacterProgress)
                .ThenInclude(cp => cp.Character)
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

        if (user == null)
            return NotFound();

        var charProgress = user.CharacterProgress
            .FirstOrDefault(cp => cp.CharacterId == user.ActiveCharacterId);

        return Ok(new UserStatsDto
        {
            CharacterXP = charProgress?.CurrentXP ?? 0,
            CharacterLevel = charProgress?.Level ?? 1,
            UserLP = user.CurrentLifePoints,
            LpLevel = user.LifeLevel
        });
    }

    [HttpPut("{userId}/stats")]
    [Authorize] 
    public async Task<IActionResult> UpdateUserStats(string userId, [FromBody] UserStatsDto stats)
    {
        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        if (user == null)
            return NotFound();

        user.CurrentLifePoints = stats.UserLP;
        user.LifeLevel = stats.LpLevel;

        var charProgress = await _context.UserCharacterProgress
        .FirstOrDefaultAsync(cp =>
            cp.UserId == user.Id &&
            cp.CharacterId == user.ActiveCharacterId);

        if (charProgress != null)
        {
            charProgress.CurrentXP = stats.CharacterXP;
            charProgress.Level = stats.CharacterLevel;
            charProgress.LastUpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("{userId}/characters")]
    [Authorize] 
    public async Task<ActionResult<IEnumerable<CharacterProgressDto>>> GetUserCharacters(string userId)
    {
        var user = await _context.Users
            .Include(u => u.CharacterProgress)
                .ThenInclude(cp => cp.Character)
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

        if (user == null)
            return NotFound();

        var result = user.CharacterProgress.Select(cp => new CharacterProgressDto
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

        return Ok(result);
    }

[HttpGet("{userId}/me")]
[Authorize] 
public async Task<ActionResult<UserMeDto>> GetUserState(string userId)
{
    var user = await _context.Users
        .Include(u => u.CharacterProgress)
            .ThenInclude(cp => cp.Character)
        .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

    if (user == null)
        return NotFound();

    var active = user.CharacterProgress
        .FirstOrDefault(cp => cp.CharacterId == user.ActiveCharacterId);

    // Load all characters from the Characters table
    var allCharacters = await _context.Characters
        .AsNoTracking()
        .OrderBy(c => c.Cid)
        .ToListAsync();

    var characters = allCharacters.Select(c =>
    {
        // Find progress row if it exists (may be null for locked/untouched characters)
        var cp = user.CharacterProgress.FirstOrDefault(p => p.CharacterId == c.Id);

        return new CharacterProgressDto
        {
            Id = c.Id.ToString(),
            CharacterId = c.Id.ToString(),
            Cid = c.Cid,
            CharacterName = c.Name,
            CharacterEmoji = c.Emoji,
            UnlockLevel = c.UnlockLevel,    
            IsUnlocked = user.LifeLevel >= c.UnlockLevel,
            TotalXP = cp?.TotalXP ?? 0,
            CurrentXP = cp?.CurrentXP ?? 0,
            Level = cp?.Level ?? 1,
            BonusCategoryName = c.BonusCategoryName
        };
    }).ToList();

    return Ok(new UserMeDto
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
    });
}

[HttpPost("{userId}/apply-task-delta")]
[Authorize] 
public async Task<IActionResult> ApplyTaskDelta(string userId, [FromBody] ApplyTaskDeltaDto dto)
{
    if (dto.CharacterId == Guid.Empty)
        return BadRequest("CharacterId is required.");

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

    if (user == null)
        return NotFound("User not found.");

    var characterProgress = await _context.UserCharacterProgress
        .FirstOrDefaultAsync(cp =>
            cp.UserId == user.Id &&
            cp.CharacterId == dto.CharacterId);

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


        // ===== BONUS CALCULATION FOR TODAY'S TASKS =====
        if (!string.IsNullOrEmpty(dto.CategoryName))
        {
            var activeChar = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == dto.CharacterId);

            if (activeChar?.BonusCategoryName == dto.CategoryName)
            {
                // Apply bonus to both positive (complete) and negative (uncomplete) deltas
                dto.XpDelta = (int)Math.Round(dto.XpDelta * 1.2);
                dto.LpDelta = (int)Math.Round(dto.LpDelta * 1.2);
            }
        }
        // ===== BONUS CALCULATION END =====


    // Apply character XP delta
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

    // Apply user LP delta
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
    return Ok(new
    {
        user.CurrentLifePoints,
        user.LifeLevel,
        characterProgress.CurrentXP,
        characterProgress.Level,
        ActualXpAwarded = dto.XpDelta,
        ActualLpAwarded = dto.LpDelta
    });
}
}
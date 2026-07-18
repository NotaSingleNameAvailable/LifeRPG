using LifeRPG.Core.Models;

namespace LifeRPG.Infrastructure.Data
{
    public static class DataSeeder
    {
    public static void SeedCategories(AppDbContext context)
    {
        var existingNames = context.Categories.Select(c => c.Name).ToHashSet();

        var toAdd = new List<Category>();

        if (!existingNames.Contains("Fitness"))
            toAdd.Add(new Category { Name = "Fitness", Description = "Physical health tasks", Icon = "💪", ColorCode = "#FF6B6B" });
        if (!existingNames.Contains("Finance"))
            toAdd.Add(new Category { Name = "Finance", Description = "Money and budgeting", Icon = "💰", ColorCode = "#FFD93D" });
        if (!existingNames.Contains("Learning"))
            toAdd.Add(new Category { Name = "Learning", Description = "Skills and study", Icon = "📘", ColorCode = "#6BCB77" });
        if (!existingNames.Contains("Mindfulness"))
            toAdd.Add(new Category { Name = "Mindfulness", Description = "Mental health and meditation", Icon = "🧘", ColorCode = "#A78BFA" });
        if (!existingNames.Contains("Nutrition"))
            toAdd.Add(new Category { Name = "Nutrition", Description = "Diet and healthy eating", Icon = "🥗", ColorCode = "#34D399" });
        if (!existingNames.Contains("Social"))
            toAdd.Add(new Category { Name = "Social", Description = "Relationships and communication", Icon = "🤝", ColorCode = "#60A5FA" });
        if (!existingNames.Contains("Creativity"))
            toAdd.Add(new Category { Name = "Creativity", Description = "Art, music, writing", Icon = "🎨", ColorCode = "#F472B6" });
        if (!existingNames.Contains("Adventure"))
            toAdd.Add(new Category { Name = "Adventure", Description = "Exploration and new experiences", Icon = "🗺️", ColorCode = "#FB923C" });
        if (!existingNames.Contains("Discipline"))
            toAdd.Add(new Category { Name = "Discipline", Description = "Habits and self-control", Icon = "⚡", ColorCode = "#94A3B8" });

        if (toAdd.Count > 0)
        {
            context.Categories.AddRange(toAdd);
            context.SaveChanges();
        }
    }
    public static void SeedCharacters(AppDbContext context)
    {
        if (!context.Characters.Any())
        {
            context.Characters.AddRange(
                new Character
                {
                    Cid = 1,
                    Name = "Magician",
                    Emoji = "🧙",
                    Description = "Master of knowledge and arcane wisdom.",
                    UnlockLevel = 0,
                    BonusCategoryName = "Learning"
                },
                new Character
                {
                    Cid = 2,
                    Name = "Dragon",
                    Emoji = "🐉",
                    Description = "Ancient beast of overwhelming power.",
                    UnlockLevel = 1,
                    BonusCategoryName = "Adventure"
                },
                new Character
                {
                    Cid = 3,
                    Name = "Knight",
                    Emoji = "⚔️",
                    Description = "Disciplined warrior of honor.",
                    UnlockLevel = 2,
                    BonusCategoryName = "Fitness"
                },
                new Character
                {
                    Cid = 4,
                    Name = "Phoenix",
                    Emoji = "🔥",
                    Description = "Always rises again after failure.",
                    UnlockLevel = 5,
                    BonusCategoryName = "Mindfulness"
                },
                new Character
                {
                    Cid = 5,
                    Name = "Monk",
                    Emoji = "🧘",
                    Description = "Seeker of balance and inner peace.",
                    UnlockLevel = 6,
                    BonusCategoryName = "Mindfulness"
                },
                new Character
                {
                    Cid = 6,
                    Name = "Merchant",
                    Emoji = "💰",
                    Description = "Builder of wealth and financial mastery.",
                    UnlockLevel = 9,
                    BonusCategoryName = "Finance"
                },
                new Character
                {
                    Cid = 7,
                    Name = "Viking",
                    Emoji = "🪓",
                    Description = "Relentless conqueror of challenges.",
                    UnlockLevel = 12,
                    BonusCategoryName = "Discipline"
                },
                new Character
                {
                    Cid = 8,
                    Name = "Robot",
                    Emoji = "🤖",
                    Description = "Cold efficiency and optimized productivity.",
                    UnlockLevel = 15,
                    BonusCategoryName = "Learning"
                },
                new Character
                {
                    Cid = 9,
                    Name = "Necromancer",
                    Emoji = "☠️",
                    Description = "Thrives through endless grind and sacrifice.",
                    UnlockLevel = 20,
                    BonusCategoryName = "Nutrition"
                }
            );

            context.SaveChanges();
        }
    }
        public static void SeedAchievements(AppDbContext context)
        {
            if (!context.Achievements.Any())
            {
                context.Achievements.AddRange(
                    // Task-based
                    new Achievement { Key = "first_blood",    Name = "First Blood",     Emoji = "🩸", Description = "Complete your first task.",   Category = "tasks",  RequiredValue = 1   },
                    new Achievement { Key = "getting_started", Name = "Getting Started", Emoji = "🌱", Description = "Complete 10 tasks.",           Category = "tasks",  RequiredValue = 10  },
                    new Achievement { Key = "on_a_roll",      Name = "On a Roll",       Emoji = "🔄", Description = "Complete 50 tasks.",           Category = "tasks",  RequiredValue = 50  },
                    new Achievement { Key = "relentless",     Name = "Relentless",      Emoji = "💯", Description = "Complete 100 tasks.",          Category = "tasks",  RequiredValue = 100 },
                    new Achievement { Key = "unstoppable",    Name = "Unstoppable",     Emoji = "🚀", Description = "Complete 500 tasks.",          Category = "tasks",  RequiredValue = 500 },
                    // Streak-based
                    new Achievement { Key = "consistent",     Name = "Consistent",      Emoji = "📅", Description = "Reach a 3 day streak.",        Category = "streak", RequiredValue = 3   },
                    new Achievement { Key = "week_warrior",   Name = "Week Warrior",    Emoji = "⚔️", Description = "Reach a 7 day streak.",        Category = "streak", RequiredValue = 7   },
                    new Achievement { Key = "dedicated",      Name = "Dedicated",       Emoji = "🏆", Description = "Reach a 30 day streak.",       Category = "streak", RequiredValue = 30  },
                    // LP Level-based
                    new Achievement { Key = "newcomer",       Name = "Newcomer",        Emoji = "👋", Description = "Reach LP Level 2.",            Category = "level",  RequiredValue = 2   },
                    new Achievement { Key = "rising",         Name = "Rising",          Emoji = "📈", Description = "Reach LP Level 5.",            Category = "level",  RequiredValue = 5   },
                    new Achievement { Key = "veteran",        Name = "Veteran",         Emoji = "🎖️", Description = "Reach LP Level 10.",           Category = "level",  RequiredValue = 10  },
                    new Achievement { Key = "legend",         Name = "Legend",          Emoji = "👑", Description = "Reach LP Level 20.",           Category = "level",  RequiredValue = 20  }
                );
                context.SaveChanges();
            }
        }
    }
}
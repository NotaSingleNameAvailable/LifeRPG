using LifeRPG.Core.Models;

namespace LifeRPG.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static void SeedCategories(AppDbContext context)
        {
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category
                    {
                        Name = "Fitness",
                        Description = "Physical health tasks",
                        Icon = "💪",
                        ColorCode = "#FF6B6B"
                    },
                    new Category
                    {
                        Name = "Finance",
                        Description = "Money and budgeting",
                        Icon = "💰",
                        ColorCode = "#FFD93D"
                    },
                    new Category
                    {
                        Name = "Learning",
                        Description = "Skills and study",
                        Icon = "📘",
                        ColorCode = "#6BCB77"
                    }
                );

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
                    UnlockLevel = 0
                },

                new Character
                {
                    Cid = 2,
                    Name = "Dragon",
                    Emoji = "🐉",
                    Description = "Ancient beast of overwhelming power.",
                    UnlockLevel = 1
                },

                new Character
                {
                    Cid = 3,
                    Name = "Knight",
                    Emoji = "⚔️",
                    Description = "Disciplined warrior of honor.",
                    UnlockLevel = 2
                },

                new Character
                {
                    Cid = 4,
                    Name = "Phoenix",
                    Emoji = "🔥",
                    Description = "Always rises again after failure.",
                    UnlockLevel = 5
                },

                new Character
                {
                    Cid = 5,
                    Name = "Monk",
                    Emoji = "🧘",
                    Description = "Seeker of balance and inner peace.",
                    UnlockLevel = 6
                },

                new Character
                {
                    Cid = 6,
                    Name = "Merchant",
                    Emoji = "💰",
                    Description = "Builder of wealth and financial mastery.",
                    UnlockLevel = 9
                },

                new Character
                {
                    Cid = 7,
                    Name = "Viking",
                    Emoji = "🪓",
                    Description = "Relentless conqueror of challenges.",
                    UnlockLevel = 12
                },

                new Character
                {
                    Cid = 8,
                    Name = "Robot",
                    Emoji = "🤖",
                    Description = "Cold efficiency and optimized productivity.",
                    UnlockLevel = 15
                },

                new Character
                {
                    Cid = 9,
                    Name = "Necromancer",
                    Emoji = "☠️",
                    Description = "Thrives through endless grind and sacrifice.",
                    UnlockLevel = 20
                }


            );

                context.SaveChanges();
            }
        }
    }
}
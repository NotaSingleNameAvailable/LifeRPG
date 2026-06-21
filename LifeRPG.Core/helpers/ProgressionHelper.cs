using System;

namespace LifeRPG.Core.Helpers
{
    public static class ProgressionHelper
    {
        public static int GetRequiredPointsForNextLevel(int level)
        {
            return 100 + ((level - 1) * 50);
        }

        public static (int newLevel, int newCurrentPoints) AddPoints(int currentLevel, int currentPoints, int pointsToAdd)
        {
            int level = currentLevel;
            int points = currentPoints + pointsToAdd;

            while (points >= GetRequiredPointsForNextLevel(level))
            {
                points -= GetRequiredPointsForNextLevel(level);
                level++;
            }

            return (level, points);
        }

        public static (int newLevel, int newCurrentPoints) RemovePoints(int currentLevel, int currentPoints, int pointsToRemove)
        {
            int level = currentLevel;
            int points = currentPoints - pointsToRemove;

            while (points < 0 && level > 1)
            {
                level--;
                points += GetRequiredPointsForNextLevel(level);
            }

            if (level == 1 && points < 0)
                points = 0;

            return (level, points);
        }
    }
}
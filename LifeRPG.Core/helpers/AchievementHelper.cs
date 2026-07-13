using LifeRPG.Core.Models;

namespace LifeRPG.Core.Helpers
{
    public static class AchievementHelper
    {
        public static List<Achievement> GetEarned(
            IEnumerable<Achievement> allAchievements,
            int totalTasksCompleted,
            int streakCount,
            int lpLevel)
        {
            return allAchievements.Where(a => a.Category switch
            {
                "tasks"  => totalTasksCompleted >= a.RequiredValue,
                "streak" => streakCount >= a.RequiredValue,
                "level"  => lpLevel >= a.RequiredValue,
                _        => false
            }).ToList();
        }
    }
}
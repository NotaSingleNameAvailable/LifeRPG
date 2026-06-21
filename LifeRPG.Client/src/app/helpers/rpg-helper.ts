// src/app/helpers/rpg-helper.ts
export class RPGHelper {

  static getRequiredPointsForNextLevel(level: number): number {
    return 100 + (level - 1) * 50;
  }

  static addPoints(currentLevel: number, currentPoints: number, pointsToAdd: number): { newLevel: number, newPoints: number } {
    let level = currentLevel;
    let points = currentPoints + pointsToAdd;

    while (points >= this.getRequiredPointsForNextLevel(level)) {
      points -= this.getRequiredPointsForNextLevel(level);
      level++;
    }

    return { newLevel: level, newPoints: points };
  }

  static removePoints(currentLevel: number, currentPoints: number, pointsToRemove: number): { newLevel: number, newPoints: number } {
    let level = currentLevel;
    let points = currentPoints - pointsToRemove;

    while (points < 0 && level > 1) {
      level--;
      points += this.getRequiredPointsForNextLevel(level);
    }

    if (level === 1 && points < 0) points = 0;

    return { newLevel: level, newPoints: points };
  }

  static getXPPercent(currentPoints: number, currentLevel: number): number {
    const max = this.getRequiredPointsForNextLevel(currentLevel);
    return Math.min(100, (currentPoints / max) * 100);
  }

  static getLPPercent(currentPoints: number, currentLevel: number): number {
    // Same logic as XP
    return this.getXPPercent(currentPoints, currentLevel);
  }
}
export interface User {
  id: string;
  username: string;
  email: string;
  passwordHash: string;
  totalXP: number;
  level: number;
  streakCount: number;
  lastTaskDate?: string;
  createdAt: string;
  completedAt?: string;
}

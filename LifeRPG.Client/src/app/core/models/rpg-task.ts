import { User } from './user';
import { Category } from './category';

export interface RpgTask {
  id: string;
  name: string;
  description: string;
  xp: number;
  isCompleted: boolean;
  userId: string;
  categoryId: string;
  user?: User;
  category?: Category;
}

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RouterLink } from '@angular/router';
import { TaskService } from '../services/task.service';
import { UserService } from '../core/services/user.service';
import { RPGHelper } from '../helpers/rpg-helper';
import { UserStateService } from '../core/services/user-state.service';
import { CelebrationComponent } from '../shared/celebration/celebration.component';


interface Task {
  id: string;
  userId: string;
  title: string;
  description?: string;
  categoryId: string;
  xpValue: number;
  isCompleted: boolean;
  recurrencePattern?: string;
  dueDate?: string | null;
  createdAt: string;
  completedAt?: string | null;
  awardedCharacterId?: string | null;
}

interface UserStats {
  characterXP: number;
  characterLevel: number;
  userLP: number;
  lpLevel: number;
}

interface XpPopup {
  id: number;
  taskId: string;
  text: string;
  type: 'gain' | 'loss';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, CelebrationComponent],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
})
export class Dashboard implements OnInit {
  tasks: Task[] = [];


  userId = localStorage.getItem('userId') ?? '';


  // RPG stats
  userLP = 50;
  lpLevel = 1;
  characterXP = 0;
  characterLevel = 1;
  activeCharacter: {id: string | null;emoji: string;name?: string;} = {id: null,emoji: '🧙'};

  constructor(
    private taskService: TaskService,
    private router: Router,
    private userState: UserStateService
  ) {}

  ngOnInit(): void {
    if (!this.userId) {
      this.router.navigate(['/login']);
      return;
    }

    this.userState.load(this.userId);

    this.userState.getState().subscribe(state => {
      if (!state) return;

      this.userLP = state.userLP;
      this.lpLevel = state.lpLevel;

      this.characterXP = state.characterXP;
      this.characterLevel = state.characterLevel;

      this.activeCharacter = state.activeCharacter ? {id: state.activeCharacter.id,emoji: state.activeCharacter.emoji, name: state.activeCharacter.name } : { id: null, emoji: '🧙' };
    });

    this.loadTasks();
  }




  get activeTasks(): Task[] {
    return this.tasks.filter(task => !this.isTaskCompletedToday(task));
  }

  get completedTasks(): Task[] {
    return this.tasks.filter(task => this.isTaskCompletedToday(task));
  }


  loadTasks(): void {
    this.taskService.getTasks().subscribe({
      next: (data) => this.tasks = data.filter(task => this.shouldTaskAppearToday(task)).map(task => ({...task,awardedCharacterId: task.awardedCharacterId ?? null})),
      error: (err) => console.error('Error loading tasks', err)
    });
  }



  completeTask(task: Task): void {
    const isGaining = !task.isCompleted; // true = completing, false = uncompleting

    // For uncompleting: the character losing XP is the one who earned it, not the active one
    const relevantCharacterId = isGaining
      ? this.activeCharacter?.id ?? null
      : task.awardedCharacterId ?? null;

    const characterEmoji = relevantCharacterId
      ? this.getCharacterEmoji(relevantCharacterId)
      : null;

    // Show XP popup
    this.showPopup(task.id, task.xpValue, isGaining, characterEmoji);

 this.taskService.completeTask(task.id).subscribe({
    next: (result: any) => {
      // Check if a forced character switch happened
      if (result?.forcedCharacterSwitch) {
        this.userState.forcedCharacterSwitch$.next({
          emoji: result.switchedToEmoji,
          name: result.switchedToName
        });
      }
        // The backend returns a result object that includes two lists:
        // newlyEarned (achievements just gained) and newlyLost (achievements just lost).
        // We loop through each list and fire an event for every achievement in it.
        // CelebrationService is listening to these events and will show the popup modal for each one.
      if (result?.newlyEarned?.length) {
        result.newlyEarned.forEach((a: any) => {
          this.userState.achievementEarned$.next(a);// fires "achievement earned" event → CelebrationService shows gold modal
        });
      }

      if (result?.newlyLost?.length) {
        result.newlyLost.forEach((a: any) => {
          this.userState.achievementLost$.next(a);// fires "achievement lost" event → CelebrationService shows grey modal
        });
      }
      this.loadTasks();
      this.userState.load(this.userId);
    },
    error: (err) => {
      console.error('Could not update task FULL ERROR:', err);
    }
  });
  }

  // Progress bar helpers
  calculateCharacterXPPercent(): number {
    return RPGHelper.getXPPercent(this.characterXP, this.characterLevel);
  }

  calculateLPPercent(): number {
    return RPGHelper.getLPPercent(this.userLP, this.lpLevel);
  }

  getCharacterEmoji(characterId: string | null): string {
    if (!characterId) return '';
    const all = this.userState.getSnapshot()?.allCharacters ?? [];
    const match = all.find(c => c.id.toLowerCase() === characterId.toLowerCase());
    return match?.emoji ?? '🧙';
  }

  maxXPForLevel(level: number): number {
    return RPGHelper.getRequiredPointsForNextLevel(level);
  }

  maxLPForLevel(level: number): number {
    return RPGHelper.getRequiredPointsForNextLevel(level);
  }

  //side drawer  section
  isDrawerOpen = false;

  toggleDrawer(): void {
    this.isDrawerOpen = !this.isDrawerOpen;
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
  }

  logout(): void {
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    localStorage.removeItem('email');
    this.router.navigate(['/login']);
  }
  //side drawer  section end

  // ======================================
  // Change page functions here 
  // ======================================

    goToCreateTask(): void {
    this.router.navigate(['/task-editor']);
  }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }


  goToTodayTasks(): void {
    this.router.navigate(['/today-tasks']);
  }

  // Ending here

  openTaskEditor(task: Task): void {
    this.router.navigate(['/task-editor', task.id]);
  }

  // ======================================
  // TASK VISIBILITY LOGIC (TODAY FILTER)
  // ======================================
 shouldTaskAppearToday(task: Task): boolean {
    const today = new Date();

    const created = new Date(task.dueDate ?? task.createdAt);

    // strip time (IMPORTANT: ignore hours)
    const sameDate =
      created.getFullYear() === today.getFullYear() &&
      created.getMonth() === today.getMonth() &&
      created.getDate() === today.getDate();

    // NON-RECURRING TASKS
    if (!task.recurrencePattern) {
      return sameDate;
    }

    // DAILY TASKS
    if (task.recurrencePattern === 'daily') {
      return true;
    }

    return false;
 }

  private isSameDate(date1: Date, date2: Date): boolean {
    return (
      date1.getFullYear() === date2.getFullYear() &&
      date1.getMonth() === date2.getMonth() &&
      date1.getDate() === date2.getDate()
    );
}



isTaskCompletedToday(task: Task): boolean {

  if (!task.recurrencePattern || task.recurrencePattern === '') {
    return task.isCompleted;
  }

  if (task.recurrencePattern === 'daily') {
    if (!task.completedAt) return false;

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const completed = new Date(task.completedAt);
    completed.setHours(0, 0, 0, 0);

    return this.isSameDate(today, completed);
  }

  return false;
}

  // ======================================
  // XP popup starts here
  // ======================================
  xpPopups: XpPopup[] = [];
  private popupCounter = 0;

  showPopup(taskId: string, xpValue: number, isGaining: boolean, characterEmoji: string | null): void {
    const sign = isGaining ? '+' : '-';
    const emoji = characterEmoji ? `${characterEmoji} ` : '';
    const text = `${emoji}${sign}${xpValue} XP    ${sign}${xpValue} LP`;

    const popup: XpPopup = {
      id: ++this.popupCounter,
      taskId,
      text,
      type: isGaining ? 'gain' : 'loss'
    };

    this.xpPopups.push(popup);
    setTimeout(() => {
      this.xpPopups = this.xpPopups.filter(p => p.id !== popup.id);
    }, 1400);
  }
  // ======================================
  // XP popup ends here
  // ======================================

}
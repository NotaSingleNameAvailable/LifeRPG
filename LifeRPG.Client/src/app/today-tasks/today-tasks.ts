import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { UserStateService } from '../core/services/user-state.service';
import { RouterLink } from '@angular/router';
import { CelebrationComponent } from '../shared/celebration/celebration.component';


interface DailyTask {
  id: number;
  title: string;
  completed: boolean;
  awardedCharacterId?: string | null;
}

interface ActiveCharacterView {
  emoji: string;
  name?: string;
}

interface DailyTaskPopup {
  id: number;
  taskId: number;
  text: string;
  type: 'gain' | 'loss';
}

@Component({
  selector: 'app-today-tasks',
  standalone: true,
  imports: [CommonModule , RouterLink, CelebrationComponent],
  templateUrl: './today-tasks.html',
  styleUrls: ['./today-tasks.css']
})
export class TodayTasks implements OnInit {
  dailyTasks: DailyTask[] = [];

  // ===== RPG STATS (active character only) =====
  characterXP = 0;
  characterLevel = 1;
  userLP = 50;
  lpLevel = 1;

  activeCharacterId: string | null = null;
  activeCharacter: ActiveCharacterView = { emoji: '🧙' };

  private readonly TASK_XP = 10;
  private readonly TASK_LP = 10;

  private readonly TASK_POOL: string[] = [
    'Drink 2L of water',
    'Take a 10 minute walk',
    'Stretch for 5 minutes',
    'Read 10 pages',
    'Clean your desk',
    'Eat a fruit',
    'Go outside for sunlight',
    'Do 20 pushups',
    'Meditate for 5 minutes',
    'Write tomorrow\'s plan',
    'No sugary drinks today',
    'Review your goals',
    'Take vitamins',
    'Make your bed',
    'Listen to an educational podcast'
  ];

  constructor(
    private router: Router,
    private userState: UserStateService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    const userId = localStorage.getItem('userId') ?? '';

    this.loadTodayTasks();

    this.userState.load(userId);

    this.userState.getState().subscribe(state => {
      if (!state) return;

      this.userLP = state.userLP;
      this.lpLevel = state.lpLevel;

      this.characterXP = state.characterXP;
      this.characterLevel = state.characterLevel;

      this.activeCharacterId = state.activeCharacter?.id ?? null;
      this.activeCharacter = state.activeCharacter ?? { emoji: '🧙' };
    });
  }

  // ======================================
  // NAVIGATION
  // ======================================

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  goToCreateTask(): void {
    this.router.navigate(['/task-editor']);
  }

  goToTodayTasks(): void {
    this.router.navigate(['/today-tasks']);
  }



  // ======================================
  // SIMPLE XP HELPERS
  // ======================================

  maxXPForLevel(level: number): number {
    return 100 + ((Math.max(level, 1) - 1) * 50);
  }

  maxLPForLevel(level: number): number {
    return 100 + ((Math.max(level, 1) - 1) * 50);
  }

  calculateCharacterXPPercent(): number {
    const max = this.maxXPForLevel(this.characterLevel);
    return max > 0 ? Math.min((this.characterXP / max) * 100, 100) : 0;
  }

  calculateLPPercent(): number {
    const max = this.maxLPForLevel(this.lpLevel);
    return max > 0 ? Math.min((this.userLP / max) * 100, 100) : 0;
  }

  // ======================================
  // COMPUTED GETTERS
  // ======================================

  get completedCount(): number {
    return this.dailyTasks.filter(t => t.completed).length;
  }

  get allTasksCompleted(): boolean {
    return this.dailyTasks.length > 0 && this.dailyTasks.every(t => t.completed);
  }

  // ======================================
  // TODAY TASKS
  // ======================================

  private getTodayKey(): string {
    const userId = localStorage.getItem('userId') ?? 'unknown';
    return `todayTasks_${userId}_${new Date().toISOString().split('T')[0]}`;
  }

  private loadTodayTasks(): void {
    const todayKey = this.getTodayKey();
    const savedTasks = localStorage.getItem(`todayTasks_${todayKey}`);

    if (savedTasks) {
      this.dailyTasks = JSON.parse(savedTasks);
      return;
    }

    const shuffled = [...this.TASK_POOL].sort(() => Math.random() - 0.5);

    this.dailyTasks = shuffled
      .slice(0, 6)
      .map((title, index) => ({
        id: index + 1,
        title,
        completed: false,
        awardedCharacterId: null
      }));

    this.saveTodayTasks();
  }

  // ======================================
  // TOGGLE TASK
  // ======================================

  toggleTask(task: DailyTask): void {
    const userId = localStorage.getItem('userId') ?? '';
    if (!userId) return;

    const activeCharacterId = this.userState.getSnapshot()?.activeCharacter?.id ?? null;
    const isCompleting = !task.completed;

    const previousCompleted = task.completed;
    const previousAwardedCharacterId = task.awardedCharacterId ?? null;

    if (!task.awardedCharacterId) {
      task.awardedCharacterId = activeCharacterId;
    }

    const targetCharacterId = task.awardedCharacterId;

    if (!targetCharacterId) {
      console.error('No character available to assign this daily task.');
      return;
    }

    // For uncompleting: emoji of the character losing XP, not the active one
    const relevantCharacterId = isCompleting ? activeCharacterId : task.awardedCharacterId;
    const characterEmoji = relevantCharacterId
      ? this.getCharacterEmoji(relevantCharacterId)
      : null;

    //  Show popup
    this.showPopup(task.id, this.TASK_XP, isCompleting, characterEmoji);

    task.completed = isCompleting;

    if (!isCompleting) {
      task.awardedCharacterId = null;
    }

    this.http.post(`http://localhost:5266/api/user/${userId}/apply-task-delta`, {
      characterId: targetCharacterId,
      xpDelta: isCompleting ? this.TASK_XP : -this.TASK_XP,
      lpDelta: isCompleting ? this.TASK_LP : -this.TASK_LP
    }).subscribe({
      next: (result: any) => {
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
        this.saveTodayTasks();
        this.userState.load(userId);
      },
      error: (err) => {
        task.completed = previousCompleted;
        task.awardedCharacterId = previousAwardedCharacterId;
        console.error('Failed to apply daily task delta', err);
      }
    });
  }

  // ======================================
  // SAVE
  // ======================================

  private saveTodayTasks(): void {
    const todayKey = this.getTodayKey();
    localStorage.setItem(`todayTasks_${todayKey}`, JSON.stringify(this.dailyTasks));
  }

  // ======================================
  // getCharacterEmoji to show emoji of character that completed task
  // ======================================

  getCharacterEmoji(characterId: string | null): string {
    if (!characterId) return '';
    const all = this.userState.getSnapshot()?.allCharacters ?? [];
    const match = all.find(c => c.id.toLowerCase() === characterId.toLowerCase());
    return match?.emoji ?? '🧙';
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
  // xpPopups start
  // ======================================
  xpPopups: DailyTaskPopup[] = [];
  private popupCounter = 0;

  showPopup(taskId: number, xpValue: number, isGaining: boolean, characterEmoji: string | null): void {
    const sign = isGaining ? '+' : '-';
    const emoji = characterEmoji ? `${characterEmoji} ` : '';
    const text = `${emoji}${sign}${xpValue} XP    ${sign}${xpValue} LP`;

    const popup: DailyTaskPopup = {
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
  // xpPopups end
  // ======================================



}
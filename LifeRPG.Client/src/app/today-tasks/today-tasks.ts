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
  categoryName: string;  
  categoryEmoji: string; 
}

interface ActiveCharacterView {
  emoji: string;
  name?: string;
}

interface DailyTaskPopup {
  id: number;
  text: string;
  type: 'gain' | 'loss';
  y: number;
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

  private readonly TASK_POOL: { title: string; categoryName: string; categoryEmoji: string }[] = [
    { title: 'Drink 2L of water',                   categoryName: 'Nutrition',    categoryEmoji: '🥗' },
    { title: 'Take a 10 minute walk',               categoryName: 'Fitness',      categoryEmoji: '💪' },
    { title: 'Stretch for 5 minutes',               categoryName: 'Fitness',      categoryEmoji: '💪' },
    { title: 'Read 10 pages',                       categoryName: 'Learning',     categoryEmoji: '📘' },
    { title: 'Clean your desk',                     categoryName: 'Discipline',   categoryEmoji: '⚡' },
    { title: 'Eat a fruit',                         categoryName: 'Nutrition',    categoryEmoji: '🥗' },
    { title: 'Go outside for sunlight',             categoryName: 'Adventure',    categoryEmoji: '🗺️' },
    { title: 'Do 20 pushups',                       categoryName: 'Fitness',      categoryEmoji: '💪' },
    { title: 'Meditate for 5 minutes',              categoryName: 'Mindfulness',  categoryEmoji: '🧘' },
    { title: 'Write tomorrow\'s plan',              categoryName: 'Discipline',   categoryEmoji: '⚡' },
    { title: 'No sugary drinks today',              categoryName: 'Nutrition',    categoryEmoji: '🥗' },
    { title: 'Review your goals',                   categoryName: 'Mindfulness',  categoryEmoji: '🧘' },
    { title: 'Take vitamins',                       categoryName: 'Nutrition',    categoryEmoji: '🥗' },
    { title: 'Make your bed',                       categoryName: 'Discipline',   categoryEmoji: '⚡' },
    { title: 'Listen to an educational podcast',    categoryName: 'Learning',     categoryEmoji: '📘' },
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
      .map((item, index) => ({
        id: index + 1,
        title: item.title,
        completed: false,
        awardedCharacterId: null,
        categoryName: item.categoryName, 
        categoryEmoji: item.categoryEmoji  
      }));

    this.saveTodayTasks();
  }

  // ======================================
  // TOGGLE TASK
  // ======================================

  toggleTask(task: DailyTask, event: MouseEvent): void {
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

    task.completed = isCompleting;

    if (!isCompleting) {
      task.awardedCharacterId = null;
    }

    this.http.post(`http://localhost:5266/api/user/${userId}/apply-task-delta`, {
      characterId: targetCharacterId,
      xpDelta: isCompleting ? this.TASK_XP : -this.TASK_XP,
      lpDelta: isCompleting ? this.TASK_LP : -this.TASK_LP,
      categoryName: task.categoryName
    }).subscribe({
      next: (result: any) => {
        const actualXp = Math.abs(result?.actualXpAwarded ?? this.TASK_XP);
        const actualLp = Math.abs(result?.actualLpAwarded ?? this.TASK_LP);
        //  Show popup
        this.showPopup(actualXp, actualLp, isCompleting, characterEmoji, event.clientY)
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
    this.userState.clear();
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    localStorage.removeItem('email');
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
  //side drawer  section end




  // ======================================
  // xpPopups start
  // ======================================
  xpPopups: DailyTaskPopup[] = [];
  private popupCounter = 0;

  showPopup(xpValue: number, lpValue: number, isGaining: boolean, characterEmoji: string | null, y: number): void {
    const sign = isGaining ? '+' : '-';
    const emoji = characterEmoji ? `${characterEmoji} ` : '';
    const text = `${emoji}${sign}${xpValue} XP    ${sign}${lpValue} LP`;

    const popup: DailyTaskPopup = {
      id: ++this.popupCounter,
      text,
      type: isGaining ? 'gain' : 'loss',
      y
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
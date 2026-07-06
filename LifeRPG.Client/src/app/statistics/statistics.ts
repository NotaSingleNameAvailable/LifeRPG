import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';

interface CharacterStat {
  characterId: string;
  characterName: string;
  characterEmoji: string;
  tasksCompleted: number;
  totalXP: number;
  level: number;
  isUnlocked: boolean;
  unlockLevel: number;
  percentage: number;
}

@Component({
  selector: 'app-statistics',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './statistics.html',
  styleUrls: ['./statistics.css']
})
export class Statistics implements OnInit {
  userId = localStorage.getItem('userId') ?? '';
  streakCount = 0;
  characters: CharacterStat[] = [];
  sortMode: 'mostUsed' | 'alphabetical' = 'mostUsed';

  // Side drawer
  isDrawerOpen = false;

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    if (!this.userId) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.http.get<any>(`http://localhost:5266/api/statistics/${this.userId}`)
      .subscribe({
        next: (data) => {
          this.streakCount = data.streakCount;
          const total = data.characters.reduce((sum: number, c: any) => sum + c.tasksCompleted, 0);
          this.characters = data.characters.map((c: any) => ({
            characterId: c.characterId,
            characterName: c.characterName,
            characterEmoji: c.characterEmoji,
            tasksCompleted: c.tasksCompleted,
            totalXP: c.totalXP,
            level: c.level,
            isUnlocked: c.isUnlocked,
            unlockLevel: c.unlockLevel,
            percentage: total > 0 ? Math.round((c.tasksCompleted / total) * 100) : 0
          }));
        },
        error: (err) => console.error('Failed to load statistics', err)
      });
  }

  get sortedCharacters(): CharacterStat[] {
    const list = [...this.characters];
    if (this.sortMode === 'alphabetical') {
      return list.sort((a, b) => a.characterName.localeCompare(b.characterName));
    }
    return list.sort((a, b) => b.tasksCompleted - a.tasksCompleted);
  }

  setSortMode(mode: 'mostUsed' | 'alphabetical'): void {
    this.sortMode = mode;
  }

  // Drawer
  toggleDrawer(): void { this.isDrawerOpen = !this.isDrawerOpen; }
  closeDrawer(): void { this.isDrawerOpen = false; }

  logout(): void {
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    localStorage.removeItem('email');
    this.router.navigate(['/login']);
  }
}
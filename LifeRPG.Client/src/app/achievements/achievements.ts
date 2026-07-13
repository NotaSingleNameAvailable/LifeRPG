import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';

interface AchievementView {
  id: string;
  key: string;
  name: string;
  emoji: string;
  description: string;
  category: string;
  requiredValue: number;
  isEarned: boolean;
  earnedAt?: string;
}

@Component({
  selector: 'app-achievements',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './achievements.html',
  styleUrls: ['./achievements.css']
})
export class Achievements implements OnInit {
  userId = localStorage.getItem('userId') ?? '';
  achievements: AchievementView[] = [];
  isDrawerOpen = false;

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    if (!this.userId) { this.router.navigate(['/login']); return; }
    this.loadAchievements();
  }

  loadAchievements(): void {
    this.http.get<AchievementView[]>(`http://localhost:5266/api/achievement/${this.userId}`)
      .subscribe({
        next: (data) => this.achievements = data,
        error: (err) => console.error('Failed to load achievements', err)
      });
  }

  get taskAchievements() { return this.achievements.filter(a => a.category === 'tasks'); }
  get streakAchievements() { return this.achievements.filter(a => a.category === 'streak'); }
  get levelAchievements() { return this.achievements.filter(a => a.category === 'level'); }

  get earnedCount() { return this.achievements.filter(a => a.isEarned).length; }

  toggleDrawer(): void { this.isDrawerOpen = !this.isDrawerOpen; }
  closeDrawer(): void { this.isDrawerOpen = false; }

  logout(): void {
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    localStorage.removeItem('email');
    this.router.navigate(['/login']);
  }
}
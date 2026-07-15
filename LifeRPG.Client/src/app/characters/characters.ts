import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink , Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { UserStateService } from '../core/services/user-state.service';

interface CharacterCard {
  id: string;
  cid: number;
  characterName: string;
  characterEmoji: string;
  description: string;
  unlockLevel: number;
  isUnlocked: boolean;
  isActive: boolean;
  totalXP: number;
  currentXP: number;
  level: number;
}

@Component({
  selector: 'app-characters',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './characters.html',
  styleUrls: ['./characters.css']
})
export class Characters implements OnInit {

  userId = localStorage.getItem('userId') ?? '';
  characters: CharacterCard[] = [];
  isDrawerOpen = false;

  constructor(private http: HttpClient , private router: Router, private userState: UserStateService) {}

  ngOnInit(): void {
    this.loadCharacters();
  }

  loadCharacters(): void {
    this.http.get<CharacterCard[]>(`http://localhost:5266/api/character/${this.userId}`)
      .subscribe({
        next: (data) => {
          this.characters = data;
        },
        error: (err) => {
          console.error('Could not load characters', err);
        }
      });
  }

  getXPPercent(currentXP: number, level: number): number {
    const safeLevel = Math.max(level || 1, 1);
    const required = 100 + ((safeLevel - 1) * 50);
    return required > 0 ? (currentXP / required) * 100 : 0;
  }

  getRequiredXP(level: number): number {
    const safeLevel = Math.max(level || 1, 1);
    return 100 + ((safeLevel - 1) * 50);
  }

  selectCharacter(characterId: string): void {
    if (!this.userId) return;

    this.http.put(
      `http://localhost:5266/api/character/select/${this.userId}/${characterId}`,
      {}
    ).subscribe({
      next: () => {
        this.loadCharacters();
      },
      error: (err) => console.error('Could not select character', err)
    });
  }

  toggleDrawer(): void { this.isDrawerOpen = !this.isDrawerOpen; }
  closeDrawer(): void { this.isDrawerOpen = false; }

  logout(): void {
    this.userState.clear();
    localStorage.removeItem('userId');
    localStorage.removeItem('username');
    localStorage.removeItem('email');
    this.router.navigate(['/login']);
  }
}
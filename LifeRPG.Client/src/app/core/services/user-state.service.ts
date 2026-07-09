import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { HttpClient } from '@angular/common/http';

export interface UserState {
  userId: string;
  userLP: number;
  lpLevel: number;
  characterXP: number;
  characterLevel: number;
  activeCharacter: { id: string; emoji: string; name?: string; } | null;
  allCharacters: { id: string; emoji: string; name: string; unlockLevel: number; }[];
}

export interface CharacterUnlockEvent {
  emoji: string;
  name: string;
}

export interface ForcedSwitchEvent {
  emoji: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class UserStateService {
  private state$ = new BehaviorSubject<UserState | null>(null);

  // Event streams components subscribe to
  characterLevelUp$ = new Subject<void>();
  lifeLevelUp$ = new Subject<void>();
  characterUnlocked$ = new Subject<CharacterUnlockEvent>();
  forcedCharacterSwitch$ = new Subject<ForcedSwitchEvent>();


  constructor(private http: HttpClient) {}

  load(userId: string) {
    return this.http.get<any>(`http://localhost:5266/api/user/${userId}/me`)
      .subscribe(data => {
        const active = data.characters.find(
          (c: any) => c.characterId === data.activeCharacterId
        );

        const newState: UserState = {
          userId,
          userLP: data.currentLifePoints,
          lpLevel: data.lifeLevel,
          characterXP: data.characterXP,
          characterLevel: data.characterLevel,
          activeCharacter: active ? {
            id: active.characterId,
            emoji: active.characterEmoji,
            name: active.characterName
          } : null,
          allCharacters: data.characters.map((c: any) => ({
            id: c.characterId,
            emoji: c.characterEmoji,
            name: c.characterName,
            unlockLevel: c.unlockLevel
          }))
        };

        // Compare old state vs new state before replacing
        const old = this.state$.value;
        if (old) {
          // Character level up
          if (newState.characterLevel > old.characterLevel) {
            this.characterLevelUp$.next();
          }

          // Life level up
          if (newState.lpLevel > old.lpLevel) {
            this.lifeLevelUp$.next();

            // Check for newly unlocked characters
            const oldLevel = old.lpLevel;
            const newLevel = newState.lpLevel;

            // Find characters whose unlockLevel falls in the range gained
            const unlocked = newState.allCharacters.filter(c =>
              c.unlockLevel > oldLevel && c.unlockLevel <= newLevel
            );

            unlocked.forEach(c => {
              this.characterUnlocked$.next({ emoji: c.emoji, name: c.name });
            });
          }
        }

        this.state$.next(newState);
      });
  }

  getState() { return this.state$.asObservable(); }
  getSnapshot() { return this.state$.value; }
}
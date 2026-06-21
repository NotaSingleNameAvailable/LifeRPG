import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { HttpClient } from '@angular/common/http';

export interface UserState {
  userId: string;

  userLP: number;
  lpLevel: number;

  characterXP: number;
  characterLevel: number;

  activeCharacter: {
    id: string;
    emoji: string;
    name?: string;
  } | null;
}

@Injectable({ providedIn: 'root' })
export class UserStateService {

  private state$ = new BehaviorSubject<UserState | null>(null);

  constructor(private http: HttpClient) {}

  // LOAD FROM BACKEND (single source of truth)
  load(userId: string) {
    return this.http.get<any>(`http://localhost:5266/api/user/${userId}/me`)
      .subscribe(data => {

        const active = data.characters.find(
          (c: any) => c.characterId === data.activeCharacterId
        );

        const state: UserState = {
          userId,

          userLP: data.currentLifePoints,
          lpLevel: data.lifeLevel,

          characterXP: data.characterXP,
          characterLevel: data.characterLevel,

          activeCharacter: active ? {
            id: active.characterId,
            emoji: active.characterEmoji,
            name: active.characterName
          } : null
        };

        this.state$.next(state);
      });
  }

  // OBSERVABLE FOR COMPONENTS
  getState() {
    return this.state$.asObservable();
  }

  // SNAPSHOT ACCESS
  getSnapshot() {
    return this.state$.value;
  }

}
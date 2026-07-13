import { Injectable, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { UserStateService, CharacterUnlockEvent, ForcedSwitchEvent, AchievementEvent } from './user-state.service';



@Injectable({ providedIn: 'root' })
export class CelebrationService implements OnDestroy {
  private subs = new Subscription();

  // State visible to components
  showCharacterLevelUp = false;
  showLifeLevelUp = false;
  unlockQueue: CharacterUnlockEvent[] = [];
  currentUnlock: CharacterUnlockEvent | null = null;
  showForcedSwitch: ForcedSwitchEvent | null = null;
  earnedQueue: AchievementEvent[] = [];
  currentEarned: AchievementEvent | null = null;
  lostQueue: AchievementEvent[] = [];
  currentLost: AchievementEvent | null = null;

  constructor(private userState: UserStateService) {
    this.subs.add(
      this.userState.characterLevelUp$.subscribe(() => {
        this.showCharacterLevelUp = true;
        setTimeout(() => this.showCharacterLevelUp = false, 2000);
      })
    );

    this.subs.add(
      this.userState.lifeLevelUp$.subscribe(() => {
        this.showLifeLevelUp = true;
        setTimeout(() => this.showLifeLevelUp = false, 2500);
      })
    );

    this.subs.add(
      this.userState.characterUnlocked$.subscribe((event) => {
        this.unlockQueue.push(event);
        if (!this.currentUnlock) this.showNextUnlock();
      })
    );

      this.subs.add(
        this.userState.forcedCharacterSwitch$.subscribe((event) => {
        this.showForcedSwitch = event;
       })
    );
    this.subs.add(
    this.userState.achievementEarned$.subscribe(event => {
        this.earnedQueue.push(event);
        if (!this.currentEarned) this.showNextEarned();
      })
    );

    this.subs.add(
      this.userState.achievementLost$.subscribe(event => {
        this.lostQueue.push(event);
        if (!this.currentLost) this.showNextLost();
      })
    );
  }

  showNextUnlock(): void {
    if (this.unlockQueue.length === 0) {
      this.currentUnlock = null;
      return;
    }
    this.currentUnlock = this.unlockQueue.shift()!;
  }

  closeUnlock(): void {
    this.currentUnlock = null;
    setTimeout(() => this.showNextUnlock(), 300);
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  closeForcedSwitch(): void {
  this.showForcedSwitch = null;
  }

  showNextEarned(): void {
  this.currentEarned = this.earnedQueue.length > 0 ? this.earnedQueue.shift()! : null;
}

closeEarned(): void {
  this.currentEarned = null;
  setTimeout(() => this.showNextEarned(), 300);
}

showNextLost(): void {
  this.currentLost = this.lostQueue.length > 0 ? this.lostQueue.shift()! : null;
}

closeLost(): void {
  this.currentLost = null;
  setTimeout(() => this.showNextLost(), 300);
}
}
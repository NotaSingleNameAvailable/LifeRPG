import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CelebrationService } from '../../core/services/celebration.service';
import { UserStateService } from '../../core/services/user-state.service';

@Component({
  selector: 'app-celebration',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './celebration.component.html',
  styleUrls: ['./celebration.component.css']
})
export class CelebrationComponent {
  constructor(
    public celebration: CelebrationService,
    public userState: UserStateService
  ) {}
}
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TaskService } from '../services/task.service';
import { CategoryService } from '../services/category.service';

/* =========================================================
   TASK MODEL (backend shape reference only)
   ========================================================= */
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
}

@Component({
  selector: 'app-task-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './task-editor.html',
  styleUrls: ['./task-editor.css'],
})
export class TaskEditor implements OnInit {

  /* =========================================================
     AUTH / CONTEXT
     ========================================================= */
  userId = localStorage.getItem('userId') ?? '';

  /* =========================================================
     UI DATA
     ========================================================= */
  categories: any[] = [];

  /* =========================================================
     ROUTE STATE
     ========================================================= */
  taskId: string | null = null;
  isEditMode = false;

  /* =========================================================
     FORM STATE (ONLY UI INPUTS, NOT GAME LOGIC)
     NOTE: xpValue is NOT stored (derived from difficulty)
     ========================================================= */
  taskForm = {
    title: '',
    description: '',
    categoryId: '',
    recurrencePattern: '',

    // difficulty is the ONLY source of XP/LP
    difficulty: 'easy' as 'easy' | 'medium' | 'hard' | 'insane'
  };

    formSubmitted = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private taskService: TaskService,
    private categoryService: CategoryService
  ) {}

  /* =========================================================
     INIT
     ========================================================= */
  ngOnInit(): void {
    if (!this.userId) {
      this.router.navigate(['/login']);
      return;
    }

    this.loadCategories();

    this.taskId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.taskId;

      if (!this.isEditMode) {
        this.formSubmitted = true;
      }

    if (this.isEditMode && this.taskId) {
      this.loadTask(this.taskId);
    }
  }

  /* =========================================================
     LOAD CATEGORIES (dropdown)
     ========================================================= */
  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (data) => this.categories = data,
      error: (err) => console.error('Error loading categories', err)
    });
  }

  /* =========================================================
     LOAD TASK (EDIT MODE)
     ========================================================= */
  loadTask(taskId: string): void {
    this.taskService.getTasks().subscribe({
      next: (tasks) => {
        const task = tasks.find(t => t.id === taskId && t.userId === this.userId);

        if (!task) {
          console.error('Task not found');
          this.router.navigate(['/dashboard']);
          return;
        }

        // populate form from backend task
        this.taskForm = {
          title: task.title ?? '',
          description: task.description ?? '',
          categoryId: task.categoryId ?? '',
          recurrencePattern: task.recurrencePattern || '',
          difficulty: this.mapXpToDifficulty(task.xpValue)
        };
      },
      error: (err) => {
        console.error('Error loading task', err);
        this.router.navigate(['/dashboard']);
      }
    });
  }

  /* =========================================================
     XP SYSTEM (CORE GAME LOGIC)
     ========================================================= */

  // Convert difficulty → XP
  private getXpFromDifficulty(): number {
    switch (this.taskForm.difficulty) {
      case 'easy': return 10;
      case 'medium': return 20;
      case 'hard': return 30;
      case 'insane': return 50;
      default: return 10;
    }
  }

  // LP is always half XP
  private getLpFromDifficulty(): number {
    return Math.floor(this.getXpFromDifficulty() / 2);
  }

  // Used when loading an existing task (reverse mapping)
  private mapXpToDifficulty(xp: number): 'easy' | 'medium' | 'hard' | 'insane' {
    switch (xp) {
      case 10: return 'easy';
      case 20: return 'medium';
      case 30: return 'hard';
      case 50: return 'insane';
      default: return 'easy';
    }
  }

  setDifficulty(level: 'easy' | 'medium' | 'hard' | 'insane'): void {
  this.taskForm.difficulty = level;
}

  /* =========================================================
     FORM VALIDATION
     ========================================================= */



  isTitleInvalid(): boolean {
    return this.formSubmitted && this.taskForm.title.trim().length === 0;
}

isCategoryInvalid(): boolean {
  return this.formSubmitted && this.taskForm.categoryId.trim().length === 0;
}

isFormValid(): boolean {
  return !this.isTitleInvalid() && !this.isCategoryInvalid();
}

isFormInvalid(): boolean {
  return !this.isFormValid();
}
  /* =========================================================
     SAVE TASK (CREATE / UPDATE)
     ========================================================= */
  saveTask(): void {

    this.formSubmitted = true;

    if (!this.isFormValid()) return;

    const payload = {
      userId: this.userId,
      categoryId: this.taskForm.categoryId,
      title: this.taskForm.title.trim(),
      description: this.taskForm.description.trim() || null,
      recurrencePattern: this.taskForm.recurrencePattern,

      // IMPORTANT: XP comes from difficulty ONLY
      xpValue: this.getXpFromDifficulty(),

    
    };

    if (this.isEditMode && this.taskId) {

      this.taskService.updateTask(this.taskId, payload).subscribe({
        next: () => this.router.navigate(['/dashboard']),
        error: (err) => console.error('Task update failed', err)
      });

    } else {

      this.taskService.createTask(payload).subscribe({
        next: () => this.router.navigate(['/dashboard']),
        error: (err) => console.error('Task creation failed', err)
      });

    }
  }

  /* =========================================================
     DELETE TASK
     ========================================================= */
  deleteTask(): void {
    if (!this.taskId) return;

    this.taskService.deleteTask(this.taskId).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => console.error('Task deletion failed', err)
    });
  }

  /* =========================================================
     NAVIGATION
     ========================================================= */
  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  /* =========================================================
     UI HELPERS (for template)
     ========================================================= */

  // Live XP/LP preview in UI

  get xpValue(): number {
    return this.getXpFromDifficulty();
  }
  get previewLP(): number {
    return this.getLpFromDifficulty();
  }


}
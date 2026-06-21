import { Routes } from '@angular/router';
import { Register } from './auth/register/register';
import { Login } from './auth/login/login';
import { Dashboard } from './dashboard/dashboard';
import { TaskEditor } from './task-editor/task-editor';
import { Characters } from './characters/characters';
import { TodayTasks } from './today-tasks/today-tasks';

export const routes: Routes = [
  { path: 'register', component: Register },
  { path: 'login', component: Login },
  { path: 'dashboard', component: Dashboard },
  { path: 'today-tasks', component: TodayTasks },
  { path: 'characters', component: Characters },

  // task editor routes
  { path: 'task-editor', component: TaskEditor },
  { path: 'task-editor/:id', component: TaskEditor },
  { path: '', redirectTo: '/register', pathMatch: 'full' }
];
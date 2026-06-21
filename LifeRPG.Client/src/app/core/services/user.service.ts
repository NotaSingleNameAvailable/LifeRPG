import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserStats {
  characterXP: number;
  characterLevel: number;
  userLP: number;
  lpLevel: number;
}

export interface User {
  id: string;
  username: string;
  email: string;
  totalLifePoints: number;
  currentLifePoints: number;
  lifeLevel: number;
  streakCount: number;
  createdAt: string;
  characterXP: number;
  characterLevel: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5266/api/user';

  constructor(private http: HttpClient) {}

  register(user: { username: string; email: string; password: string }): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/register`, user);
  }


  login(username: string, password: string): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/login`, { username, password });
  }

  getUserStats(userId: string): Observable<UserStats> {
    return this.http.get<UserStats>(`${this.apiUrl}/${userId}/stats`);
  }

  updateUserStats(userId: string, stats: UserStats): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${userId}/stats`, stats);
  }
}
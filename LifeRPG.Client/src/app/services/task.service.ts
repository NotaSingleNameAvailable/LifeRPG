import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private apiUrl = 'http://localhost:5266/api/task';

  constructor(private http: HttpClient) {}

  getTasks(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  createTask(task: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, task);
  }

    // ✅ Add this method
  completeTask(taskId: string | undefined): Observable<any> {
    if (!taskId) throw new Error('Task ID is required');
    return this.http.put<any>(`${this.apiUrl}/${taskId}/complete`, {});
  }

    updateTask(taskId: string, task: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${taskId}`, task);
  }

  deleteTask(taskId: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${taskId}`);
  }
}
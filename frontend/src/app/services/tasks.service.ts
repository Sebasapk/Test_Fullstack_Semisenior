import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { CreateTaskRequest } from '../models/create-task-request';
import { Task } from '../models/task';
import { TaskStatus } from '../models/task-status';

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/tasks`;

  getAll(filters: { status?: TaskStatus; userId?: number } = {}): Observable<Task[]> {
    let params = new HttpParams();
    if (filters.status) {
      params = params.set('status', filters.status);
    }
    if (filters.userId) {
      params = params.set('userId', filters.userId);
    }
    return this.http.get<Task[]>(this.url, { params });
  }

  create(task: CreateTaskRequest): Observable<Task> {
    return this.http.post<Task>(this.url, task);
  }

  updateStatus(id: number, status: TaskStatus): Observable<Task> {
    return this.http.put<Task>(`${this.url}/${id}/status`, { status });
  }
}

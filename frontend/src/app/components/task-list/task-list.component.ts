import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';

import { Task } from '../../models/task';
import { TaskStatus } from '../../models/task-status';
import { TasksService } from '../../services/tasks.service';

@Component({
  selector: 'app-task-list',
  imports: [DatePipe],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.css'
})
export class TaskListComponent implements OnInit {
  private readonly tasksService = inject(TasksService);

  protected readonly statuses = Object.values(TaskStatus);
  protected readonly tasks = signal<Task[]>([]);
  private selectedStatus: TaskStatus | undefined;

  ngOnInit(): void {
    this.loadTasks();
  }

  protected onStatusFilterChange(value: string): void {
    this.selectedStatus = value ? (value as TaskStatus) : undefined;
    this.loadTasks();
  }

  private loadTasks(): void {
    this.tasksService
      .getAll({ status: this.selectedStatus })
      .subscribe((tasks) => this.tasks.set(tasks));
  }
}

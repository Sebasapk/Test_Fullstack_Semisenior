import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';

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
  protected readonly actionLabels: Record<TaskStatus, string> = {
    [TaskStatus.Pending]: 'Mark as pending',
    [TaskStatus.InProgress]: 'Mark as in progress',
    [TaskStatus.Done]: 'Mark as done'
  };
  protected readonly tasks = signal<Task[]>([]);
  protected readonly loading = signal(false);
  private selectedStatus: TaskStatus | undefined;

  ngOnInit(): void {
    this.reload();
  }

  protected onStatusFilterChange(value: string): void {
    this.selectedStatus = value ? (value as TaskStatus) : undefined;
    this.reload();
  }

  protected availableStatuses(task: Task): TaskStatus[] {
    return this.statuses.filter(
      (status) =>
        status !== task.status &&
        !(task.status === TaskStatus.Pending && status === TaskStatus.Done)
    );
  }

  // AdditionalData is free-form JSON, so the API does not guarantee that tags is an array.
  protected tagsOf(task: Task): string[] {
    const tags = task.additionalData?.tags;
    return Array.isArray(tags) ? tags : [];
  }

  protected changeStatus(task: Task, status: TaskStatus): void {
    this.tasksService.updateStatus(task.id, status).subscribe(() => this.reload());
  }

  reload(): void {
    this.loading.set(true);
    this.tasksService
      .getAll({ status: this.selectedStatus })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe((tasks) => this.tasks.set(tasks));
  }
}

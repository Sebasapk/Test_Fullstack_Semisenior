import { Component, ChangeDetectionStrategy } from '@angular/core';

import { TaskFormComponent } from '../components/task-form/task-form.component';
import { TaskListComponent } from '../components/task-list/task-list.component';

@Component({
  selector: 'app-tasks-page',
  imports: [TaskFormComponent, TaskListComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  template: `
    <div class="page-layout">
      <app-task-form (created)="taskList.reload()" />
      <app-task-list #taskList />
    </div>
  `,
})
export class TasksPageComponent {}

import { Component } from '@angular/core';

import { TaskFormComponent } from './components/task-form/task-form.component';
import { TaskListComponent } from './components/task-list/task-list.component';
import { UserListComponent } from './components/user-list/user-list.component';

@Component({
  selector: 'app-root',
  imports: [TaskFormComponent, TaskListComponent, UserListComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}

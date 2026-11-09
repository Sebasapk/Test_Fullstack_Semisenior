import { Routes } from '@angular/router';

import { UserListComponent } from './components/user-list/user-list.component';
import { TasksPageComponent } from './pages/tasks-page.component';

export const routes: Routes = [
  { path: '', redirectTo: 'tasks', pathMatch: 'full' },
  { path: 'tasks', component: TasksPageComponent },
  { path: 'users', component: UserListComponent }
];

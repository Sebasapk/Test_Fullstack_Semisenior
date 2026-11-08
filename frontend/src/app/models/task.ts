import { TaskAdditionalData } from './task-additional-data';
import { TaskStatus } from './task-status';

export interface Task {
  id: number;
  title: string;
  description: string | null;
  status: TaskStatus;
  userId: number;
  userName: string;
  createdAt: string;
  updatedAt: string;
  additionalData: TaskAdditionalData | null;
}

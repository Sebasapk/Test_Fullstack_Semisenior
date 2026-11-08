import { TaskAdditionalData } from './task-additional-data';

export interface CreateTaskRequest {
  title: string;
  description?: string;
  userId: number;
  additionalData?: TaskAdditionalData;
}

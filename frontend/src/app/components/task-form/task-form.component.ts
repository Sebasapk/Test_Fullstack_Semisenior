import { Component, OnInit, inject, output, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';

import { CreateTaskRequest } from '../../models/create-task-request';
import { TaskAdditionalData } from '../../models/task-additional-data';
import { User } from '../../models/user';
import { TasksService } from '../../services/tasks.service';
import { UsersService } from '../../services/users.service';

const MAX_TAGS = 10;
const MAX_TAG_LENGTH = 30;

function parseTags(value: string): string[] {
  return value.split(',').map((tag) => tag.trim()).filter(Boolean);
}

function tagsValidator(control: AbstractControl<string>): ValidationErrors | null {
  const tags = parseTags(control.value);
  if (tags.length > MAX_TAGS) {
    return { maxTags: true };
  }
  if (tags.some((tag) => tag.length > MAX_TAG_LENGTH)) {
    return { maxTagLength: true };
  }
  return null;
}

@Component({
  selector: 'app-task-form',
  imports: [ReactiveFormsModule],
  templateUrl: './task-form.component.html'
})
export class TaskFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly tasksService = inject(TasksService);
  private readonly usersService = inject(UsersService);

  readonly created = output<void>();

  protected readonly users = signal<User[]>([]);
  protected readonly maxTags = MAX_TAGS;
  protected readonly maxTagLength = MAX_TAG_LENGTH;
  protected readonly priorities = ['Low', 'Medium', 'High'];

  protected readonly form = this.fb.group({
    title: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(200)]),
    description: this.fb.nonNullable.control('', Validators.maxLength(1000)),
    userId: new FormControl<number | null>(null, Validators.required),
    priority: this.fb.nonNullable.control(''),
    estimatedEndDate: this.fb.nonNullable.control(''),
    tags: this.fb.nonNullable.control('', tagsValidator)
  });

  ngOnInit(): void {
    this.usersService.getAll().subscribe((users) => this.users.set(users));
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.tasksService.create(this.buildRequest()).subscribe(() => {
      this.form.reset();
      this.created.emit();
    });
  }

  private buildRequest(): CreateTaskRequest {
    const { title, description, userId, priority, estimatedEndDate, tags } = this.form.getRawValue();

    const additionalData: TaskAdditionalData = {};
    if (priority) {
      additionalData.priority = priority;
    }
    if (estimatedEndDate) {
      additionalData.estimatedEndDate = estimatedEndDate;
    }
    const tagList = parseTags(tags);
    if (tagList.length > 0) {
      additionalData.tags = tagList;
    }

    return {
      title,
      description: description || undefined,
      userId: userId!,
      additionalData: Object.keys(additionalData).length > 0 ? additionalData : undefined
    };
  }
}

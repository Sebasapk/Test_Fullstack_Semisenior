import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';

import { User } from '../../models/user';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'app-user-list',
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css'
})
export class UserListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly usersService = inject(UsersService);

  protected readonly users = signal<User[]>([]);

  // Only checks the users already loaded; the API still has the final say on uniqueness.
  private readonly uniqueEmail = (control: AbstractControl<string>): ValidationErrors | null => {
    const email = control.value.trim().toLowerCase();
    return this.users().some((user) => user.email.toLowerCase() === email)
      ? { duplicateEmail: true }
      : null;
  };

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(150), this.uniqueEmail]]
  });

  ngOnInit(): void {
    this.loadUsers();
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.usersService.create(this.form.getRawValue()).subscribe(() => {
      this.form.reset();
      this.loadUsers();
    });
  }

  private loadUsers(): void {
    this.usersService.getAll().subscribe((users) => this.users.set(users));
  }
}

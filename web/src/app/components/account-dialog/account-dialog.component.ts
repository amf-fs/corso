import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { Account } from '../../app.model';

@Component({
  selector: 'app-account-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    ReactiveFormsModule
  ],
  templateUrl: './account-dialog.component.html',
  styleUrl: './account-dialog.component.scss'
})
export class AccountDialogComponent {
  accountForm: FormGroup;
  showPassword = false;

  constructor(
    private readonly dialogRef: MatDialogRef<AccountDialogComponent>,
    private readonly formBuilder: FormBuilder
  ) {
    this.accountForm = this.formBuilder.group({
      accountName: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  get accountNameControl() {
    return this.accountForm.get('accountName');
  }

  get usernameControl() {
    return this.accountForm.get('username');
  }

  get passwordControl() {
    return this.accountForm.get('password');
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.accountForm.invalid) {
      this.accountForm.markAllAsTouched();
      return;
    }

    const newAccount: Account = {
      name: this.accountNameControl?.value as string,
      username: this.usernameControl?.value as string,
      password: this.passwordControl?.value as string
    };

    this.dialogRef.close(newAccount);
  }
}

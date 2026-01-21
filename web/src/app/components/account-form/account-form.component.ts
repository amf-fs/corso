import { Component, effect, EventEmitter, input, Output, signal, inject, DestroyRef } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Account } from '../../app.model';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../services/account.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
    selector: 'app-account-form',
    standalone: true,
    imports: [
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        MatSnackBarModule,
        ReactiveFormsModule,
        FormsModule,
        CommonModule
    ],
    templateUrl: './account-form.component.html',
    styleUrl: './account-form.component.scss'
})
export class AccountFormComponent {
    @Output() accountCreated = new EventEmitter<Account>();
    @Output() accountUpdated = new EventEmitter<Account>();
    @Output() newAccountClicked = new EventEmitter<void>();
    @Output() accountsImported = new EventEmitter<void>();

    get accountNameControl() {
      return this.accountForm.get('accountName');
    }

    get usernameControl() {
      return this.accountForm.get('username');
    }

    get passwordControl() {
      return this.accountForm.get('password');
    }

    private _showPassword = false;
    get showPassword() : boolean{
      return this._showPassword;
    }

    private selectedFile = signal<File | null>(null);
    get fileName() : string | undefined {
      return this.selectedFile()?.name;
    }

    account = input<Account | null>(null);
    accountForm: FormGroup

    private readonly destroyRef = inject(DestroyRef);

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly accountService: AccountService,
        private readonly snackBar: MatSnackBar
    ) {
        this.accountForm = this.formBuilder.group({
            accountName: ['', Validators.required],
            username: ['', Validators.required],
            password: ['', Validators.required],
        })

        this.watchAccountChanges();
    }

    onDeleteButtonClick(): void {
      this.selectedFile.set(null);
    }

    onFileButtonClick(): void {
      const file = this.selectedFile();

      // If file already selected, import it
      if (file) {
        this.accountService.import(file)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.accountsImported.emit();
              this.selectedFile.set(null);
            }
          });
      } else {
        // If no file selected, open file picker
        const fileInput = document.getElementById('file') as HTMLInputElement;
        fileInput?.click();
      }
    }

    onSaveButtonClick(): void {
        const existingAccount = this.account();

        const formValues: Account = {
            name: this.accountNameControl?.value as string,
            username: this.usernameControl?.value as string,
            password: this.passwordControl?.value as string
        };

        if (existingAccount) {
            formValues.id = existingAccount.id;
            this.accountUpdated.emit(formValues)

        } else {
            this.accountCreated.emit(formValues);
        }

        this.accountForm.reset();
    }

    onNewAccountButtonClick() {
        this.accountForm.reset();
        this.newAccountClicked.emit();
    }

    onPasswordVisibilityButtonClick(): void {
      this._showPassword = !this._showPassword;
    }

    onFileSelected(event: Event): void {
      const fileInput = event.target as HTMLInputElement;

      if(fileInput.files && fileInput.files.length > 0) {
        this.selectedFile.set(fileInput.files[0]);
      }
    }

    isFormInvalid(): boolean {
        return this.accountForm.invalid
    }

    private watchAccountChanges() {
        effect(() => {
            const accountValue = this.account();
            if (accountValue) {
                this.accountForm.patchValue({
                    accountName: accountValue.name,
                    username: accountValue.username,
                    password: accountValue.password
                });
            } else {
                this.accountForm.reset();
            }
        });
    }
}

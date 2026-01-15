import { Component, effect, EventEmitter, input, Output } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Account } from '../../app.model';

@Component({
    selector: 'app-account-form',
    standalone: true,
    imports: [
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        ReactiveFormsModule,
        FormsModule,
    ],
    templateUrl: './account-form.component.html',
    styleUrl: './account-form.component.scss'
})
export class AccountFormComponent {
    @Output() accountCreated = new EventEmitter<Account>();
    @Output() accountUpdated = new EventEmitter<Account>();
    @Output() newAccountClicked = new EventEmitter<void>();

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

    private _fileName = '';
    get fileName() : string {
      return this._fileName;
    }

    account = input<Account | null>(null);
    accountForm: FormGroup


    constructor(private readonly formBuilder: FormBuilder) {
        this.accountForm = this.formBuilder.group({
            accountName: ['', Validators.required],
            username: ['', Validators.required],
            password: ['', Validators.required],
        })

        this.watchAccountChanges();
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
        this._fileName = fileInput.files[0].name;
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

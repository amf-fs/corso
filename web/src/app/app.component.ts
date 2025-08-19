import { Component, computed, signal, ViewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Account } from './app.model';
import { MatListModule, MatSelectionList } from '@angular/material/list';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    ReactiveFormsModule,
    FormsModule,
    MatListModule],
  providers: [
    { provide: MAT_FORM_FIELD_DEFAULT_OPTIONS, useValue: { appearance: 'outline' } }
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  @ViewChild(MatSelectionList) selectionList!: MatSelectionList;

  accountForm: FormGroup
  filteredAccounts = computed(() => (this.filterBySearchTerm()));
  selectedAccounts: Account[] = [];

  get accountNameControl() {
    return this.accountForm.get('accountName');
  }

  get usernameControl() {
    return this.accountForm.get('username');
  }

  get passwordControl() {
    return this.accountForm.get('password');
  }

  private readonly searchTerm = signal<string>('');
  private readonly allAccounts = signal<Account[]>([]);

  constructor(private readonly formBuilder: FormBuilder) {
    this.accountForm = this.formBuilder.group({
      accountName: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required],
    })
  }

  onSaveButtonClick(): void {
    const formValues: Account = {
      id: this.selectedAccounts.length > 0 ? this.selectedAccounts[0].id : Math.floor(Math.random() * 1000) + 1,
      name: this.accountNameControl?.value as string,
      username: this.usernameControl?.value as string,
      password: this.passwordControl?.value as string
    };

    if (this.selectedAccounts.length > 0) {
      // Update existing account
      this.allAccounts.update(accounts =>
        accounts.map(account =>
          account.id === this.selectedAccounts[0].id ? formValues : account
        )
      );
    } else {
      // Create new account
      this.allAccounts.update(accounts => [...accounts, formValues]);
    }

    this.reset();
  }

  onSearchInput({ target }: Event): void {
    this.searchTerm.set((target as HTMLInputElement).value);
  }

  onNewAccountButtonClick() {
    this.reset();
  }

  onAccountListSelectionChange() {
    if (this.selectedAccounts) {
      const account = this.selectedAccounts[0];
      this.accountForm.patchValue({
        accountName: account.name,
        username: account.username,
        password: account.password
      });
    }
  }

  filterBySearchTerm(): Account[] {
    const search = this.searchTerm().toLowerCase().trim();

    if (!search) {
      return this.allAccounts();
    }

    return this.allAccounts().filter(account =>
      account.name.toLowerCase().includes(search));
  }

  isFormInvalid(): boolean {
    return this.accountForm.invalid
  }

  private reset() {
    this.accountForm.reset();
    this.selectedAccounts = [];
    this.selectionList?.deselectAll();
  }
}



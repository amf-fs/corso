import { Component, computed, signal, ViewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Account } from './app.model';
import { MatListModule, MatSelectionList } from '@angular/material/list';
import { AccountFormComponent } from "./components/account-form/account-form.component";
import { AccountListComponent } from './components/account-list/account-list.component';

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
    MatListModule,
    AccountFormComponent,
    AccountListComponent
  ],
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
  allAccounts = signal<Account[]>([]);

  private _selectedAccount: Account | null = null;
  get selectedAccount(): Account | null {
    return this._selectedAccount;
  }

  private readonly searchTerm = signal<string>('');

  constructor(private readonly formBuilder: FormBuilder) {
    this.accountForm = this.formBuilder.group({
      accountName: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required],
    })
  }

  onAccountCreated(newAccount: Account) {
    this.allAccounts.update(accounts => [...accounts, newAccount]);
    this._selectedAccount = null;
    this.selectionList?.deselectAll();
  }

  onAccountUpdated(updatedAccount: Account) {
    this.allAccounts.update(accounts =>
      accounts.map(account =>
        account.id === this.selectedAccount?.id ? updatedAccount : account
      )
    );

    this.selectionList?.deselectAll();
    this._selectedAccount = null;
  }

  onNewAccountClicked() {
    this.selectionList?.deselectAll();
    this.selectedAccounts = [];
    this._selectedAccount = null;
  }

  onSearchInput({ target }: Event): void {
    this.searchTerm.set((target as HTMLInputElement).value);
  }

  //TODO: deprecated
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

  onAccountSelectionChange(selectedAccount: Account) {
    this._selectedAccount = selectedAccount
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
}



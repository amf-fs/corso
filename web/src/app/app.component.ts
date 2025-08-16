import { Component, computed, signal, ViewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Account } from './app.model';
import { MatListModule, MatSelectionList, MatSelectionListChange } from '@angular/material/list';

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
    MatListModule],
  providers: [
    { provide: MAT_FORM_FIELD_DEFAULT_OPTIONS, useValue: { appearance: 'outline' } }
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  @ViewChild(MatSelectionList) selectionList!: MatSelectionList;

  accountNameControl = new FormControl('', Validators.required);
  usernameControl = new FormControl('', [Validators.required]);
  passwordControl = new FormControl('', [Validators.required]);
  filteredAccounts = computed(() => (this.filterBySearchTerm()));

  private readonly searchTerm = signal<string>('');
  private readonly allAccounts = signal<Account[]>([]);
  private selectedAccount: Account | null = null;


  onSaveButtonClick(): void {
    const account: Account = {
      name: this.accountNameControl.value as string,
      username: this.usernameControl.value as string,
      password: this.passwordControl.value as string
    }

    this.allAccounts.update(accounts => [...accounts, account]);

    this.resetForm();

  }

  onSearchInput({ target }: Event): void {
    this.searchTerm.set((target as HTMLInputElement).value);
  }

  onListOptionClick(event: MatSelectionListChange) {
    this.selectedAccount = event.options[0]?.value as Account;

    if (this.selectedAccount) {
      this.accountNameControl.setValue(this.selectedAccount.name);
      this.usernameControl.setValue(this.selectedAccount.username);
      this.passwordControl.setValue(this.selectedAccount.password);
    }
  }

  onNewAccountButtonClick() {
    this.resetForm();
    this.selectionList?.deselectAll();
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
    return this.accountNameControl.invalid ||
      this.usernameControl.invalid ||
      this.passwordControl.invalid
  }

  private resetForm() {
    this.accountNameControl.reset();
    this.usernameControl.reset();
    this.passwordControl.reset();
  }
}



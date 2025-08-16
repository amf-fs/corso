import { Component, computed, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Account } from './app.model';
import { MatListModule, MatSelectionListChange } from '@angular/material/list';

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
  accountNameControl = new FormControl('', Validators.required);
  usernameControl = new FormControl('', [Validators.required]);
  passwordControl = new FormControl('', [Validators.required]);
  filteredAccounts = computed(() => (this.filterBySearchTerm()));

  private readonly searchTerm = signal<string>('');
  private readonly allAccounts = signal<Account[]>([]);


  onSaveButtonClick(): void {
    const account: Account = {
      name: this.accountNameControl.value as string,
      username: this.usernameControl.value as string,
      password: this.passwordControl.value as string
    }

    this.allAccounts.update(accounts => [...accounts, account]);

    this.accountNameControl.reset();
    this.usernameControl.reset();
    this.passwordControl.reset();
  }

  onSearchInput({ target }: Event): void {
    this.searchTerm.set((target as HTMLInputElement).value);
  }

  onListOptionClick(event: MatSelectionListChange) {
    const selectedAccount = event.options[0]?.value as Account;
    
    if (selectedAccount) {
      this.accountNameControl.setValue(selectedAccount.name);
      this.usernameControl.setValue(selectedAccount.username);
      this.passwordControl.setValue(selectedAccount.password);
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
}



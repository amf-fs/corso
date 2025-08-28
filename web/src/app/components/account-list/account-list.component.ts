import { Component, computed } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Account } from '../../app.model';
import { MatListModule } from '@angular/material/list';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-account-list',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatListModule,
    FormsModule
  ],
  templateUrl: './account-list.component.html',
  styleUrl: './account-list.component.scss'
})
export class AccountListComponent {
  filteredAccounts = computed(() => (this.filterBySearchTerm()));
  selectedAccounts: Account[] = [];
  
  onSearchInput({ target }: Event): void {
    //this.searchTerm.set((target as HTMLInputElement).value);
  }

  onAccountListSelectionChange() {
    // if (this.selectedAccounts) {
    //   const account = this.selectedAccounts[0];
    //   this.accountForm.patchValue({
    //     accountName: account.name,
    //     username: account.username,
    //     password: account.password
    //   });
    // }
  }

  private filterBySearchTerm(): Account[] {
    return [];
    // const search = this.searchTerm().toLowerCase().trim();

    // if (!search) {
    //   return this.allAccounts();
    // }

    // return this.allAccounts().filter(account =>
    //   account.name.toLowerCase().includes(search));
  }
}

import { Component, computed, EventEmitter, input, Output, signal, ViewChild } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Account } from '../../app.model';
import { MatListModule, MatSelectionList } from '@angular/material/list';
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
  @ViewChild(MatSelectionList) selectionList!: MatSelectionList;
  @Output() accountSelectionChanged = new EventEmitter<Account>();

  filteredAccounts = computed(() => (this.filterBySearchTerm()));
  selectedAccounts: Account[] = [];
  accounts = input<Account[]>([])

  private readonly searchTerm = signal<string>('');
  
  onSearchInput({ target }: Event): void {
    this.searchTerm.set((target as HTMLInputElement).value);
  }

  onAccountListSelectionChange(): void {
    this.accountSelectionChanged.emit(this.selectedAccounts[0]);
  }

  clearSelection(): void {
    this.selectionList.deselectAll();
  }

  private filterBySearchTerm(): Account[] {
    const search = this.searchTerm().toLowerCase().trim();

    if (!search) {
      return this.accounts();
    }

    return this.accounts().filter(account =>
      account.name.toLowerCase().includes(search));
  }
}

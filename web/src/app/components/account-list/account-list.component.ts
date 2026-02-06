import { Component, computed, DestroyRef, EventEmitter, inject, input, Output, signal } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Account } from '../../app.model';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { AccountDialogComponent } from '../account-dialog/account-dialog.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-account-list',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatListModule,
    MatDividerModule,
    MatDialogModule,
    MatButtonModule
  ],
  templateUrl: './account-list.component.html',
  styleUrl: './account-list.component.scss'
})
export class AccountListComponent {
  @Output() accountUpdated = new EventEmitter<Account>();

  filteredAccounts = computed(() => (this.filterBySearchTerm()));
  accounts = input<Account[]>([])

  private readonly searchTerm = signal<string>('');
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  onSearchInput({ target }: Event): void {
    this.searchTerm.set((target as HTMLInputElement).value);
  }

  onEditButtonClick(account: Account, event: Event): void {
    event.stopPropagation();

    const dialogRef = this.dialog.open(AccountDialogComponent, {
      width: '90vw',
      maxWidth: '480px',
      disableClose: true,
      data: account
    });

    dialogRef.afterClosed()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter((result): result is Account => !!result)
      )
      .subscribe(updatedAccount => this.accountUpdated.emit(updatedAccount));
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

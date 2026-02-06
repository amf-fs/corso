import { Component, DestroyRef, inject, signal } from '@angular/core';
import { Account } from '../../app.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AccountService } from '../../services/account.service';
import { AccountListComponent } from '../account-list/account-list.component';
import { ActionBarComponent } from '../action-bar/action-bar.component';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [
    MatListModule,
    MatDividerModule,
    CommonModule,
    ActionBarComponent,
    AccountListComponent
  ],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss'
})
export class AccountsComponent {
  allAccounts = signal<Account[]>([]);

  private readonly destroyRef = inject(DestroyRef);

  constructor(
    private readonly accountService: AccountService
  ) {
  }

  ngOnInit(): void {
    this.loadAccounts();
  }

  onAccountCreated(newAccount: Account) {
    this.accountService.add(newAccount)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(createdAccount => {
        this.allAccounts.update(accounts => [...accounts, createdAccount]);
      });
  }

  onAccountUpdated(updatedAccount: Account) {
    this.accountService.update(updatedAccount)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({complete: () => {
        this.allAccounts.update(accounts =>
          accounts.map(account =>
            account.id === updatedAccount.id ? updatedAccount : account
          )
        );
      }})
  }

  onNewAccountCreated(newAccount: Account): void {
    this.accountService.add(newAccount)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(createdAccount => {
        this.allAccounts.update(accounts => [...accounts, createdAccount]);
      });
  }

  onFileImported(): void {
    this.loadAccounts();
  }

  private loadAccounts(): void {
    this.accountService.getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(accounts => {
        this.allAccounts.set(accounts);
      })
  }
}

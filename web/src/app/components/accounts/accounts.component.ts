import { Component, DestroyRef, inject, signal } from '@angular/core';
import { Account } from '../../app.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AccountService } from '../../services/account.service';
import { AccountListComponent } from '../account-list/account-list.component';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs';
import { AccountDialogComponent } from '../account-dialog/account-dialog.component';

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatDividerModule,
    MatDialogModule,
    CommonModule,
    AccountListComponent
  ],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss'
})
export class AccountsComponent {
  allAccounts = signal<Account[]>([]);
  selectedFile = signal<File | null>(null);

  get fileName(): string | undefined {
    return this.selectedFile()?.name;
  }

  private readonly destroyRef = inject(DestroyRef);

  constructor(
    private readonly accountService: AccountService,
    private readonly dialog: MatDialog
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

  openNewAccountDialog(): void {
    const dialogRef = this.dialog.open(AccountDialogComponent, {
      width: '480px',
      disableClose: true
    });

    dialogRef.afterClosed()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter((result): result is Account => !!result)
      )
      .subscribe(account => {
        if (!account.id) {
          this.onAccountCreated(account);
        } else {
          this.onAccountUpdated(account);
        }
      });
  }

  onFileButtonClick(): void {
    const file = this.selectedFile();

    // If file already selected, import it
    if (file) {
      this.accountService.import(file)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.loadAccounts();
            this.selectedFile.set(null);
          }
        });
    } else {
      // If no file selected, open file picker
      const fileInput = document.getElementById('file') as HTMLInputElement;
      fileInput?.click();
    }
  }

  onDeleteButtonClick(): void {
    this.selectedFile.set(null);
    // Reset file input so same file can be selected again
    const fileInput = document.getElementById('file') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  onFileSelected(event: Event): void {
    const fileInput = event.target as HTMLInputElement;

    if(fileInput.files && fileInput.files.length > 0) {
      this.selectedFile.set(fileInput.files[0]);
    }
  }

  private loadAccounts(): void {
    this.accountService.getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(accounts => {
        this.allAccounts.set(accounts);
      })
  }
}

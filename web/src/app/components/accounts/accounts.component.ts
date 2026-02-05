import { Component, DestroyRef, inject, signal } from '@angular/core';
import { Account } from '../../app.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../../services/account.service';
import { AccountListComponent } from '../account-list/account-list.component';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs';
import { AccountFormComponent } from '../account-form/account-form.component';
import { AccountDialogComponent } from '../account-dialog/account-dialog.component';

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    ReactiveFormsModule,
    FormsModule,
    MatListModule,
    MatDividerModule,
    MatDialogModule,
    CommonModule,
    AccountFormComponent,
    AccountListComponent
  ],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss'
})
export class AccountsComponent {
  accountForm: FormGroup
  allAccounts = signal<Account[]>([]);
  selectedFile = signal<File | null>(null);

  private _selectedAccount: Account | null = null;

  get selectedAccount(): Account | null {
    return this._selectedAccount;
  }

  get fileName(): string | undefined {
    return this.selectedFile()?.name;
  }

  private readonly destroyRef = inject(DestroyRef);

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly accountService: AccountService,
    private readonly dialog: MatDialog
  ) {
    this.accountForm = this.formBuilder.group({
      accountName: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required],
    })
  }

  ngOnInit(): void {
    this.loadAccounts();
  }

  onAccountCreated(fromForm: Account) {
    this.accountService.add(fromForm)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(newAccount => {
        this.allAccounts.update(accounts => [...accounts, newAccount]);
        this._selectedAccount = null;
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

        this._selectedAccount = null;
      }})
  }

  onNewAccountClicked() {
    this._selectedAccount = null;
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
      .subscribe(newAccount => this.onAccountCreated(newAccount));
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

  onAccountSelectionChange(selectedAccount: Account) {
    this._selectedAccount = selectedAccount
  }

  private loadAccounts(): void {
    this.accountService.getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(accounts => {
        this.allAccounts.set(accounts);
      })
  }
}

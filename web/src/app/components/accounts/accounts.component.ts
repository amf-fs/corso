import { Component, DestroyRef, inject, signal, ViewChild } from '@angular/core';
import { Account } from '../../app.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../../services/account.service';
import { AccountListComponent } from '../account-list/account-list.component';
import { MatButtonModule } from '@angular/material/button';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { AccountFormComponent } from '../account-form/account-form.component';

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
    AccountFormComponent,
    AccountListComponent
  ],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss'
})
export class AccountsComponent {
  @ViewChild(AccountListComponent) accountList!: AccountListComponent;

  accountForm: FormGroup
  allAccounts = signal<Account[]>([]);

  private _selectedAccount: Account | null = null;

  get selectedAccount(): Account | null {
    return this._selectedAccount;
  }

  private readonly destroyRef = inject(DestroyRef);

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly accountService: AccountService
  ) {
    this.accountForm = this.formBuilder.group({
      accountName: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required],
    })
  }

  ngOnInit(): void {
    this.accountService.getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(accounts => {
        this.allAccounts.set(accounts);
      })
  }

  onAccountCreated(fromForm: Account) {
    this.accountService.add(fromForm)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(newAccount => {
        this.allAccounts.update(accounts => [...accounts, newAccount]);
        this._selectedAccount = null;
        this.accountList.clearSelection();
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

        this.accountList.clearSelection();
        this._selectedAccount = null;
      }})
  }

  onNewAccountClicked() {
    this.accountList.clearSelection();
    this._selectedAccount = null;
  }

  onAccountSelectionChange(selectedAccount: Account) {
    this._selectedAccount = selectedAccount
  }
}

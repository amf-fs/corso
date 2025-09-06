import { Component, DestroyRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Account } from './app.model';
import { MatListModule } from '@angular/material/list';
import { AccountFormComponent } from "./components/account-form/account-form.component";
import { AccountListComponent } from './components/account-list/account-list.component';
import { AccountService } from './services/account.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

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
export class AppComponent implements OnInit {
  @ViewChild(AccountListComponent) accountList!: AccountListComponent;

  accountForm: FormGroup
  allAccounts = signal<Account[]>([]);

  private _selectedAccount: Account | null = null;
  get selectedAccount(): Account | null {
    return this._selectedAccount;
  }

  private readonly destroyRef = inject(DestroyRef);

  constructor(private readonly formBuilder: FormBuilder, private readonly accountService: AccountService) {
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

  onAccountCreated(newAccount: Account) {
    this.allAccounts.update(accounts => [...accounts, newAccount]);
    this._selectedAccount = null;
    this.accountList.clearSelection();
  }

  onAccountUpdated(updatedAccount: Account) {
    this.allAccounts.update(accounts =>
      accounts.map(account =>
        account.id === this.selectedAccount?.id ? updatedAccount : account
      )
    );

    this.accountList.clearSelection();
    this._selectedAccount = null;
  }

  onNewAccountClicked() {
    this.accountList.clearSelection();
    this._selectedAccount = null;
  }

  onAccountSelectionChange(selectedAccount: Account) {
    this._selectedAccount = selectedAccount
  }
}



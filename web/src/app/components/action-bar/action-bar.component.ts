import { Component, DestroyRef, EventEmitter, inject, Output, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../services/account.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { filter } from 'rxjs';
import { AccountDialogComponent } from '../account-dialog/account-dialog.component';
import { Account } from '../../app.model';

@Component({
  selector: 'app-action-bar',
  standalone: true,
  imports: [
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    CommonModule
  ],
  templateUrl: './action-bar.component.html',
  styleUrl: './action-bar.component.scss'
})
export class ActionBarComponent {
  @Output() newAccountCreated = new EventEmitter<Account>();
  @Output() fileImported = new EventEmitter<void>();

  selectedFile = signal<File | null>(null);
  private readonly accountService = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  get fileName(): string | undefined {
    return this.selectedFile()?.name;
  }

  onNewAccountClick(): void {
    const dialogRef = this.dialog.open(AccountDialogComponent, {
      width: '90vw',
      maxWidth: '480px',
      disableClose: true
    });

    dialogRef.afterClosed()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter((result): result is Account => !!result)
      )
      .subscribe(newAccount => {
        this.newAccountCreated.emit(newAccount);
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
            this.selectedFile.set(null);
            this.fileImported.emit();
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
}

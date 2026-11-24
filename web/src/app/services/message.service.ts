import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class MessageService {
  constructor(private readonly snackBar: MatSnackBar) {}

  success(message: string): void {
    this.show(message);
  }

  error(message = 'Oops, an error has occurred!'): void {
    this.show(message);
  }

  private show(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 3000, verticalPosition: 'top' });
  }
}

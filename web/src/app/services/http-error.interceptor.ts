import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { EMPTY } from 'rxjs';

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unknown error occurred. Please try again later.';

      if(error.status === 401){
        errorMessage = 'Closing the vault time is over...';

        if(router.url === '/master-password'){
          errorMessage = 'Invalid master password. Please try again.';
        }

        router.navigate(['/master-password']);
      }

      snackBar.open(errorMessage, 'Close', { duration: 3000, verticalPosition: 'top' });

      return EMPTY;
    })
  );
};

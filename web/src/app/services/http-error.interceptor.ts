import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { EMPTY } from 'rxjs';
import { MessageService } from './message.service';

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const messageService = inject(MessageService);
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

      messageService.error(errorMessage);

      return EMPTY;
    })
  );
};

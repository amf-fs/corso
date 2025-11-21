import { HttpClient } from '@angular/common/http';
import { Injectable, Signal, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { EMPTY, Observable, switchMap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {
  private readonly _apiUrl = `${environment.corsoApiBasePath}/authorizations`;

  private _isAuthenticated = signal<boolean>(false);
  get isAuthenticated(): Signal<boolean> {
    return this._isAuthenticated.asReadonly();
  }

  constructor(private readonly httpClient: HttpClient) { }

  authenticate(masterPassword: string): Observable<never> {
    return this.httpClient.post(this._apiUrl, { masterPassword })
      .pipe(switchMap(() => {
        this._isAuthenticated.set(true);
        return EMPTY;
      }));
  }
}

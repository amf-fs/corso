import { HttpClient } from '@angular/common/http';
import { Injectable, Signal, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { EMPTY, Observable, of, switchMap } from 'rxjs';

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

  authenticate(masterPassword: string): Observable<boolean> {
    return this.httpClient.post(this._apiUrl, { masterPassword }, { withCredentials: true })
      .pipe(switchMap(() => {
        this._isAuthenticated.set(true);
        return of(true);
      }));
  }
}

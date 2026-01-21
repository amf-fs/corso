import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EMPTY, Observable, switchMap } from 'rxjs';
import { Account } from '../app.model';
import { environment } from '../../environments/environment';

@Injectable({providedIn: 'root'})
export class AccountService {
  private readonly apiUrl = `${environment.corsoApiBasePath}/accounts`;

  constructor(private readonly httpClient: HttpClient) {

  }

  getAll(): Observable<Account[]>{
    return this.httpClient.get<Account[]>(this.apiUrl, {withCredentials: true});
  }

  add(newAccount: Account): Observable<Account> {
    return this.httpClient.post<Account>(this.apiUrl, newAccount, {withCredentials: true});
  }

  update(toUpdate: Account): Observable<void> {
    return this.httpClient.put<void>(`${this.apiUrl}/${toUpdate.id}`, toUpdate, {withCredentials: true})
  }

  import(file: File): Observable<void> {
    const formData = new FormData();
    formData.append('file', file);
    return this.httpClient.post<void>(`${this.apiUrl}/import`, formData, {withCredentials: true});
  }
}

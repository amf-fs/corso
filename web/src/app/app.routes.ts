import { Routes } from '@angular/router';
import { MasterPasswordComponent } from './components/master-password/master-password.component';
import { authorizationGuard } from './services/authorization.guard';
import { AccountsComponent } from './components/accounts/accounts.component';

export const routes: Routes = [
  {path: '', component: AccountsComponent, canActivate: [authorizationGuard]},
  {path: 'master-password', component: MasterPasswordComponent},
  {path: '**', redirectTo: ''} //TODO: Handle unknown routes later
];

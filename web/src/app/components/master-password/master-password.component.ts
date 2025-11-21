import { Component } from '@angular/core';
import { AuthorizationService } from '../../services/authorization.service';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-master-password',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,],
  templateUrl: './master-password.component.html',
  styleUrl: './master-password.component.scss'
})
export class MasterPasswordComponent {
  masterPassword: string = '';

  constructor(private readonly authorizationService: AuthorizationService) { }

  onSubmit(form: any): void {
    this.authorizationService.authenticate(this.masterPassword).subscribe();
  }
}

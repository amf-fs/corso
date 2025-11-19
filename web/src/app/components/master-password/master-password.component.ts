import { Component, DestroyRef } from '@angular/core';
import { AuthorizationService } from '../../services/authorization.service';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

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

  constructor(
    private readonly authorizationService: AuthorizationService,
    private readonly router: Router,
    private readonly destroyRef: DestroyRef
  ) { }

  onSubmit(): void {
    this.authorizationService.authenticate(this.masterPassword)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(isAuthenticated => {
        if(isAuthenticated)
          this.router.navigate(['/']);
      });
  }
}

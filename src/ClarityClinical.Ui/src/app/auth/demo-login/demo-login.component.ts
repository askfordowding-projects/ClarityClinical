import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DemoRole, DemoSessionService } from '../../core/auth/demo-session.service';

@Component({
  selector: 'app-demo-login',
  standalone: true,
  templateUrl: './demo-login.component.html',
  styleUrl: './demo-login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DemoLoginComponent {
  private readonly session = inject(DemoSessionService);
  private readonly router = inject(Router);

  protected readonly isSigningIn = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected enterAs(role: DemoRole): void {
    if (this.isSigningIn()) {
      return;
    }

    this.isSigningIn.set(true);
    this.errorMessage.set(null);

    this.session.login(role).subscribe({
      next: () => {
        const destination = role === 'Clinician'
          ? '/clinical/consultations'
          : '/admin';
        void this.router.navigateByUrl(destination);
      },
      error: () => {
        this.errorMessage.set('The demonstration session could not be started. Please try again.');
        this.isSigningIn.set(false);
      }
    });
  }
}

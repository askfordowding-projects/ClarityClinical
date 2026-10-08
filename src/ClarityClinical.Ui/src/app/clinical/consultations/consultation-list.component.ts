import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DemoSessionService } from '../../core/auth/demo-session.service';

@Component({
  selector: 'app-consultation-list',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main class="landing-shell">
      <section class="landing-card">
        <p class="landing-card__eyebrow">Clinician demonstration</p>
        <h1>Clarity Clinical</h1>
        <div class="demo-warning" role="note">
          <strong>SYNTHETIC DATA ONLY</strong>
          <span>This demonstration is not for clinical use.</span>
        </div>
        <article class="scenario-card">
          <div>
            <span class="scenario-card__setting">Urgent Treatment Centre</span>
            <h2>Miguel Santos</h2>
            <p>58 years · left calf swelling · ankle wound · Spanish → English consultation</p>
          </div>
          <button
            type="button"
            data-action="start-miguel"
            [disabled]="isStarting()"
            (click)="startMiguel()">
            {{ isStarting() ? 'Starting consultation…' : 'Start Miguel consultation' }}
          </button>
        </article>
        @if (errorMessage()) {
          <p class="error" role="alert">{{ errorMessage() }}</p>
        }
        <p class="journey-note">This creates an isolated visitor session and opens the Figure 1 consultation workspace.</p>
        <a class="secondary-link" [routerLink]="['/login']">Return to demo entry</a>
      </section>
    </main>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; }
    .landing-shell { min-height: 100vh; display: grid; place-items: center; padding: 2rem; background: #eef4f3; color: #172624; }
    .landing-card { width: min(48rem, 100%); padding: 2rem; border: 1px solid #cad9d6; border-radius: 1rem; background: #fff; }
    .landing-card__eyebrow { margin: 0; color: #4f726d; font-size: .75rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; }
    h1 { margin: .25rem 0 1rem; }
    .demo-warning { display: flex; flex-wrap: wrap; justify-content: space-between; gap: .5rem 1rem; padding: .7rem .8rem; border-left: .25rem solid #a56a28; background: #fff8ed; color: #6f4b20; font-size: .78rem; }
    .scenario-card { display: flex; justify-content: space-between; align-items: center; gap: 1.2rem; margin: 1rem 0; padding: 1.2rem; border: 1px solid #cbd9d6; border-radius: .8rem; background: #f8fbfa; }
    .scenario-card h2, .scenario-card p { margin: 0; }
    .scenario-card p { margin-top: .35rem; color: #5d716d; }
    .scenario-card__setting { color: #4f726d; font-size: .72rem; font-weight: 700; text-transform: uppercase; letter-spacing: .06em; }
    button { flex: 0 0 auto; padding: .65rem .85rem; border: 1px solid #356e67; border-radius: .55rem; background: #356e67; color: #fff; font: inherit; font-weight: 700; cursor: pointer; }
    button:disabled { cursor: wait; opacity: .65; }
    .error { padding: .65rem .75rem; border-radius: .5rem; background: #fff3ef; color: #7b4135; }
    .journey-note { color: #657874; font-size: .78rem; }
    .secondary-link { color: #2d655e; font-weight: 700; }
    @media (max-width: 40rem) { .scenario-card { display: grid; } button { width: 100%; } }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConsultationListComponent {
  private readonly session = inject(DemoSessionService);
  private readonly router = inject(Router);
  protected readonly isStarting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected startMiguel(): void {
    if (this.isStarting()) return;
    this.isStarting.set(true);
    this.errorMessage.set(null);
    this.session.startScenario('miguel-santos-leg-swelling').subscribe({
      next: consultationId => void this.router.navigateByUrl(`/clinical/consultations/${consultationId}`),
      error: () => {
        this.errorMessage.set('The Miguel demonstration could not be started. Please try again.');
        this.isStarting.set(false);
      }
    });
  }
}

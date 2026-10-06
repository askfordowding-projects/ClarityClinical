import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-consultation-list',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main class="landing-shell">
      <section class="landing-card">
        <p class="landing-card__eyebrow">Clinician demonstration</p>
        <h1>Consultations</h1>
        <p>The Miguel Santos synthetic consultation is the first executable scenario.</p>
        <a class="primary-link" [routerLink]="['/login']">Return to demo entry</a>
      </section>
    </main>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; }
    .landing-shell { min-height: 100vh; display: grid; place-items: center; padding: 2rem; background: #eef4f3; }
    .landing-card { width: min(42rem, 100%); padding: 2rem; border: 1px solid #cad9d6; border-radius: 1rem; background: #fff; }
    .landing-card__eyebrow { color: #4f726d; font-size: .75rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; }
    .primary-link { display: inline-block; margin-top: 1rem; color: #2d655e; font-weight: 700; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConsultationListComponent {}

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main class="admin-shell">
      <header class="admin-header">
        <div>
          <p>Administration</p>
          <h1>Clarity Clinical</h1>
        </div>
        <span>DEMONSTRATION SYSTEM · SYNTHETIC DATA ONLY</span>
      </header>
      <section class="admin-grid" aria-label="Administration areas">
        <article><strong>Synthetic patients</strong><span>Miguel Santos and future demo records</span></article>
        <article><strong>Scenarios</strong><span>Controlled consultation demonstrations</span></article>
        <article><strong>Clinical rules</strong><span>Versioned deterministic reasoning</span></article>
        <article><strong>Audit</strong><span>Trace clinical facts to assessment changes</span></article>
      </section>
      <a [routerLink]="['/login']">Return to demo entry</a>
    </main>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; }
    .admin-shell { min-height: 100vh; padding: 2rem; background: #edf3f2; color: #172624; }
    .admin-header { display: flex; justify-content: space-between; align-items: start; gap: 1rem; padding: 1.25rem; border-radius: .9rem; background: #183d39; color: #fff; }
    .admin-header p, .admin-header h1 { margin: 0; }
    .admin-header p { color: #c8d9d6; font-size: .72rem; text-transform: uppercase; letter-spacing: .08em; }
    .admin-header span { font-size: .72rem; color: #ffe9ad; }
    .admin-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 1rem; margin: 1rem 0; }
    .admin-grid article { display: grid; gap: .35rem; padding: 1.25rem; border: 1px solid #cbd9d6; border-radius: .8rem; background: #fff; }
    .admin-grid span { color: #5d716d; }
    a { color: #2d655e; font-weight: 700; }
    @media (max-width: 44rem) { .admin-grid { grid-template-columns: 1fr; } .admin-header { display: grid; } }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminDashboardComponent {}

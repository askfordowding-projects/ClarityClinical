import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DemoAdminService } from './demo-admin.service';
import { DemoScenario } from './demo-admin.models';
@Component({ selector: 'app-admin-scenarios', standalone: true, imports: [RouterLink], templateUrl: './scenarios.component.html', styleUrl: '../governance/governance-page.scss', changeDetection: ChangeDetectionStrategy.OnPush })
export class ScenariosComponent implements OnInit {
  private readonly service = inject(DemoAdminService); protected readonly scenarios = signal<DemoScenario[]>([]); protected readonly error = signal<string | null>(null);
  ngOnInit(): void { this.service.getScenarios().subscribe({ next: v => this.scenarios.set(v), error: () => this.error.set('Demo scenarios could not be loaded.') }); }
}

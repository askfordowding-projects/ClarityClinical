import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminGovernanceService } from './admin-governance.service';
import { ClinicalRuleVersion } from './admin-governance.models';

@Component({ selector: 'app-clinical-rules', standalone: true, imports: [RouterLink], templateUrl: './clinical-rules.component.html', styleUrl: './governance-page.scss', changeDetection: ChangeDetectionStrategy.OnPush })
export class ClinicalRulesComponent implements OnInit {
  private readonly service = inject(AdminGovernanceService);
  protected readonly rules = signal<ClinicalRuleVersion[]>([]);
  protected readonly error = signal<string | null>(null);
  ngOnInit(): void {
    this.service.getRules().subscribe({ next: value => this.rules.set(value), error: () => this.error.set('Clinical rules could not be loaded.') });
  }
}

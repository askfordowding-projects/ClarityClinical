import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminGovernanceService } from './admin-governance.service';
import { GuidelineSource } from './admin-governance.models';

@Component({ selector: 'app-guidelines', standalone: true, imports: [RouterLink], templateUrl: './guidelines.component.html', styleUrl: './governance-page.scss', changeDetection: ChangeDetectionStrategy.OnPush })
export class GuidelinesComponent implements OnInit {
  private readonly service = inject(AdminGovernanceService);
  protected readonly sources = signal<GuidelineSource[]>([]);
  protected readonly error = signal<string | null>(null);
  ngOnInit(): void { this.service.getGuidelines().subscribe({ next: value => this.sources.set(value), error: () => this.error.set('Guideline sources could not be loaded.') }); }
}

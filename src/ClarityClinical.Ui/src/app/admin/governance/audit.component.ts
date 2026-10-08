import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminGovernanceService } from './admin-governance.service';
import { AdminAuditPage } from './admin-governance.models';

@Component({ selector: 'app-admin-audit', standalone: true, imports: [RouterLink], templateUrl: './audit.component.html', styleUrl: './governance-page.scss', changeDetection: ChangeDetectionStrategy.OnPush })
export class AuditComponent implements OnInit {
  private readonly service = inject(AdminGovernanceService);
  protected readonly audit = signal<AdminAuditPage>({ totalCount: 0, items: [] });
  protected readonly error = signal<string | null>(null);
  ngOnInit(): void { this.service.getAudit().subscribe({ next: value => this.audit.set(value), error: () => this.error.set('Audit activity could not be loaded.') }); }
}

import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DemoAdminService } from './demo-admin.service';
import { DemoPatient } from './demo-admin.models';
@Component({ selector: 'app-admin-patients', standalone: true, imports: [RouterLink], templateUrl: './patients.component.html', styleUrl: '../governance/governance-page.scss', changeDetection: ChangeDetectionStrategy.OnPush })
export class PatientsComponent implements OnInit {
  private readonly service = inject(DemoAdminService); protected readonly patients = signal<DemoPatient[]>([]); protected readonly error = signal<string | null>(null);
  ngOnInit(): void { this.service.getPatients().subscribe({ next: v => this.patients.set(v), error: () => this.error.set('Synthetic patients could not be loaded.') }); }
}

import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ClinicalWorkspace } from './clinical-workspace.models';
import { ClinicalWorkspaceService } from './clinical-workspace.service';
import { SymptomPanelComponent } from './components/symptom-panel/symptom-panel.component';
import { ConsultationPanelComponent } from './components/consultation-panel/consultation-panel.component';
import { PatientContextPanelComponent } from './components/patient-context-panel/patient-context-panel.component';
import { ClinicalIntelligencePanelComponent } from './components/clinical-intelligence-panel/clinical-intelligence-panel.component';

@Component({
  selector: 'app-clinical-workspace',
  standalone: true,
  imports: [
    SymptomPanelComponent,
    ConsultationPanelComponent,
    PatientContextPanelComponent,
    ClinicalIntelligencePanelComponent
  ],
  templateUrl: './clinical-workspace.component.html',
  styleUrl: './clinical-workspace.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClinicalWorkspaceComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly workspaceService = inject(ClinicalWorkspaceService);

  protected readonly workspace = signal<ClinicalWorkspace | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    const consultationId = this.route.snapshot.paramMap.get('id');
    if (!consultationId) {
      this.isLoading.set(false);
      this.errorMessage.set('A consultation identifier is required.');
      return;
    }

    this.workspaceService.getWorkspace(consultationId).subscribe({
      next: workspace => {
        this.workspace.set(workspace);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('The consultation workspace could not be loaded.');
        this.isLoading.set(false);
      }
    });
  }
}

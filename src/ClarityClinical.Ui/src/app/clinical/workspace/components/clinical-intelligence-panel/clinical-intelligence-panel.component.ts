import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  AssessmentCandidate,
  ClinicianResponseSubmission
} from '../../clinical-workspace.models';

@Component({
  selector: 'app-clinical-intelligence-panel',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './clinical-intelligence-panel.component.html',
  styleUrl: './clinical-intelligence-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClinicalIntelligencePanelComponent {
  @Input() assessments: AssessmentCandidate[] = [];
  @Input() warnings: string[] = [];
  @Input() suggestedChecks: string[] = [];
  @Output() readonly responseRequested = new EventEmitter<ClinicianResponseSubmission>();

  protected readonly scoreExplanations = new Set<string>();
  protected readonly changeExplanations = new Set<string>();
  protected modifyingKey: string | null = null;
  protected rationale = '';
  protected modifiedAction = '';

  protected toggleScoreExplanation(key: string): void {
    this.toggle(this.scoreExplanations, key);
  }

  protected toggleChangeExplanation(key: string): void {
    this.toggle(this.changeExplanations, key);
  }

  protected startModification(assessment: AssessmentCandidate): void {
    this.modifyingKey = assessment.key;
    this.rationale = assessment.response?.rationale ?? '';
    this.modifiedAction = assessment.response?.modifiedAction ?? '';
  }

  protected submitResponse(
    assessment: AssessmentCandidate,
    responseType: 'Accepted' | 'Rejected' | 'Deferred'): void {
    this.responseRequested.emit({
      recommendationKey: assessment.key,
      responseType,
      rationale: null,
      modifiedAction: null
    });
  }

  protected submitModification(assessment: AssessmentCandidate): void {
    if (!this.canSaveModification()) {
      return;
    }

    this.responseRequested.emit({
      recommendationKey: assessment.key,
      responseType: 'Modified',
      rationale: this.rationale.trim() || null,
      modifiedAction: this.modifiedAction.trim() || null
    });
    this.modifyingKey = null;
  }

  protected evidenceSourceLabel(source: string): string {
    switch (source) {
      case 'PatientReport':
        return 'Patient report';
      case 'ClinicalObservation':
        return 'Clinician observation';
      case 'EstablishedRecord':
        return 'Established record';
      case 'AiInference':
        return 'AI inference';
      default:
        return source;
    }
  }
  protected canSaveModification(): boolean {
    return this.rationale.trim().length > 0 || this.modifiedAction.trim().length > 0;
  }

  private toggle(values: Set<string>, key: string): void {
    values.has(key) ? values.delete(key) : values.add(key);
  }
}

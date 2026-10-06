import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AssessmentCandidate } from '../../clinical-workspace.models';

@Component({
  selector: 'app-clinical-intelligence-panel',
  standalone: true,
  templateUrl: './clinical-intelligence-panel.component.html',
  styleUrl: './clinical-intelligence-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClinicalIntelligencePanelComponent {
  @Input() assessments: AssessmentCandidate[] = [];
}

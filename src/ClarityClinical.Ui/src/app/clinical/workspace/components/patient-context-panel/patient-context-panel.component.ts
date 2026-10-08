import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { PatientContext } from '../../clinical-workspace.models';

@Component({
  selector: 'app-patient-context-panel',
  standalone: true,
  templateUrl: './patient-context-panel.component.html',
  styleUrl: './patient-context-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PatientContextPanelComponent {
  @Input() patient: PatientContext | null = null;

  protected hasMedication(code: string): boolean {
    return this.patient?.medications.some(item => item.code.toLowerCase() === code.toLowerCase()) ?? false;
  }
}

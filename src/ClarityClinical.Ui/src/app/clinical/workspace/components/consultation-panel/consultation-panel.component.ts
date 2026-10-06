import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { TranscriptEntry } from '../../clinical-workspace.models';

@Component({
  selector: 'app-consultation-panel',
  standalone: true,
  templateUrl: './consultation-panel.component.html',
  styleUrl: './consultation-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConsultationPanelComponent {
  @Input() transcript: TranscriptEntry[] = [];
}

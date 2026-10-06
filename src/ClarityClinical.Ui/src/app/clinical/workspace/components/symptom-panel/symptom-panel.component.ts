import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-symptom-panel',
  standalone: true,
  templateUrl: './symptom-panel.component.html',
  styleUrl: './symptom-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SymptomPanelComponent {}

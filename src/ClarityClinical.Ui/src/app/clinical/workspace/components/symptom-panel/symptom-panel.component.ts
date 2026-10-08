import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

@Component({
  selector: 'app-symptom-panel',
  standalone: true,
  templateUrl: './symptom-panel.component.html',
  styleUrl: './symptom-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SymptomPanelComponent {
  protected readonly filters = ['Pain', 'Swelling', 'Rash', 'Injury', 'Numbness', 'Mobility', 'Infection'] as const;
  protected readonly selectedFilters = signal<ReadonlySet<string>>(new Set(['Swelling']));

  protected toggleFilter(filter: string): void {
    const next = new Set(this.selectedFilters());
    next.has(filter) ? next.delete(filter) : next.add(filter);
    this.selectedFilters.set(next);
  }
}

import { TestBed } from '@angular/core/testing';
import { SymptomPanelComponent } from './symptom-panel.component';

describe('SymptomPanelComponent', () => {
  it('shows Figure 1 concern copy and supports selectable symptom filters', async () => {
    await TestBed.configureTestingModule({ imports: [SymptomPanelComponent] }).compileComponents();
    const fixture = TestBed.createComponent(SymptomPanelComponent); fixture.detectChanges();
    const root = fixture.nativeElement as HTMLElement;
    expect(root.textContent).toContain('Left calf / ankle');
    expect(root.textContent).toContain('Swelling · wound · restricted mobility');
    const swelling = [...root.querySelectorAll('button')].find(x => x.textContent?.trim() === 'Swelling') as HTMLButtonElement;
    expect(swelling.getAttribute('aria-pressed')).toBe('true');
    const pain = [...root.querySelectorAll('button')].find(x => x.textContent?.trim() === 'Pain') as HTMLButtonElement;
    pain.click(); fixture.detectChanges();
    expect(pain.getAttribute('aria-pressed')).toBe('true');
  });
});

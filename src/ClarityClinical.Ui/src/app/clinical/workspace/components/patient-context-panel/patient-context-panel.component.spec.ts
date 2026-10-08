import { TestBed } from '@angular/core/testing';
import { PatientContextPanelComponent } from './patient-context-panel.component';

describe('PatientContextPanelComponent', () => {
  it('shows the Figure 1 medication safety flag for Ramipril', async () => {
    await TestBed.configureTestingModule({ imports: [PatientContextPanelComponent] }).compileComponents();
    const fixture=TestBed.createComponent(PatientContextPanelComponent);
    fixture.componentInstance.patient={ id:'p', displayName:'Miguel Santos', age:58, conditions:[], allergies:[{code:'ibuprofen',displayName:'Ibuprofen'}], medications:[{code:'ramipril',displayName:'Ramipril'}] };
    fixture.detectChanges();
    const text=fixture.nativeElement.textContent;
    expect(text).toContain('Medication safety');
    expect(text).toContain('Ramipril + NSAIDs');
  });
});

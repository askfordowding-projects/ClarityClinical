import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { DemoAdminService } from './demo-admin.service';
import { ScenariosComponent } from './scenarios.component';

describe('ScenariosComponent', () => {
  it('shows canonical scenario metadata and step count', async () => {
    await TestBed.configureTestingModule({ imports: [ScenariosComponent], providers: [provideRouter([]), { provide: DemoAdminService, useValue: { getScenarios: () => of([{ id: '1', scenarioKey: 'miguel-santos-leg-swelling', name: 'Miguel Santos - calf swelling', setting: 'Urgent Treatment Centre', sourceLanguage: 'es', clinicianLanguage: 'en', isCanonical: true, stepCount: 4 }]) } }] }).compileComponents();
    const fixture = TestBed.createComponent(ScenariosComponent); fixture.detectChanges();
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Miguel Santos - calf swelling'); expect(text).toContain('Urgent Treatment Centre'); expect(text).toContain('4 steps'); expect(text).toContain('read-only');
  });
});

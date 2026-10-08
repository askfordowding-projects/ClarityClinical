import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { DemoAdminService } from './demo-admin.service';
import { SandboxEditorComponent } from './sandbox-editor.component';

describe('SandboxEditorComponent', () => {
  it('makes sandbox isolation explicit and loads editable patient/scenario data', async () => {
    await TestBed.configureTestingModule({ imports:[SandboxEditorComponent], providers:[provideRouter([]), { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'miguel-santos-leg-swelling' } } } }, { provide: DemoAdminService, useValue: { createSandbox: () => of({ id:'s', sourceScenarioId:'c', patient:{ id:'p', givenName:'Miguel', familyName:'Santos', displayName:'Miguel Santos', dateOfBirth:'1968-06-15', conditions:[{code:'type-2-diabetes',displayName:'Type 2 diabetes'}], allergies:[], medications:[] }, scenario:{ id:'x', scenarioKey:'sandbox-key', name:'Miguel scenario', description:'Demo', setting:'Urgent Treatment Centre', sourceLanguage:'es', clinicianLanguage:'en', isCanonical:false, stepCount:4 } }), updateSandboxPatient: () => of(null), updateSandboxScenario: () => of(null) } }] }).compileComponents();
    const fixture=TestBed.createComponent(SandboxEditorComponent); fixture.detectChanges();
    const text=fixture.nativeElement.textContent;
    expect(text).toContain('Your sandbox copy'); expect(text).toContain('canonical Miguel template remains unchanged'); expect(text).toContain('Miguel'); const setting = fixture.nativeElement.querySelector('[data-input=setting]') as HTMLInputElement; expect(setting.value).toBe('Urgent Treatment Centre');
  });
});

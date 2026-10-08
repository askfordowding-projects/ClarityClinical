import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { DemoAdminService } from './demo-admin.service';
import { PatientsComponent } from './patients.component';

describe('PatientsComponent', () => {
  it('shows canonical patient context as read-only demo data', async () => {
    await TestBed.configureTestingModule({ imports: [PatientsComponent], providers: [provideRouter([]), { provide: DemoAdminService, useValue: { getPatients: () => of([{ id: '1', displayName: 'Miguel Santos', dateOfBirth: '1968-06-15', isCanonical: true, conditions: [{ code: 'type-2-diabetes', displayName: 'Type 2 diabetes' }], allergies: [{ code: 'penicillin', displayName: 'Penicillin' }], medications: [{ code: 'metformin', displayName: 'Metformin' }] }]) } }] }).compileComponents();
    const fixture = TestBed.createComponent(PatientsComponent); fixture.detectChanges();
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Miguel Santos'); expect(text).toContain('Type 2 diabetes'); expect(text).toContain('Penicillin'); expect(text).toContain('Metformin'); expect(text).toContain('read-only');
  });
});

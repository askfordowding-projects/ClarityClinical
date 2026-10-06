import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { EMPTY, of } from 'rxjs';
import { ClinicalWorkspaceComponent } from './clinical-workspace.component';
import { ClinicalWorkspaceService } from './clinical-workspace.service';
import { ClinicalWorkspace } from './clinical-workspace.models';

describe('ClinicalWorkspaceComponent', () => {
  let fixture: ComponentFixture<ClinicalWorkspaceComponent>;
  let service: jasmine.SpyObj<ClinicalWorkspaceService>;

  beforeEach(async () => {
    service = jasmine.createSpyObj<ClinicalWorkspaceService>('ClinicalWorkspaceService', ['getWorkspace']);

    await TestBed.configureTestingModule({
      imports: [ClinicalWorkspaceComponent],
      providers: [
        { provide: ClinicalWorkspaceService, useValue: service },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: '11111111-1111-1111-1111-111111111111' }) } }
        }
      ]
    }).compileComponents();
  });

  it('renders the four Figure 1 clinical workspace regions', () => {
    service.getWorkspace.and.returnValue(EMPTY);
    createComponent();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('[data-region="symptoms"]')).not.toBeNull();
    expect(element.querySelector('[data-region="consultation"]')).not.toBeNull();
    expect(element.querySelector('[data-region="patient-context"]')).not.toBeNull();
    expect(element.querySelector('[data-region="clinical-intelligence"]')).not.toBeNull();
    expect(element.textContent).toContain('DEMONSTRATION SYSTEM');
    expect(element.textContent).toContain('SYNTHETIC DATA ONLY');
  });

  it('binds the Figure 1 patient and assessment data from one workspace response', () => {
    service.getWorkspace.and.returnValue(of(createWorkspace()));
    createComponent();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(service.getWorkspace).toHaveBeenCalledTimes(1);
    expect(text).toContain('Miguel Santos');
    expect(text).toContain('Penicillin');
    expect(text).toContain('Ibuprofen');
    expect(text).toContain('Metformin');
    expect(text).toContain('Deep vein thrombosis');
    expect(text).toContain('72%');
    expect(text).toContain('Cellulitis');
    expect(text).toContain('58%');
    expect(text).toContain('Musculoskeletal inflammation');
    expect(text).toContain('41%');
    expect(text).toContain('Provisional ' + String.fromCharCode(8226) + ' Clinician review required');
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(ClinicalWorkspaceComponent);
    fixture.detectChanges();
  }

  function createWorkspace(): ClinicalWorkspace {
    return {
      consultation: { id: '11111111-1111-1111-1111-111111111111', status: 'InProgress' },
      patient: {
        id: '22222222-2222-2222-2222-222222222222',
        displayName: 'Miguel Santos',
        age: 58,
        conditions: [
          { code: 'type-2-diabetes', displayName: 'Type 2 diabetes' },
          { code: 'hypertension', displayName: 'Hypertension' }
        ],
        allergies: [
          { code: 'penicillin', displayName: 'Penicillin' },
          { code: 'ibuprofen', displayName: 'Ibuprofen' }
        ],
        medications: [
          { code: 'metformin', displayName: 'Metformin' },
          { code: 'ramipril', displayName: 'Ramipril' }
        ]
      },
      transcript: [],
      assessments: [
        createAssessment('dvt', 'Deep vein thrombosis', 72),
        createAssessment('cellulitis', 'Cellulitis', 58),
        createAssessment('musculoskeletal', 'Musculoskeletal inflammation', 41)
      ],
      warnings: [],
      suggestedChecks: [],
      permissions: {
        canRecordObservation: true,
        canAddPatientReport: true,
        canCompleteConsultation: true
      }
    };
  }

  function createAssessment(key: string, displayName: string, score: number) {
    return {
      key,
      displayName,
      currentScore: score,
      previousScore: null,
      scoreType: 'IllustrativePriority',
      supportingEvidence: [],
      missingEvidence: [],
      suggestedChecks: [],
      warnings: [],
      changeReason: null
    };
  }
});

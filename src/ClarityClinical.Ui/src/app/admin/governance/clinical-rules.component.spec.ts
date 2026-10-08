import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';
import { AdminGovernanceService } from './admin-governance.service';
import { ClinicalRulesComponent } from './clinical-rules.component';

describe('ClinicalRulesComponent', () => {
  let fixture: ComponentFixture<ClinicalRulesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClinicalRulesComponent],
      providers: [provideRouter([]), {
        provide: AdminGovernanceService,
        useValue: { getRules: () => of([{
          ruleKey: 'dvt', displayName: 'Deep vein thrombosis', version: 'demo-illustrative-1.0',
          status: 'Active', scoreType: 'IllustrativePriority', isIllustrative: true,
          provenanceNote: 'Illustrative demo logic only; this score is not a validated disease probability.',
          effectiveFrom: '2026-10-08',
          source: { referenceCode: 'NG158', organisation: 'NICE', title: 'Venous thromboembolic diseases', url: null, publishedDate: '2020-03-26', lastUpdatedDate: null, lastReviewedDate: null, reviewStatus: 'ReviewRequired' }
        }]) }
      }]
    }).compileComponents();
    fixture = TestBed.createComponent(ClinicalRulesComponent);
    fixture.detectChanges();
  });

  it('shows provenance and makes illustrative scoring explicit', () => {
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Deep vein thrombosis');
    expect(text).toContain('NG158');
    expect(text).toContain('IllustrativePriority');
    expect(text).toContain('not a validated disease probability');
  });
});

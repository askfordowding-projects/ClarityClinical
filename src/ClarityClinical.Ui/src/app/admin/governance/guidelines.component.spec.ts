import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';
import { AdminGovernanceService } from './admin-governance.service';
import { GuidelinesComponent } from './guidelines.component';

describe('GuidelinesComponent', () => {
  let fixture: ComponentFixture<GuidelinesComponent>;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GuidelinesComponent],
      providers: [provideRouter([]), { provide: AdminGovernanceService, useValue: { getGuidelines: () => of([{ referenceCode: 'NG158', organisation: 'NICE', title: 'Venous thromboembolic diseases', url: 'https://www.nice.org.uk/guidance/ng158', publishedDate: '2020-03-26', lastUpdatedDate: null, lastReviewedDate: null, reviewStatus: 'ReviewRequired' }]) } }]
    }).compileComponents();
    fixture = TestBed.createComponent(GuidelinesComponent); fixture.detectChanges();
  });
  it('shows source identity and review status', () => {
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('NG158'); expect(text).toContain('NICE'); expect(text).toContain('ReviewRequired');
  });
});

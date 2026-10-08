import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { provideRouter } from '@angular/router';
import { AdminGovernanceService } from './admin-governance.service';
import { AuditComponent } from './audit.component';

describe('AuditComponent', () => {
  let fixture: ComponentFixture<AuditComponent>;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuditComponent],
      providers: [provideRouter([]), { provide: AdminGovernanceService, useValue: { getAudit: () => of({ totalCount: 1, items: [{ id: '1', consultationId: '2', actorRole: 'Clinician', action: 'TranscriptCorrected', entityType: 'TranscriptSegment', entityId: '3', occurredAt: '2026-10-08T10:00:00Z', correlationId: 'corr', previousState: null, newState: 'Corrected', reason: null, ruleVersion: null }] }) } }]
    }).compileComponents();
    fixture = TestBed.createComponent(AuditComponent); fixture.detectChanges();
  });
  it('shows traceable audit activity without requiring clinical free text', () => {
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('TranscriptCorrected'); expect(text).toContain('TranscriptSegment'); expect(text).toContain('Clinician');
  });
});

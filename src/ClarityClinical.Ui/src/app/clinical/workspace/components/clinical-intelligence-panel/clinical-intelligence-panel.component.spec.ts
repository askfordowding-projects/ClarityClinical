import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ClinicalIntelligencePanelComponent } from './clinical-intelligence-panel.component';
import { AssessmentCandidate } from '../../clinical-workspace.models';

describe('ClinicalIntelligencePanelComponent', () => {
  let fixture: ComponentFixture<ClinicalIntelligencePanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClinicalIntelligencePanelComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ClinicalIntelligencePanelComponent);
    fixture.componentInstance.assessments = [createAssessment()];
    fixture.detectChanges();
  });

  it('reveals score evidence only when requested', () => {
    expect(element().querySelector('[data-explanation="score"]')).toBeNull();

    clickButton('Why this score?');
    fixture.detectChanges();

    const explanation = element().querySelector('[data-explanation="score"]');
    expect(explanation).not.toBeNull();
    expect(explanation?.textContent).toContain('Unilateral calf swelling');
    expect(explanation?.textContent).toContain('Wells assessment');
  });

  it('reveals why the assessment changed', () => {
    expect(element().querySelector('[data-explanation="change"]')).toBeNull();

    clickButton('Why did this change?');
    fixture.detectChanges();

    expect(element().querySelector('[data-explanation="change"]')?.textContent)
      .toContain('Recent prolonged travel added to clinical reasoning.');
  });

  it('offers accept modify reject and defer controls', () => {
    const text = element().textContent ?? '';
    expect(text).toContain('Accept');
    expect(text).toContain('Modify');
    expect(text).toContain('Reject');
    expect(text).toContain('Defer');
  });

  it('requires rationale or changed action before submitting a modification', () => {
    clickButton('Modify');
    fixture.detectChanges();

    const submit = button('Save modification');
    expect(submit.disabled).toBeTrue();

    const rationale = element().querySelector('textarea[name="rationale"]') as HTMLTextAreaElement;
    rationale.value = 'Travel history needs confirmation';
    rationale.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect(button('Save modification').disabled).toBeFalse();
  });

  function clickButton(label: string): void {
    button(label).click();
  }

  function button(label: string): HTMLButtonElement {
    const match = [...element().querySelectorAll('button')]
      .find(item => item.textContent?.trim() === label) as HTMLButtonElement | undefined;
    expect(match).toBeDefined();
    return match!;
  }

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function createAssessment(): AssessmentCandidate {
    return {
      key: 'dvt',
      displayName: 'Deep vein thrombosis',
      currentScore: 72,
      previousScore: 67,
      scoreType: 'IllustrativePriority',
      supportingEvidence: [
        { factCode: 'unilateral-calf-swelling', displayText: 'Unilateral calf swelling', source: 'PatientReport' }
      ],
      missingEvidence: ['Wells assessment'],
      suggestedChecks: ['Complete Wells assessment'],
      warnings: [],
      changeReason: 'Recent prolonged travel added to clinical reasoning.',
      response: null
    };
  }
});

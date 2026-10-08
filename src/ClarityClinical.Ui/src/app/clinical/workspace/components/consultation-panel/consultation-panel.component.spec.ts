import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ConsultationPanelComponent } from './consultation-panel.component';
import { SpeechTranslationService, SpeechTranslationCallbacks } from '../../../speech/speech-translation.service';
import { ClinicalWorkspaceService } from '../../clinical-workspace.service';
import { ClinicalWorkspace } from '../../clinical-workspace.models';

describe('ConsultationPanelComponent', () => {
  let fixture: ComponentFixture<ConsultationPanelComponent>;
  let speech: jasmine.SpyObj<SpeechTranslationService>;
  let workspaceService: jasmine.SpyObj<ClinicalWorkspaceService>;
  let callbacks: SpeechTranslationCallbacks | null;

  beforeEach(async () => {
    callbacks = null;
    speech = jasmine.createSpyObj<SpeechTranslationService>('SpeechTranslationService', ['start', 'stop']);
    speech.start.and.callFake(async options => {
      callbacks = options.callbacks;
    });
    speech.stop.and.resolveTo();
    workspaceService = jasmine.createSpyObj<ClinicalWorkspaceService>('ClinicalWorkspaceService', [
      'addTranscriptSegment',
      'correctTranscriptSegment',
      'changeTranscriptSpeaker',
      'excludeTranscriptSegment',
      'redactTranscriptSegment'
    ]);
    workspaceService.addTranscriptSegment.and.returnValue(of({ transcript: [] } as unknown as ClinicalWorkspace));
    workspaceService.correctTranscriptSegment.and.returnValue(of({ transcript: [] } as unknown as ClinicalWorkspace));
    workspaceService.changeTranscriptSpeaker.and.returnValue(of({ transcript: [] } as unknown as ClinicalWorkspace));
    workspaceService.excludeTranscriptSegment.and.returnValue(of({ transcript: [] } as unknown as ClinicalWorkspace));
    workspaceService.redactTranscriptSegment.and.returnValue(of({ transcript: [] } as unknown as ClinicalWorkspace));

    await TestBed.configureTestingModule({
      imports: [ConsultationPanelComponent],
      providers: [
        { provide: SpeechTranslationService, useValue: speech },
        { provide: ClinicalWorkspaceService, useValue: workspaceService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ConsultationPanelComponent);
    fixture.componentRef.setInput('consultationId', 'consultation-1');
    fixture.detectChanges();
  });

  it('starts Spanish to English translation for the patient and persists the final segment', async () => {
    startListening();
    await fixture.whenStable();

    expect(speech.start).toHaveBeenCalled();
    const options = speech.start.calls.mostRecent().args[0];
    expect(options.sourceLanguage).toBe('es-ES');
    expect(options.targetLanguage).toBe('en');

    callbacks!.onRecognizing('Tengo la pierna izquierda hinchada');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Tengo la pierna izquierda hinchada');

    callbacks!.onRecognized({
      sourceText: 'Tengo la pierna izquierda hinchada desde ayer.',
      translatedText: 'My left leg has been swollen since yesterday.',
      recognitionConfidence: null
    });
    fixture.detectChanges();

    expect(workspaceService.addTranscriptSegment).toHaveBeenCalledWith(
      'consultation-1',
      jasmine.objectContaining({
        speakerRole: 'Patient',
        sourceLanguage: 'es-ES',
        originalText: 'Tengo la pierna izquierda hinchada desde ayer.',
        translatedText: 'My left leg has been swollen since yesterday.'
      })
    );
  });

  it('reverses translation direction when the clinician is speaking', async () => {
    const speaker = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
    speaker.value = 'Clinician';
    speaker.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    startListening();
    await fixture.whenStable();

    const options = speech.start.calls.mostRecent().args[0];
    expect(options.sourceLanguage).toBe('en-GB');
    expect(options.targetLanguage).toBe('es');
  });

  it('allows a typed statement to be saved without the speech provider', () => {
    const textarea = fixture.nativeElement.querySelector('[data-input="typed-statement"]') as HTMLTextAreaElement;
    textarea.value = 'Tengo dolor desde ayer.';
    textarea.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    const add = fixture.nativeElement.querySelector('[data-action="add-typed-statement"]') as HTMLButtonElement;
    add.click();
    fixture.detectChanges();

    expect(workspaceService.addTranscriptSegment).toHaveBeenCalledWith(
      'consultation-1',
      jasmine.objectContaining({
        speakerRole: 'Patient',
        sourceLanguage: 'es-ES',
        originalText: 'Tengo dolor desde ayer.',
        translatedText: null
      })
    );
  });


  it('saves a clinician transcript correction', () => {
    setTranscript();
    click('[data-action="correct-transcript"]');
    fixture.detectChanges();

    input('[data-input="corrected-text"]', 'Tengo la pierna izquierda hinchada desde ayer.');
    input('[data-input="corrected-translation"]', 'My left leg has been swollen since yesterday.');
    click('[data-action="save-correction"]');

    expect(workspaceService.correctTranscriptSegment).toHaveBeenCalledWith(
      'consultation-1',
      'segment-1',
      jasmine.objectContaining({ correctedText: 'Tengo la pierna izquierda hinchada desde ayer.' })
    );
  });

  it('changes transcript speaker attribution', () => {
    setTranscript();
    click('[data-action="change-speaker"]');
    fixture.detectChanges();

    select('[data-input="segment-speaker"]', 'Interpreter');
    click('[data-action="save-speaker"]');

    expect(workspaceService.changeTranscriptSpeaker).toHaveBeenCalledWith(
      'consultation-1',
      'segment-1',
      'Interpreter'
    );
  });

  it('excludes transcript from reasoning with a reason', () => {
    setTranscript();
    click('[data-action="exclude-transcript"]');
    fixture.detectChanges();

    select('[data-input="disposition-reason"]', 'Personal information not clinically relevant');
    click('[data-action="confirm-exclude"]');

    expect(workspaceService.excludeTranscriptSegment).toHaveBeenCalledWith(
      'consultation-1',
      'segment-1',
      'Personal information not clinically relevant'
    );
  });

  it('redacts transcript with a reason', () => {
    setTranscript();
    click('[data-action="redact-transcript"]');
    fixture.detectChanges();

    select('[data-input="disposition-reason"]', 'Patient requested exclusion');
    click('[data-action="confirm-redact"]');

    expect(workspaceService.redactTranscriptSegment).toHaveBeenCalledWith(
      'consultation-1',
      'segment-1',
      'Patient requested exclusion'
    );
  });


  it('does not expose editing controls for a redacted transcript segment', () => {
    fixture.componentRef.setInput('transcript', [{
      id: 'segment-redacted', speakerRole: 'Patient', originalLanguage: 'es-ES',
      originalText: '[Redacted]', translatedText: null,
      recognitionConfidence: 0.92, corrected: false,
      includeInReasoning: false, isRedacted: true
    }]);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[data-action="correct-transcript"]')).toBeNull();
    expect(fixture.nativeElement.querySelector('[data-action="change-speaker"]')).toBeNull();
    expect(fixture.nativeElement.querySelector('[data-action="exclude-transcript"]')).toBeNull();
    expect(fixture.nativeElement.querySelector('[data-action="redact-transcript"]')).toBeNull();
  });
  function setTranscript(): void {
    fixture.componentRef.setInput('transcript', [{
      id: 'segment-1', speakerRole: 'Patient', originalLanguage: 'es-ES',
      originalText: 'Tengo la pierna izquierda inchada desde ayer.',
      translatedText: 'My left leg has been swollen since yesterday.',
      recognitionConfidence: 0.92, corrected: false,
      includeInReasoning: true, isRedacted: false
    }]);
    fixture.detectChanges();
  }

  function click(selector: string): void {
    (fixture.nativeElement.querySelector(selector) as HTMLButtonElement).click();
  }

  function input(selector: string, value: string): void {
    const element = fixture.nativeElement.querySelector(selector) as HTMLInputElement | HTMLTextAreaElement;
    element.value = value;
    element.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function select(selector: string, value: string): void {
    const element = fixture.nativeElement.querySelector(selector) as HTMLSelectElement;
    element.value = value;
    element.dispatchEvent(new Event('change'));
    fixture.detectChanges();
  }
  function startListening(): void {
    const start = fixture.nativeElement.querySelector('[data-action="start-listening"]') as HTMLButtonElement;
    start.click();
  }
});
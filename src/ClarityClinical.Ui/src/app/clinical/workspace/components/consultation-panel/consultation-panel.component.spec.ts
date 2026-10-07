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
    workspaceService = jasmine.createSpyObj<ClinicalWorkspaceService>('ClinicalWorkspaceService', ['addTranscriptSegment']);
    workspaceService.addTranscriptSegment.and.returnValue(of({ transcript: [] } as unknown as ClinicalWorkspace));

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

  function startListening(): void {
    const start = fixture.nativeElement.querySelector('[data-action="start-listening"]') as HTMLButtonElement;
    start.click();
  }
});
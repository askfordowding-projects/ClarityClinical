import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { ClinicalWorkspace, TranscriptEntry } from '../../clinical-workspace.models';
import { ClinicalWorkspaceService } from '../../clinical-workspace.service';
import { SpeechRecognitionResult, SpeechTranslationService } from '../../../speech/speech-translation.service';

type SpeakerRole = 'Patient' | 'Clinician';

interface LanguageDirection {
  sourceLanguage: string;
  targetLanguage: string;
}

@Component({
  selector: 'app-consultation-panel',
  standalone: true,
  templateUrl: './consultation-panel.component.html',
  styleUrl: './consultation-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConsultationPanelComponent {
  private readonly speech = inject(SpeechTranslationService);
  private readonly workspaceService = inject(ClinicalWorkspaceService);

  @Input() consultationId = '';
  @Input() transcript: TranscriptEntry[] = [];
  @Output() workspaceUpdated = new EventEmitter<ClinicalWorkspace>();

  protected readonly isListening = signal(false);
  protected readonly interimText = signal('');
  protected readonly speechError = signal<string | null>(null);
  protected readonly selectedSpeaker = signal<SpeakerRole>('Patient');
  protected readonly typedStatement = signal('');

  protected async startListening(): Promise<void> {
    if (!this.consultationId || this.isListening()) {
      return;
    }

    const speaker = this.selectedSpeaker();
    const direction = this.languageDirection(speaker);
    this.speechError.set(null);
    this.interimText.set('');

    try {
      await this.speech.start({
        sourceLanguage: direction.sourceLanguage,
        targetLanguage: direction.targetLanguage,
        callbacks: {
          onRecognizing: text => this.interimText.set(text),
          onRecognized: result => this.handleRecognized(speaker, direction, result),
          onError: message => {
            this.speechError.set(message);
            this.isListening.set(false);
          },
          onStopped: () => this.isListening.set(false)
        }
      });
      this.isListening.set(true);
    } catch {
      this.speechError.set('Live speech is unavailable. You can type the statement instead.');
      this.isListening.set(false);
    }
  }

  protected async stopListening(): Promise<void> {
    try {
      await this.speech.stop();
    } finally {
      this.isListening.set(false);
      this.interimText.set('');
    }
  }

  protected changeSpeaker(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedSpeaker.set(value === 'Clinician' ? 'Clinician' : 'Patient');
  }

  protected updateTypedStatement(event: Event): void {
    this.typedStatement.set((event.target as HTMLTextAreaElement).value);
  }

  protected addTypedStatement(): void {
    const text = this.typedStatement().trim();
    if (!text || !this.consultationId) {
      return;
    }

    const speaker = this.selectedSpeaker();
    const direction = this.languageDirection(speaker);
    this.persistSegment(speaker, direction.sourceLanguage, text, null, null);
    this.typedStatement.set('');
  }

  protected languageLabel(): string {
    return this.selectedSpeaker() === 'Patient' ? 'Spanish → English' : 'English → Spanish';
  }

  private handleRecognized(
    speaker: SpeakerRole,
    direction: LanguageDirection,
    result: SpeechRecognitionResult
  ): void {
    this.interimText.set('');
    this.persistSegment(
      speaker,
      direction.sourceLanguage,
      result.sourceText,
      result.translatedText,
      result.recognitionConfidence
    );
  }

  private persistSegment(
    speakerRole: SpeakerRole,
    sourceLanguage: string,
    originalText: string,
    translatedText: string | null,
    recognitionConfidence: number | null
  ): void {
    this.workspaceService.addTranscriptSegment(this.consultationId, {
      speakerRole,
      sourceLanguage,
      originalText,
      translatedText,
      recognitionConfidence
    }).subscribe({
      next: workspace => this.workspaceUpdated.emit(workspace),
      error: () => this.speechError.set('The transcript could not be saved.')
    });
  }

  private languageDirection(speaker: SpeakerRole): LanguageDirection {
    return speaker === 'Patient'
      ? { sourceLanguage: 'es-ES', targetLanguage: 'en' }
      : { sourceLanguage: 'en-GB', targetLanguage: 'es' };
  }
}
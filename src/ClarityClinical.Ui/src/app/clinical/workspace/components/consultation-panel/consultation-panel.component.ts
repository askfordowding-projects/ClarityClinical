import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { ClinicalWorkspace, TranscriptEntry } from '../../clinical-workspace.models';
import { ClinicalWorkspaceService } from '../../clinical-workspace.service';
import { SpeechRecognitionResult, SpeechTranslationService } from '../../../speech/speech-translation.service';

type LiveSpeakerRole = 'Patient' | 'Clinician';
type TranscriptAction = 'correct' | 'speaker' | 'exclude' | 'redact';

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
  protected readonly selectedSpeaker = signal<LiveSpeakerRole>('Patient');
  protected readonly typedStatement = signal('');

  protected readonly activeSegmentId = signal<string | null>(null);
  protected readonly transcriptAction = signal<TranscriptAction | null>(null);
  protected readonly correctedText = signal('');
  protected readonly correctedTranslation = signal('');
  protected readonly segmentSpeaker = signal('Patient');
  protected readonly dispositionReason = signal('');
  protected readonly transcriptActionError = signal<string | null>(null);
  protected readonly actionBusy = signal(false);

  protected readonly transcriptSpeakerRoles = [
    'Patient', 'Clinician', 'Carer', 'FamilyMember', 'Interpreter', 'Other', 'Unknown'
  ];

  protected readonly dispositionReasons = [
    'Personal information not clinically relevant',
    'Third-party information',
    'Patient requested exclusion',
    'Transcription error',
    'Duplicate',
    'Other'
  ];

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

  protected beginCorrection(entry: TranscriptEntry): void {
    this.beginTranscriptAction(entry.id, 'correct');
    this.correctedText.set(entry.originalText);
    this.correctedTranslation.set(entry.translatedText ?? '');
  }

  protected beginSpeakerChange(entry: TranscriptEntry): void {
    this.beginTranscriptAction(entry.id, 'speaker');
    this.segmentSpeaker.set(entry.speakerRole);
  }

  protected beginDisposition(entry: TranscriptEntry, action: 'exclude' | 'redact'): void {
    this.beginTranscriptAction(entry.id, action);
    this.dispositionReason.set('');
  }

  protected cancelTranscriptAction(): void {
    this.resetTranscriptAction();
  }

  protected updateCorrectedText(event: Event): void {
    this.correctedText.set((event.target as HTMLTextAreaElement).value);
  }

  protected updateCorrectedTranslation(event: Event): void {
    this.correctedTranslation.set((event.target as HTMLInputElement).value);
  }

  protected updateSegmentSpeaker(event: Event): void {
    this.segmentSpeaker.set((event.target as HTMLSelectElement).value);
  }

  protected updateDispositionReason(event: Event): void {
    this.dispositionReason.set((event.target as HTMLSelectElement).value);
  }

  protected saveCorrection(): void {
    const segmentId = this.activeSegmentId();
    const correctedText = this.correctedText().trim();
    if (!segmentId || !correctedText) {
      return;
    }

    this.submitWorkspaceChange(this.workspaceService.correctTranscriptSegment(
      this.consultationId,
      segmentId,
      {
        correctedText,
        translatedText: this.correctedTranslation().trim() || null
      }
    ));
  }

  protected saveSpeaker(): void {
    const segmentId = this.activeSegmentId();
    if (!segmentId) {
      return;
    }

    this.submitWorkspaceChange(this.workspaceService.changeTranscriptSpeaker(
      this.consultationId,
      segmentId,
      this.segmentSpeaker()
    ));
  }

  protected confirmExclude(): void {
    const segmentId = this.activeSegmentId();
    const reason = this.dispositionReason();
    if (!segmentId || !reason) {
      return;
    }

    this.submitWorkspaceChange(this.workspaceService.excludeTranscriptSegment(
      this.consultationId,
      segmentId,
      reason
    ));
  }

  protected confirmRedact(): void {
    const segmentId = this.activeSegmentId();
    const reason = this.dispositionReason();
    if (!segmentId || !reason) {
      return;
    }

    this.submitWorkspaceChange(this.workspaceService.redactTranscriptSegment(
      this.consultationId,
      segmentId,
      reason
    ));
  }

  private beginTranscriptAction(segmentId: string, action: TranscriptAction): void {
    this.activeSegmentId.set(segmentId);
    this.transcriptAction.set(action);
    this.transcriptActionError.set(null);
  }

  private submitWorkspaceChange(request: Observable<ClinicalWorkspace>): void {
    this.actionBusy.set(true);
    this.transcriptActionError.set(null);
    request.subscribe({
      next: workspace => {
        this.workspaceUpdated.emit(workspace);
        this.actionBusy.set(false);
        this.resetTranscriptAction();
      },
      error: () => {
        this.actionBusy.set(false);
        this.transcriptActionError.set('The transcript change could not be saved.');
      }
    });
  }

  private resetTranscriptAction(): void {
    this.activeSegmentId.set(null);
    this.transcriptAction.set(null);
    this.correctedText.set('');
    this.correctedTranslation.set('');
    this.segmentSpeaker.set('Patient');
    this.dispositionReason.set('');
  }

  private handleRecognized(
    speaker: LiveSpeakerRole,
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
    speakerRole: LiveSpeakerRole,
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

  private languageDirection(speaker: LiveSpeakerRole): LanguageDirection {
    return speaker === 'Patient'
      ? { sourceLanguage: 'es-ES', targetLanguage: 'en' }
      : { sourceLanguage: 'en-GB', targetLanguage: 'es' };
  }
}
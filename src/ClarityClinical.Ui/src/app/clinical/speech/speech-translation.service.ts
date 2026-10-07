import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

interface SpeechTranslationConfigLike {
  speechRecognitionLanguage: string;
  addTargetLanguage(language: string): void;
}

interface AudioConfigLike {}

interface TranslationResultLike {
  text: string;
  reason: number;
  translations: {
    get(language: string): string | undefined;
  };
}

interface TranslationEventLike {
  result: TranslationResultLike;
}

interface CancellationEventLike {
  reason: number;
  errorDetails: string;
}

interface TranslationRecognizerLike {
  recognizing: ((sender: unknown, event: TranslationEventLike) => void) | null;
  recognized: ((sender: unknown, event: TranslationEventLike) => void) | null;
  canceled: ((sender: unknown, event: CancellationEventLike) => void) | null;
  sessionStopped: (() => void) | null;
  startContinuousRecognitionAsync(success: () => void, failure: (error: string) => void): void;
  stopContinuousRecognitionAsync(success: () => void, failure: (error: string) => void): void;
  close(): void;
}

interface SpeechSdkBrowser {
  SpeechTranslationConfig: {
    fromAuthorizationToken(token: string, region: string): SpeechTranslationConfigLike;
  };
  AudioConfig: {
    fromDefaultMicrophoneInput(): AudioConfigLike;
  };
  TranslationRecognizer: new (
    config: SpeechTranslationConfigLike,
    audioConfig: AudioConfigLike
  ) => TranslationRecognizerLike;
  ResultReason: {
    TranslatedSpeech: number;
  };
  CancellationReason: {
    Error: number;
  };
}

declare global {
  interface Window {
    SpeechSDK: SpeechSdkBrowser;
  }
}

export interface SpeechRecognitionResult {
  sourceText: string;
  translatedText: string | null;
  recognitionConfidence: number | null;
}

export interface SpeechTranslationCallbacks {
  onRecognizing(text: string): void;
  onRecognized(result: SpeechRecognitionResult): void;
  onError(message: string): void;
  onStopped(): void;
}

export interface SpeechTranslationOptions {
  sourceLanguage: string;
  targetLanguage: string;
  callbacks: SpeechTranslationCallbacks;
}

interface SpeechAuthorization {
  token: string;
  region: string;
  refreshAfter: string;
}

@Injectable({ providedIn: 'root' })
export class SpeechTranslationService {
  private readonly http = inject(HttpClient);
  private recognizer: TranslationRecognizerLike | null = null;
  private sdkPromise: Promise<SpeechSdkBrowser> | null = null;

  async start(options: SpeechTranslationOptions): Promise<void> {
    if (this.recognizer) {
      await this.stop();
    }

    const [authorization, speechSdk] = await Promise.all([
      firstValueFrom(this.http.post<SpeechAuthorization>('/api/speech/authorization', {})),
      this.loadSpeechSdk()
    ]);

    const translationConfig = speechSdk.SpeechTranslationConfig.fromAuthorizationToken(
      authorization.token,
      authorization.region
    );
    translationConfig.speechRecognitionLanguage = options.sourceLanguage;
    translationConfig.addTargetLanguage(options.targetLanguage);

    const audioConfig = speechSdk.AudioConfig.fromDefaultMicrophoneInput();
    const recognizer = new speechSdk.TranslationRecognizer(translationConfig, audioConfig);
    this.recognizer = recognizer;

    recognizer.recognizing = (_sender, event) => {
      if (event.result.text) {
        options.callbacks.onRecognizing(event.result.text);
      }
    };

    recognizer.recognized = (_sender, event) => {
      if (event.result.reason !== speechSdk.ResultReason.TranslatedSpeech || !event.result.text) {
        return;
      }

      options.callbacks.onRecognized({
        sourceText: event.result.text,
        translatedText: event.result.translations.get(options.targetLanguage) ?? null,
        recognitionConfidence: null
      });
    };

    recognizer.canceled = (_sender, event) => {
      if (event.reason === speechSdk.CancellationReason.Error) {
        options.callbacks.onError(event.errorDetails || 'Speech recognition failed.');
      }
    };

    recognizer.sessionStopped = () => options.callbacks.onStopped();

    await new Promise<void>((resolve, reject) => {
      recognizer.startContinuousRecognitionAsync(resolve, reject);
    });
  }

  async stop(): Promise<void> {
    const recognizer = this.recognizer;
    this.recognizer = null;
    if (!recognizer) {
      return;
    }

    await new Promise<void>((resolve, reject) => {
      recognizer.stopContinuousRecognitionAsync(
        () => {
          recognizer.close();
          resolve();
        },
        error => {
          recognizer.close();
          reject(error);
        }
      );
    });
  }

  private loadSpeechSdk(): Promise<SpeechSdkBrowser> {
    if (window.SpeechSDK) {
      return Promise.resolve(window.SpeechSDK);
    }

    if (this.sdkPromise) {
      return this.sdkPromise;
    }

    this.sdkPromise = new Promise<SpeechSdkBrowser>((resolve, reject) => {
      const script = document.createElement('script');
      script.src = '/speech-sdk/microsoft.cognitiveservices.speech.sdk.bundle-min.js';
      script.async = true;
      script.onload = () => {
        if (window.SpeechSDK) {
          resolve(window.SpeechSDK);
          return;
        }
        reject(new Error('Speech SDK loaded without exposing its browser API.'));
      };
      script.onerror = () => reject(new Error('Speech SDK could not be loaded.'));
      document.head.appendChild(script);
    });

    return this.sdkPromise;
  }
}
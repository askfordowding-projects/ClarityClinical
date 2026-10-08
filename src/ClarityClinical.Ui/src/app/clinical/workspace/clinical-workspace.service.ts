import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ClinicalWorkspace } from './clinical-workspace.models';

export interface AddTranscriptSegmentRequest {
  speakerRole: string;
  sourceLanguage: string;
  originalText: string;
  translatedText: string | null;
  recognitionConfidence: number | null;
}

export interface CorrectTranscriptSegmentRequest {
  correctedText: string;
  translatedText: string | null;
}

@Injectable({ providedIn: 'root' })
export class ClinicalWorkspaceService {
  private readonly http = inject(HttpClient);

  getWorkspace(consultationId: string): Observable<ClinicalWorkspace> {
    return this.http.get<ClinicalWorkspace>(`/api/consultations/${consultationId}/workspace`);
  }

  addTranscriptSegment(
    consultationId: string,
    request: AddTranscriptSegmentRequest
  ): Observable<ClinicalWorkspace> {
    return this.http.post<ClinicalWorkspace>(
      `/api/consultations/${consultationId}/transcript-segments`,
      request
    );
  }

  correctTranscriptSegment(
    consultationId: string,
    segmentId: string,
    request: CorrectTranscriptSegmentRequest
  ): Observable<ClinicalWorkspace> {
    return this.http.patch<ClinicalWorkspace>(
      `/api/consultations/${consultationId}/transcript-segments/${segmentId}/correction`,
      request
    );
  }

  changeTranscriptSpeaker(
    consultationId: string,
    segmentId: string,
    speakerRole: string
  ): Observable<ClinicalWorkspace> {
    return this.http.post<ClinicalWorkspace>(
      `/api/consultations/${consultationId}/transcript-segments/${segmentId}/speaker`,
      { speakerRole }
    );
  }

  excludeTranscriptSegment(
    consultationId: string,
    segmentId: string,
    reason: string
  ): Observable<ClinicalWorkspace> {
    return this.http.post<ClinicalWorkspace>(
      `/api/consultations/${consultationId}/transcript-segments/${segmentId}/exclude`,
      { reason }
    );
  }

  redactTranscriptSegment(
    consultationId: string,
    segmentId: string,
    reason: string
  ): Observable<ClinicalWorkspace> {
    return this.http.post<ClinicalWorkspace>(
      `/api/consultations/${consultationId}/transcript-segments/${segmentId}/redact`,
      { reason }
    );
  }
}
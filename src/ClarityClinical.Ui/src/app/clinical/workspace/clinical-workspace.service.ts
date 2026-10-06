import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ClinicalWorkspace } from './clinical-workspace.models';

@Injectable({ providedIn: 'root' })
export class ClinicalWorkspaceService {
  private readonly http = inject(HttpClient);

  getWorkspace(consultationId: string): Observable<ClinicalWorkspace> {
    return this.http.get<ClinicalWorkspace>(
      `/api/consultations/${consultationId}/workspace`
    );
  }
}

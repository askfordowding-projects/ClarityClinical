import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { DemoPatient, DemoScenario } from './demo-admin.models';

@Injectable({ providedIn: 'root' })
export class DemoAdminService {
  private readonly http = inject(HttpClient);
  getPatients(): Observable<DemoPatient[]> { return this.http.get<DemoPatient[]>('/api/admin/demo/patients'); }
  getScenarios(): Observable<DemoScenario[]> { return this.http.get<DemoScenario[]>('/api/admin/demo/scenarios'); }
  resetSession(sessionId: string): Observable<{ id: string; consultationId: string }> { return this.http.post<{ id: string; consultationId: string }>(`/api/admin/demo/sessions/${sessionId}/reset`, null); }
}

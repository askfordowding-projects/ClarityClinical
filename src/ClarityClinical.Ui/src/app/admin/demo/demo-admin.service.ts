import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { DemoPatient, DemoScenario, DemoSandboxSnapshot, DemoSandboxValue } from './demo-admin.models';

@Injectable({ providedIn: 'root' })
export class DemoAdminService {
  private readonly http = inject(HttpClient);
  getPatients(): Observable<DemoPatient[]> { return this.http.get<DemoPatient[]>('/api/admin/demo/patients'); }
  getScenarios(): Observable<DemoScenario[]> { return this.http.get<DemoScenario[]>('/api/admin/demo/scenarios'); }
  resetSession(sessionId: string): Observable<{ id: string; consultationId: string }> { return this.http.post<{ id: string; consultationId: string }>(`/api/admin/demo/sessions/${sessionId}/reset`, null); }

  createSandbox(scenarioKey: string): Observable<DemoSandboxSnapshot> { return this.http.post<DemoSandboxSnapshot>(`/api/admin/demo/scenarios/${scenarioKey}/sandbox`, null); }
  updateSandboxPatient(scenarioKey: string, value: { givenName: string; familyName: string; dateOfBirth: string; conditions: DemoSandboxValue[]; allergies: DemoSandboxValue[]; medications: DemoSandboxValue[] }): Observable<DemoSandboxSnapshot> { return this.http.put<DemoSandboxSnapshot>(`/api/admin/demo/scenarios/${scenarioKey}/sandbox/patient`, value); }
  updateSandboxScenario(scenarioKey: string, value: { name: string; description: string; setting: string; sourceLanguage: string; clinicianLanguage: string }): Observable<DemoSandboxSnapshot> { return this.http.put<DemoSandboxSnapshot>(`/api/admin/demo/scenarios/${scenarioKey}/sandbox/scenario`, value); }
}

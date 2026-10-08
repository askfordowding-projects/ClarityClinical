import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, switchMap, tap } from 'rxjs';

export type DemoRole = 'Clinician' | 'Administrator';

interface DemoConsultationSession {
  id: string;
  scenarioId: string;
  consultationId: string;
  expiresAt: string;
}

export interface CurrentUser {
  displayName: string;
  role: DemoRole;
  isDemo: boolean;
}

@Injectable({ providedIn: 'root' })
export class DemoSessionService {
  private readonly http = inject(HttpClient);
  private currentUser: CurrentUser | null = null;

  get user(): CurrentUser | null {
    return this.currentUser;
  }

  startScenario(scenarioKey: string): Observable<string> {
    return this.http.post<DemoConsultationSession>('/api/demo/sessions', { scenarioKey }).pipe(
      switchMap(session => this.http.post(`/api/consultations/${session.consultationId}/start`, null).pipe(
        map(() => session.consultationId)
      ))
    );
  }
  login(role: DemoRole): Observable<CurrentUser> {
    return this.http.post<CurrentUser>('/api/auth/demo-login', { role }).pipe(
      tap(user => this.currentUser = user)
    );
  }
}

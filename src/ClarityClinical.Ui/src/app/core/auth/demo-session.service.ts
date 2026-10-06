import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export type DemoRole = 'Clinician' | 'Administrator';

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

  login(role: DemoRole): Observable<CurrentUser> {
    return this.http.post<CurrentUser>('/api/auth/demo-login', { role }).pipe(
      tap(user => this.currentUser = user)
    );
  }
}

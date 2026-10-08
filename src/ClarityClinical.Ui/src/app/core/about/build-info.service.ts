import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface BuildInfo {
  version: string;
  commit: string;
  environment: string;
  latestMigration: string;
  ruleSetVersion: string;
  buildTimestamp: string | null;
  isDemo: boolean;
}

@Injectable({ providedIn: 'root' })
export class BuildInfoService {
  private readonly http = inject(HttpClient);
  getBuild(): Observable<BuildInfo> { return this.http.get<BuildInfo>('/api/system/build'); }
}

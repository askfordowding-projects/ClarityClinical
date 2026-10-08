import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminAuditPage, ClinicalRuleVersion, GuidelineSource } from './admin-governance.models';

@Injectable({ providedIn: 'root' })
export class AdminGovernanceService {
  private readonly http = inject(HttpClient);

  getGuidelines(): Observable<GuidelineSource[]> {
    return this.http.get<GuidelineSource[]>('/api/admin/governance/guidelines');
  }

  getRules(): Observable<ClinicalRuleVersion[]> {
    return this.http.get<ClinicalRuleVersion[]>('/api/admin/governance/rules');
  }

  getAudit(): Observable<AdminAuditPage> {
    return this.http.get<AdminAuditPage>('/api/admin/audit');
  }
}

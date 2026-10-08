export interface GuidelineSource {
  referenceCode: string;
  organisation: string;
  title: string;
  url: string | null;
  publishedDate: string;
  lastUpdatedDate: string | null;
  lastReviewedDate: string | null;
  reviewStatus: string;
}

export interface ClinicalRuleVersion {
  ruleKey: string;
  displayName: string;
  version: string;
  status: string;
  scoreType: string;
  isIllustrative: boolean;
  provenanceNote: string;
  effectiveFrom: string;
  source: GuidelineSource;
}

export interface AdminAuditEvent {
  id: string;
  consultationId: string;
  actorRole: string;
  action: string;
  entityType: string;
  entityId: string;
  occurredAt: string;
  correlationId: string;
  previousState: string | null;
  newState: string | null;
  reason: string | null;
  ruleVersion: string | null;
}

export interface AdminAuditPage {
  totalCount: number;
  items: AdminAuditEvent[];
}

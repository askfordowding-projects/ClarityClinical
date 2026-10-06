export interface NamedClinicalValue {
  code: string;
  displayName: string;
}

export interface PatientContext {
  id: string;
  displayName: string;
  age: number;
  conditions: NamedClinicalValue[];
  allergies: NamedClinicalValue[];
  medications: NamedClinicalValue[];
}

export interface TranscriptEntry {
  id: string;
  speakerRole: string;
  originalLanguage: string;
  originalText: string;
  translatedText: string | null;
  corrected: boolean;
}

export interface ClinicalEvidence {
  factCode: string;
  displayText: string;
  source: string;
}

export interface AssessmentCandidate {
  key: string;
  displayName: string;
  currentScore: number;
  previousScore: number | null;
  scoreType: string;
  supportingEvidence: ClinicalEvidence[];
  missingEvidence: string[];
  suggestedChecks: string[];
  warnings: string[];
  changeReason: string | null;
}

export interface WorkspacePermissions {
  canRecordObservation: boolean;
  canAddPatientReport: boolean;
  canCompleteConsultation: boolean;
}

export interface ClinicalWorkspace {
  consultation: {
    id: string;
    status: string;
  };
  patient: PatientContext;
  transcript: TranscriptEntry[];
  assessments: AssessmentCandidate[];
  warnings: string[];
  suggestedChecks: string[];
  permissions: WorkspacePermissions;
}

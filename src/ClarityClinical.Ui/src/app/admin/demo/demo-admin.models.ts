export interface DemoClinicalValue { code: string; displayName: string; }
export interface DemoPatient {
  id: string; displayName: string; dateOfBirth: string; isCanonical: boolean;
  conditions: DemoClinicalValue[]; allergies: DemoClinicalValue[]; medications: DemoClinicalValue[];
}
export interface DemoScenario {
  id: string; scenarioKey: string; name: string; setting: string; sourceLanguage: string; clinicianLanguage: string; isCanonical: boolean; stepCount: number;
}

export interface DemoSandboxValue { code: string; displayName: string; }
export interface DemoSandboxPatient { id: string; givenName: string; familyName: string; displayName: string; dateOfBirth: string; conditions: DemoSandboxValue[]; allergies: DemoSandboxValue[]; medications: DemoSandboxValue[]; }
export interface DemoSandboxScenario { id: string; scenarioKey: string; name: string; description: string; setting: string; sourceLanguage: string; clinicianLanguage: string; isCanonical: boolean; stepCount: number; }
export interface DemoSandboxSnapshot { id: string; sourceScenarioId: string; patient: DemoSandboxPatient; scenario: DemoSandboxScenario; }

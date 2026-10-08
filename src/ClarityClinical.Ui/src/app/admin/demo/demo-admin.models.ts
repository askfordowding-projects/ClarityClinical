export interface DemoClinicalValue { code: string; displayName: string; }
export interface DemoPatient {
  id: string; displayName: string; dateOfBirth: string; isCanonical: boolean;
  conditions: DemoClinicalValue[]; allergies: DemoClinicalValue[]; medications: DemoClinicalValue[];
}
export interface DemoScenario {
  id: string; scenarioKey: string; name: string; setting: string; sourceLanguage: string; clinicianLanguage: string; isCanonical: boolean; stepCount: number;
}

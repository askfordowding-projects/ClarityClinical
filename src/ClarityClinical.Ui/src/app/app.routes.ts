import { Routes } from '@angular/router';
import { DemoLoginComponent } from './auth/demo-login/demo-login.component';
import { ClinicalWorkspaceComponent } from './clinical/workspace/clinical-workspace.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: DemoLoginComponent },
  { path: 'about', loadComponent: () => import('./core/about/about-build.component').then(module => module.AboutBuildComponent) },
  { path: 'clinical/consultations/:id', component: ClinicalWorkspaceComponent },
  {
    path: 'clinical/consultations',
    loadComponent: () => import('./clinical/consultations/consultation-list.component')
      .then(module => module.ConsultationListComponent)
  },
  {
    path: 'admin',
    loadComponent: () => import('./admin/dashboard/admin-dashboard.component')
      .then(module => module.AdminDashboardComponent)
  },
  { path: 'admin/patients', loadComponent: () => import('./admin/demo/patients.component').then(module => module.PatientsComponent) },
  { path: 'admin/scenarios/:scenarioKey', loadComponent: () => import('./admin/demo/sandbox-editor.component').then(module => module.SandboxEditorComponent) },
  { path: 'admin/scenarios', loadComponent: () => import('./admin/demo/scenarios.component').then(module => module.ScenariosComponent) },
  { path: 'admin/guidelines', loadComponent: () => import('./admin/governance/guidelines.component').then(module => module.GuidelinesComponent) },
  { path: 'admin/clinical-rules', loadComponent: () => import('./admin/governance/clinical-rules.component').then(module => module.ClinicalRulesComponent) },
  { path: 'admin/audit', loadComponent: () => import('./admin/governance/audit.component').then(module => module.AuditComponent) },
  { path: '**', redirectTo: 'login' }
];

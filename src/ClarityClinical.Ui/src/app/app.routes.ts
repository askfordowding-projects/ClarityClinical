import { Routes } from '@angular/router';
import { DemoLoginComponent } from './auth/demo-login/demo-login.component';
import { ClinicalWorkspaceComponent } from './clinical/workspace/clinical-workspace.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: DemoLoginComponent },
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
  { path: 'admin/guidelines', loadComponent: () => import('./admin/governance/guidelines.component').then(module => module.GuidelinesComponent) },
  { path: 'admin/clinical-rules', loadComponent: () => import('./admin/governance/clinical-rules.component').then(module => module.ClinicalRulesComponent) },
  { path: 'admin/audit', loadComponent: () => import('./admin/governance/audit.component').then(module => module.AuditComponent) },
  { path: '**', redirectTo: 'login' }
];

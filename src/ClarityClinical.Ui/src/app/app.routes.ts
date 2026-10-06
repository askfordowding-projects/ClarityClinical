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
  { path: '**', redirectTo: 'login' }
];

import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'feriados' },
  {
    path: 'feriados',
    loadComponent: () =>
      import('./features/feriados/feriados-page').then((m) => m.FeriadosPage),
  },
  { path: '**', redirectTo: 'feriados' },
];

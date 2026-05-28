import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./features/products/routes').then((m) => m.PRODUCTS_ROUTES),
  },
  {
    path: '**',
    redirectTo: '',
  },
];

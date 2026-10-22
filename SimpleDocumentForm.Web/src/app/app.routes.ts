import { Routes } from '@angular/router';

import { FormDetail } from './forms/form-detail/form-detail';
import { FormList } from './forms/form-list/form-list';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'forms' },
  { path: 'forms', component: FormList, title: 'Document forms' },
  { path: 'forms/:id', component: FormDetail, title: 'Document form' },
  { path: '**', redirectTo: 'forms' },
];

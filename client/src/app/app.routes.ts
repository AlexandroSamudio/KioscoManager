import { Routes } from '@angular/router';
import { HomeComponent } from './_components/home/home.component';
import { RegisterComponent } from './_components/register/register.component';

export const routes: Routes = [
  {path: '', component: HomeComponent},
  {path: 'register', component: RegisterComponent},
  {path: '**', component: HomeComponent},
];

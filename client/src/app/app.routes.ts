import { Routes } from '@angular/router';
import { HomeComponent } from './_components/home/home.component';
import { RegisterComponent } from './_components/register/register.component';
import { LoginComponent } from './_components/login/login.component';
import { BienvenidaComponent } from './_components/bienvenida/bienvenida.component';
import { authGuard } from './_guards/auth.guard';
import { CrearKioscoComponent } from './_components/crear-kiosco/crear-kiosco.component';

export const routes: Routes = [
  {path: '', component: HomeComponent},
  {path: 'bienvenida', component: BienvenidaComponent, canActivate: [authGuard]},
  {path: 'register', component: RegisterComponent},
  {path: 'crear-kiosco', component: CrearKioscoComponent, canActivate:[authGuard]},
  {path: 'login', component: LoginComponent},
  {path: '**', component: HomeComponent},
];

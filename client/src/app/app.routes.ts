import { Routes } from '@angular/router';
import { HomeComponent } from './_components/home/home.component';
import { RegisterComponent } from './_components/register/register.component';
import { LoginComponent } from './_components/login/login.component';
import { BienvenidaComponent } from './_components/bienvenida/bienvenida.component';
import { authGuard } from './_guards/auth.guard';
import { CrearKioscoComponent } from './_components/crear-kiosco/crear-kiosco.component';
import { DashboardComponent } from './_components/dashboard/dashboard.component';
import { InventarioComponent } from './_components/inventario/inventario.component';
import { PuntoVentaComponent } from './_components/ventas/punto-venta.component';
import { RegistrarCompraComponent } from './_components/compras/registrar-compra.component';

export const routes: Routes = [
  {path: '', component: HomeComponent},
  {path: 'bienvenida', component: BienvenidaComponent, canActivate: [authGuard]},
  {path: 'register', component: RegisterComponent},
  {path: 'dashboard', component: DashboardComponent, canActivate: [authGuard]},
  {path: 'crear-kiosco', component: CrearKioscoComponent, canActivate: [authGuard]},
  {path: 'inventario', component: InventarioComponent, canActivate: [authGuard]},
  {path: 'punto-de-venta', component: PuntoVentaComponent, canActivate: [authGuard]},
  {path: 'registrar-compra', component: RegistrarCompraComponent, canActivate: [authGuard]},
  {path: 'login', component: LoginComponent},
  {path: '**', component: HomeComponent},
];

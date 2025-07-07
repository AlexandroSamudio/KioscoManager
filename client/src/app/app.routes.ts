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
import { ReportesPageComponent } from './_components/reportes/reportes-page.component';
import { ConfiguracionComponent } from './_components/configuracion/configuracion.component';
import { PerfilPersonalComponent } from './_components/configuracion/perfil-personal/perfil-personal.component';
import { InfoNegocioComponent } from './_components/info-negocio/info-negocio.component';
import { UsuariosPermisosComponent } from './_components/configuracion/usuarios-permisos/usuarios-permisos.component';
import { ConfiguracionCategoriasComponent } from './_components/configuracion/configuracion-categorias/configuracion-categorias.component';
// import { ConfiguracionReportesComponent } from './_components/configuracion/configuracion-reportes/configuracion-reportes.component';

export const routes: Routes = [
  {path: '', component: HomeComponent},
  {path: 'bienvenida', component: BienvenidaComponent, canActivate: [authGuard]},
  {path: 'register', component: RegisterComponent},
  {path: 'dashboard', component: DashboardComponent, canActivate: [authGuard]},
  {path: 'crear-kiosco', component: CrearKioscoComponent, canActivate: [authGuard]},
  {path: 'inventario', component: InventarioComponent, canActivate: [authGuard]},
  {path: 'punto-de-venta', component: PuntoVentaComponent, canActivate: [authGuard]},
  {path: 'registrar-compra', component: RegistrarCompraComponent, canActivate: [authGuard]},
  {path: 'reportes', component: ReportesPageComponent, canActivate: [authGuard]},
  {
    path: 'configuracion',
    component: ConfiguracionComponent,
    canActivate: [authGuard],
    children: [
      {path: '', redirectTo: 'perfil', pathMatch: 'full'},
      {path: 'perfil', component: PerfilPersonalComponent},
      {path: 'negocio', component: InfoNegocioComponent},
      {path: 'usuarios', component: UsuariosPermisosComponent},
      {path: 'categorias', component: ConfiguracionCategoriasComponent},
      // {path: 'reportes', component: ConfiguracionReportesComponent}
    ]
  },
  {path: 'login', component: LoginComponent},
  {path: '**', component: HomeComponent},
];

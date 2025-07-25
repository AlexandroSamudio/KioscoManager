import { Routes } from '@angular/router';
import { authGuard } from './_guards/auth.guard';
import { adminGuard } from './_guards/admin.guard';
import { authRedirectGuard } from './_guards/auth-redirect.guard';

export const routes: Routes = [
  {
    path: '',
    canActivate: [authRedirectGuard],
    children: []
  },
  // Rutas públicas
  {
    path: 'home',
    loadComponent: () => import('./_components/home/home.component').then(m => m.HomeComponent),
  },
  {
    path: 'login',
    loadComponent: () => import('./_components/login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./_components/register/register.component').then(m => m.RegisterComponent),
  },

  // Rutas protegidas
  {
    path: 'bienvenida',
    loadComponent: () => import('./_components/bienvenida/bienvenida.component').then(m => m.BienvenidaComponent),
    canActivate: [authGuard]
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./_components/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'crear-kiosco',
    loadComponent: () => import('./_components/crear-kiosco/crear-kiosco.component').then(m => m.CrearKioscoComponent),
    canActivate: [authGuard]
  },
  {
    path: 'inventario',
    loadComponent: () => import('./_components/inventario/inventario.component').then(m => m.InventarioComponent),
    canActivate: [authGuard]
  },
  {
    path: 'punto-de-venta',
    loadComponent: () => import('./_components/ventas/punto-venta.component').then(m => m.PuntoVentaComponent),
    canActivate: [authGuard]
  },
  {
    path: 'registrar-compra',
    loadComponent: () => import('./_components/compras/registrar-compra.component').then(m => m.RegistrarCompraComponent),
    canActivate: [authGuard]
  },
  {
    path: 'reportes',
    loadComponent: () => import('./_components/reportes/reportes-page.component').then(m => m.ReportesPageComponent),
    canActivate: [authGuard]
  },
  {
    path: 'configuracion',
    loadComponent: () => import('./_components/configuracion/configuracion.component').then(m => m.ConfiguracionComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'perfil',
        pathMatch: 'full'
      },
      {
        path: 'perfil',
        loadComponent: () => import('./_components/configuracion/perfil-personal/perfil-personal.component').then(m => m.PerfilPersonalComponent)
      },
      {
        path: 'negocio',
        loadComponent: () => import('./_components/info-negocio/info-negocio.component').then(m => m.InfoNegocioComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'usuarios',
        loadComponent: () => import('./_components/configuracion/usuarios-permisos/usuarios-permisos.component').then(m => m.UsuariosPermisosComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'categorias',
        loadComponent: () => import('./_components/configuracion/configuracion-categorias/configuracion-categorias.component').then(m => m.ConfiguracionCategoriasComponent),
        canActivate: [adminGuard]
      },
      {
        path: 'reportes',
        loadComponent: () => import('./_components/configuracion/configuracion-reportes/configuracion-reportes.component').then(m => m.ConfiguracionReportesComponent),
        canActivate: [adminGuard]
      }
    ]
  },

  {
    path: '**',
    canActivate: [authRedirectGuard],
    children: []
  }
];

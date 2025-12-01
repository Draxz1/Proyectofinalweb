import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login';
import { AppLayoutComponent } from './components/app-layout/app-layout';

// 1. IMPORTAR PAGINAS PÚBLICAS FALTANTES
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password';
import { ChangePasswordComponent } from './pages/change-password/change-password';

// 2. IMPORTAR PAGINAS INTERNAS
import { DashboardComponent } from './pages/dashboard/dashboard';
import { MeseroPanelComponent } from './pages/mesero-panel/mesero-panel';
import { CocinaPanelComponent } from './pages/cocina-panel/cocina-panel';
import { CajaPanelComponent } from './pages/caja-panel/caja-panel';
import { InventarioPanelComponent } from './pages/inventario-panel/inventario-panel';
import { AdminUsersComponent } from './pages/admin-users/admin-users';
import { ContabilidadComponent } from './pages/contabilidad/contabilidad';

// Guards
import { authGuard } from './guards/auth-guard';
import { roleGuard } from './guards/role.guard';

export const routes: Routes = [
  
  // ==========================================
  // ZONA PÚBLICA (Sin Sidebar ni Topbar)
  // ==========================================
  { path: 'login', component: LoginComponent },
  
  // 👇 AQUÍ ESTABAN FALTANDO ESTAS RUTAS 👇
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'change-password', component: ChangePasswordComponent },


  // ==========================================
  // ZONA PRIVADA (Con Sidebar y Topbar)
  // ==========================================
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [authGuard], // Candado general de sesión
    children: [
      
      // ADMIN
      { 
        path: 'dashboard', 
        component: DashboardComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['admin'] } 
      },
      { 
        path: 'contabilidad', 
        component: ContabilidadComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['admin'] } 
      },
      { 
        path: 'admin', 
        component: AdminUsersComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['admin'] } 
      },

      // MESERO
      { 
        path: 'mesero-panel', 
        component: MeseroPanelComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['mesero'] } 
      },

      // COCINA
      { 
        path: 'cocina-panel', 
        component: CocinaPanelComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['cocinero', 'cocina'] } 
      },
      { 
        path: 'inventario', 
        component: InventarioPanelComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['cocinero', 'cocina'] } 
      },

      // CAJA
      { 
        path: 'caja-panel', 
        component: CajaPanelComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['cajero', 'caja'] } 
      },

      // Redirección por defecto si entra a la raíz vacía
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },

  // Ruta comodín (Error 404 -> Login)
  { path: '**', redirectTo: 'login' }
];
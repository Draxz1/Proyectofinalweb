import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login';
import { AppLayoutComponent } from './components/app-layout/app-layout';

// 1. IMPORTAMOS TODOS LOS COMPONENTES (Asegúrate que estas rutas sean reales)
import { DashboardComponent } from './pages/dashboard/dashboard';
import { MeseroPanelComponent } from './pages/mesero-panel/mesero-panel';
import { CocinaPanelComponent } from './pages/cocina-panel/cocina-panel';
import { CajaPanelComponent } from './pages/caja-panel/caja-panel';
import { InventarioPanelComponent } from './pages/inventario-panel/inventario-panel';
import { AdminUsersComponent } from './pages/admin-users/admin-users';

// 👇👇 AQUÍ ESTABA EL FALTANTE 👇👇
import { ContabilidadComponent } from './pages/contabilidad/contabilidad'; 
// (Si te marca error aquí, verifica que la carpeta se llame 'contabilidad' y el archivo 'contabilidad.ts')

// Guards
import { authGuard } from './guards/auth-guard';
import { roleGuard } from './guards/role.guard';

export const routes: Routes = [
  // 1. Login (Pantalla Completa)
  { path: 'login', component: LoginComponent },

  // 2. App Principal (Con Sidebar y Topbar)
  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [authGuard], // Verifica que haya sesión
    children: [
      
      // --- DASHBOARD ---
      { 
        path: 'dashboard', 
        component: DashboardComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['admin'] } 
      },

      // --- CONTABILIDAD (NUEVO) ---
      { 
        path: 'contabilidad', 
        component: ContabilidadComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['admin'] } 
      },

      // --- ADMIN USUARIOS ---
      { 
        path: 'admin', 
        component: AdminUsersComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['admin'] } 
      },

      // --- MESERO ---
      { 
        path: 'mesero-panel', 
        component: MeseroPanelComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['mesero'] } 
      },

      // --- COCINA ---
      { 
        path: 'cocina-panel', 
        component: CocinaPanelComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['cocinero', 'cocina'] } 
      },
      
      // --- INVENTARIO ---
      { 
        path: 'inventario', 
        component: InventarioPanelComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['cocinero', 'cocina'] } 
      },

      // --- CAJA ---
      { 
        path: 'caja-panel', 
        component: CajaPanelComponent, 
        canActivate: [roleGuard], 
        data: { roles: ['cajero', 'caja'] } 
      },

      // Redirección por defecto
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },

  // Ruta comodín (Cualquier cosa rara va al login)
  { path: '**', redirectTo: 'login' }
];
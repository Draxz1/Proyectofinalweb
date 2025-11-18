import { Routes } from '@angular/router';

// --- Tarea de Janeli (Completada) ---
// Importamos el Layout que ella creó.
import { AppLayoutComponent } from './components/app-layout/app-layout.component';

// --- Tarea de Jared (En progreso) ---
// (Asumimos que Jared creará estos componentes en 'pages/login' y 'pages/register')
// Si aún no existen, puedes comentarlos temporalmente.
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';

// --- Tarea de Diego (En progreso) ---
// (Asumimos que Diego creará este guard en 'guards/auth.guard')
// Si aún no existe, puedes comentar la línea 'canActivate' por ahora.
import { authGuard } from './guards/auth.guard';

// --- Tarea de Rene (Los componentes que acabas de crear) ---
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { CajaPanelComponent } from './pages/caja-panel/caja-panel.component';
import { MeseroPanelComponent } from './pages/mesero-panel/mesero-panel.ts';
import { CocinaPanelComponent } from './pages/cocina-panel/cocina-panel.component';
import { InventarioPanelComponent } from './pages/inventario-panel/inventario-panel.component';
import { AdminUsersComponent } from './pages/admin-users/admin-users.component';
import { ContabilidadComponent } from './pages/contabilidad/contabilidad.component';


export const routes: Routes = [

  // --- Rutas Públicas (Sin Sidebar, sin Layout) ---
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },

  // --- Rutas Privadas (Usan el Layout y Sidebar de Janeli) ---
  {
    path: '', // Esto hace que todas las rutas "hijas" usen este layout
    component: AppLayoutComponent,
    canActivate: [authGuard], // <-- Tarea de Diego: Protege estas rutas
    children: [
      // Redirige la ruta vacía (ej. localhost:4200/) a /dashboard
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

      { path: 'dashboard', component: DashboardComponent },
      { path: 'caja', component: CajaPanelComponent },
      { path: 'mesero', component: MeseroPanelComponent },
      { path: 'cocina', component: CocinaPanelComponent },
      { path: 'inventario', component: InventarioPanelComponent },
      { path: 'admin', component: AdminUsersComponent },
      { path: 'contabilidad', component: ContabilidadComponent },

      // (Aquí se agregarán las sub-rutas de contabilidad después)
    ]
  },

  // Si la URL no coincide con nada, redirige al login
  { path: '**', redirectTo: 'login' }
];

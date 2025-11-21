import { Routes } from '@angular/router';

// Componentes Principales
import { LoginComponent } from './pages/login/login';
import { RegisterComponent } from './pages/register/register';
import { AppLayoutComponent } from './components/app-layout/app-layout';

// --- IMPORTA EL COMPONENTE DE RECUPERACIÓN ---
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password'; // <-- Asegúrate que la ruta sea correcta

// Paneles
import { DashboardComponent } from './pages/dashboard/dashboard'; 
// ... (tus otras importaciones de Admin, Mesero, etc.) ...
import { AdminUsersComponent } from './pages/admin-users/admin-users';
import { MeseroPanelComponent } from './pages/mesero-panel/mesero-panel';
import { CocinaPanelComponent } from './pages/cocina-panel/cocina-panel';
import { CajaPanelComponent } from './pages/caja-panel/caja-panel';
import { InventarioPanelComponent } from './pages/inventario-panel/inventario-panel';
import { ContabilidadComponent } from './pages/contabilidad/contabilidad';
import { ChangePasswordComponent } from './pages/change-password/change-password';

// Guard
import { authGuard } from './guards/auth-guard'; 


export const routes: Routes = [

    // Rutas Públicas 
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    
    // --- AGREGA ESTA LÍNEA ---
    { path: 'forgot-password', component: ForgotPasswordComponent }, // <-- Aquí está la magia
    { path: 'change-password', component: ChangePasswordComponent },
    
    // Rutas Privadas
    {
        path: '', 
        component: AppLayoutComponent,
        canActivate: [authGuard], 
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }, 
            { path: 'dashboard', component: DashboardComponent },
            // ... (resto de tus rutas protegidas) ...
             { path: 'admin', component: AdminUsersComponent },
            { path: 'mesero', component: MeseroPanelComponent },
            { path: 'cocina', component: CocinaPanelComponent },
            { path: 'caja', component: CajaPanelComponent },
            { path: 'inventario', component: InventarioPanelComponent },

            { path: 'contabilidad', component: ContabilidadComponent },
        ]
    },

    { path: '**', redirectTo: 'login' }
];
import { Routes } from '@angular/router';

// Componentes Principales (FIX: Importación con nombres de archivo simples)
import { LoginComponent } from './pages/login/login'; 
import { RegisterComponent } from './pages/register/register'; 
import { AppLayoutComponent } from './components/app-layout/app-layout'; 

// Paneles (Rutas dentro del Layout)
import { DashboardComponent } from './pages/dashboard/dashboard'; 
import { AdminUsersComponent } from './pages/admin-users/admin-users'; 
import { MeseroPanelComponent } from './pages/mesero-panel/mesero-panel';
import { CocinaPanelComponent } from './pages/cocina-panel/cocina-panel';
import { CajaPanelComponent } from './pages/caja-panel/caja-panel';
import { InventarioPanelComponent } from './pages/inventario-panel/inventario-panel';
import { ContabilidadComponent } from './pages/contabilidad/contabilidad';

// Guard de Diego
import { authGuard } from './guards/auth-guard'; 


export const routes: Routes = [

    // Rutas Públicas 
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },

    // Rutas Privadas (Con Layout/Sidebar)
    {
        path: '', 
        component: AppLayoutComponent,
        canActivate: [authGuard], 
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }, 
            
            { path: 'dashboard', component: DashboardComponent },
            { path: 'admin', component: AdminUsersComponent },
            { path: 'mesero', component: MeseroPanelComponent },
            { path: 'cocina', component: CocinaPanelComponent },
            { path: 'caja', component: CajaPanelComponent },
            { path: 'inventario', component: InventarioPanelComponent },

            { path: 'contabilidad', component: ContabilidadComponent },
        ]
    },

    // Redirección General
    { path: '**', redirectTo: 'login' }
];
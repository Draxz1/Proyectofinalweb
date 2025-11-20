import { Routes } from '@angular/router';


import { LoginComponent } from './pages/login/login'; 
import { RegisterComponent } from './pages/register/register'; 
import { AppLayoutComponent } from './components/app-layout/app-layout'; 


import { DashboardComponent } from './pages/dashboard/dashboard'; 
import { AdminUsersComponent } from './pages/admin-users/admin-users'; 
import { MeseroPanelComponent } from './pages/mesero-panel/mesero-panel';
import { CocinaPanelComponent } from './pages/cocina-panel/cocina-panel';
import { CajaPanelComponent } from './pages/caja-panel/caja-panel';
import { InventarioPanelComponent } from './pages/inventario-panel/inventario-panel';
import { ContabilidadComponent } from './pages/contabilidad/contabilidad';


import { authGuard } from './guards/auth-guard'; 


export const routes: Routes = [

     
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },

    
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

    
    { path: '**', redirectTo: 'login' }
];
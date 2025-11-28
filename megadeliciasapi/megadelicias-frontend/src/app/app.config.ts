import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient, withFetch } from '@angular/common/http';

// 1. IMPORTAMOS LOS ICONOS Y EL MÓDULO AQUÍ
import { 
  LucideAngularModule, 
  Search, Trash2, ShoppingCart, PlusCircle, Bell, BellRing, CheckCircle, Coffee 
} from 'lucide-angular';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withFetch()), // Mantenemos tu configuración de HTTP
    
    // 2. REGISTRAMOS LOS ICONOS GLOBALMENTE
    // Esto hace que estén disponibles en cualquier componente que importe LucideAngularModule
    importProvidersFrom(LucideAngularModule.pick({ 
      Search, 
      Trash2, 
      ShoppingCart, 
      PlusCircle, 
      Bell, 
      BellRing, 
      CheckCircle, 
      Coffee 
    }))
  ]
};
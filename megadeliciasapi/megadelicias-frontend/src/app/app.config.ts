import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient, withFetch } from '@angular/common/http';

// 1. IMPORTAMOS TODOS LOS ICONOS QUE USA LA APP
import { 
  LucideAngularModule, 
  // Iconos del Sidebar
  LayoutDashboard, 
  Users, 
  ChefHat, 
  Archive, 
  DollarSign, 
  LogOut, 
  UtensilsCrossed,
  
  // Iconos del Dashboard (Módulos)
  Calculator, 
  Utensils, 
  Package, 
  Clipboard, 
  
  // Iconos de Mesero y Cocina
  Search, 
  Trash2, 
  ShoppingCart, 
  PlusCircle, 
  Bell, 
  BellRing, 
  CheckCircle, 
  Coffee 
} from 'lucide-angular';

export const appConfig: ApplicationConfig = {
  providers: [
    // Proveedor de Rutas
    provideRouter(routes),
    
    // Proveedor HTTP (necesario para conectar con .NET)
    provideHttpClient(withFetch()), 
    
    // 2. REGISTRO GLOBAL DE ICONOS
    // Al hacer esto aquí, ya no necesitas importarlos en cada componente
    importProvidersFrom(LucideAngularModule.pick({ 
      LayoutDashboard, 
      Users, 
      ChefHat, 
      Archive, 
      DollarSign, 
      LogOut, 
      UtensilsCrossed,
      Calculator, 
      Utensils, 
      Package, 
      Clipboard,
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
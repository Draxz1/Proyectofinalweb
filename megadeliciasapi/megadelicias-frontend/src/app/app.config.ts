import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';

import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';

// --- TAREAS DE JARED ---
import { FormsModule } from '@angular/forms'; // <-- 1. Importa FormsModule
import { provideHttpClient } from '@angular/common/http'; // <-- 2. Importa provideHttpClient

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    
    provideHttpClient(),             // <-- 3. Añade esto para llamadas API
    importProvidersFrom(FormsModule) // <-- 4. Añade esto para formularios (ngModel)
  ]
};

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes)
  ]
};

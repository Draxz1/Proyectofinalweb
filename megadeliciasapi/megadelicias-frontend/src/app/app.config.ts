import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';

import { FormsModule } from '@angular/forms'; // <-- 1. Importa FormsModule
import { provideHttpClient } from '@angular/common/http'; // <-- 2. Importa provideHttpClient

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),

    // --- AÑADE ESTAS DOS LÍNEAS ---
    provideHttpClient(),             // <-- 3. Añade esto para llamadas API
    importProvidersFrom(FormsModule) // <-- 4. Añade esto para formularios (ngModel)
  ]
};
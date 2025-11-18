import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app'; // <-- FIX: Importa AppComponent

bootstrapApplication(AppComponent, appConfig) // <-- FIX: Usa AppComponent
  .catch((err) => console.error(err));
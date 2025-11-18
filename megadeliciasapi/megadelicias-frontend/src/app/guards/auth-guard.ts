import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth'; // <-- Importa tu servicio

export const authGuard: CanActivateFn = (route, state) => {

  // Inyectamos el servicio y el router
  const authService = inject(AuthService);
  const router = inject(Router);

  // Usamos el método que creamos en el servicio
  if (authService.isAuthenticated()) {
    return true; // El usuario está logueado, permite el acceso
  } else {
    // El usuario no está logueado, redirige al login
    router.navigate(['/login']);
    return false;
  }
};
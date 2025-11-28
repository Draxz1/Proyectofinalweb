import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // 1. Primero validamos si el usuario está logueado
  if (!authService.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  // 2. Obtenemos el rol del usuario actual desde el token
  const user = authService.getUserData();
  const userRole = user?.rol?.toLowerCase() || '';

  // 3. Leemos qué roles están permitidos para esta ruta específica
  // (Esta información viene del archivo app.routes.ts donde pusimos "data: { roles: [...] }")
  const allowedRoles = route.data['roles'] as Array<string>;

  // 4. VALIDACIÓN PRINCIPAL
  // Si el usuario es 'admin', le dejamos pasar a todo (Superusuario)
  // O si su rol está en la lista de permitidos, también pasa.
  if (userRole === 'admin' || (allowedRoles && allowedRoles.includes(userRole))) {
    return true;
  }

  // 5. SI NO TIENE PERMISO (Redirección Inteligente)
  // En lugar de dejarlo en una página en blanco, lo mandamos a SU panel correspondiente.
  if (userRole === 'mesero') {
    router.navigate(['/mesero-panel']);
  } else if (userRole === 'cocinero' || userRole === 'cocina') {
    router.navigate(['/cocina-panel']);
  } else if (userRole === 'cajero' || userRole === 'caja') {
    router.navigate(['/caja-panel']);
  } else {
    // Si el rol es desconocido, lo mandamos al login
    router.navigate(['/login']);
  }

  return false;
};
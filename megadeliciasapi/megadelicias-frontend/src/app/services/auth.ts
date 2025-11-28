import { Injectable, inject } from '@angular/core'; 
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode'; 
import { BehaviorSubject } from 'rxjs'; // <--- IMPORTANTE

@Injectable({
  providedIn: 'root'
})
export class AuthService { 

  private router = inject(Router);

  // 1. Creamos la "Antena" (BehaviorSubject) que guardará el estado actual
  private currentUserSubject = new BehaviorSubject<any>(null);
  
  // 2. Creamos una señal pública para que el Sidebar se pueda suscribir
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    // Al iniciar la app, intentamos cargar el usuario si ya hay token guardado
    this.currentUserSubject.next(this.getUserData());
  }

  saveToken(token: string): void {
    localStorage.setItem('token', token);
    
    // 3. ¡AVISAR A TODOS! Cuando guardamos token, emitimos el nuevo usuario
    this.currentUserSubject.next(this.getUserData());
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  logout(): void {
    localStorage.removeItem('token');
    
    // 4. AVISAR A TODOS que se cerró sesión (emitimos null)
    this.currentUserSubject.next(null);
    
    this.router.navigate(['/login']);
  }

  // Este método sigue igual, es el que lee el token
  getUserData(): { id: string, nombre: string, rol: string } | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const decoded: any = jwtDecode(token);
      
      const roleClaim = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded['role'];
      const rolFinal = Array.isArray(roleClaim) ? roleClaim[0] : roleClaim;

      return {
        id: decoded.sub || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
        nombre: decoded.name || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
        rol: rolFinal || '' 
      };
    } catch (error) {
      console.error("Error token:", error);
      return null;
    }
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const decoded: any = jwtDecode(token);
      if (decoded.exp * 1000 < Date.now()) {
        this.logout();
        return false;
      }
      return true;
    } catch (error) {
      return false;
    }
  }
}
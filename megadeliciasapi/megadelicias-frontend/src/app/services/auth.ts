import { Injectable, inject } from '@angular/core'; 
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode'; 

@Injectable({
  providedIn: 'root'
})
export class AuthService { 

  constructor(private router: Router) { }

  saveToken(token: string): void {
    localStorage.setItem('token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  logout(): void {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  getUserData(): { id: string, nombre: string, rol: string } | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const decoded: any = jwtDecode(token);
      return {
        id: decoded.sub,
        nombre: decoded.name,
        rol: decoded.role 
      };
    } catch (error) {
      console.error("Error al decodificar el token", error);
      this.logout();
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
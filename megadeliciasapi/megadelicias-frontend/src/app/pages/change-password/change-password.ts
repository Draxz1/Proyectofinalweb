import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './change-password.html',
  // Si usas CSS específico, descomenta: styleUrl: './change-password.css'
})
export class ChangePasswordComponent implements OnInit {
  
  correo: string = '';
  passwordTemporal: string = '';
  nuevoPassword: string = '';
  error: string = '';
  mensaje: string = '';
  cargando: boolean = false;

  private http = inject(HttpClient);
  private router = inject(Router);
  
  // URL de tu backend (puerto 7110 o 5143, revisa el tuyo)
  private apiUrl = 'http://localhost:5143/api/Auth/cambiar-password'; 

  ngOnInit() {
    // Recuperar el correo que guardamos al intentar loguearnos
    this.correo = localStorage.getItem('temp_email') || '';
    
    if (!this.correo) {
      // Si no hay correo, no debería estar aquí, volver al login
      this.router.navigate(['/login']);
    }
  }

  cambiar() {
    if (!this.passwordTemporal || !this.nuevoPassword) {
      this.error = 'Todos los campos son obligatorios';
      return;
    }

    this.cargando = true;
    this.error = '';
    this.mensaje = '';

    const payload = {
      correo: this.correo,
      passwordTemporal: this.passwordTemporal,
      nuevoPassword: this.nuevoPassword
    };

    this.http.post<any>(this.apiUrl, payload).subscribe({
      next: (res) => {
        this.mensaje = '✅ Contraseña actualizada exitosamente.';
        this.cargando = false;
        
        // Limpiar y redirigir al login después de 2 segundos
        localStorage.removeItem('temp_email');
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        this.cargando = false;
        this.error = err.error?.message || 'Error al cambiar la contraseña.';
      }
    });
  }
}
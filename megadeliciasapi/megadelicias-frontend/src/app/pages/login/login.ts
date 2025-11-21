import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth'; 

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html', 
  styleUrl: './login.css'
})
export class LoginComponent { 
  
  correo: string = '';
  password: string = '';
  error: string = '';

  http = inject(HttpClient);
  router = inject(Router);
  authService = inject(AuthService); 

  // Asegúrate de que este puerto (5143) sea el correcto de tu backend
  private apiUrl = 'http://localhost:5143/api/Auth/login'; 

  login() {
    this.error = ''; 
    const loginDto = { correo: this.correo, password: this.password };

    this.http.post<any>(this.apiUrl, loginDto).subscribe({
      next: (respuesta) => {
        
        // --- LÓGICA NUEVA: Verificar si requiere cambio de contraseña ---
        // Esto conecta con el código que pusimos en AuthController.cs
        if (respuesta.code === 'CHANGE_PASSWORD_REQUIRED') {
          // Guardamos el correo temporalmente para que la siguiente pantalla lo use
          localStorage.setItem('temp_email', this.correo);
          
          // Redirigimos a la pantalla de cambio de contraseña
          this.router.navigate(['/change-password']);
          return;
        }
        // -------------------------------------------------------------

        // Si no requiere cambio, es un Login normal
        this.authService.saveToken(respuesta.token);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        // Manejo de errores específico para contraseña temporal vencida
        if (err.error?.code === 'TEMP_EXPIRED') {
             this.error = 'Su contraseña temporal ha caducado. Por favor solicite una nueva.';
        } else {
             this.error = 'Credenciales inválidas. Por favor, inténtalo de nuevo.';
        }
        console.error('Error en el login:', err);
      }
    });
  }
}
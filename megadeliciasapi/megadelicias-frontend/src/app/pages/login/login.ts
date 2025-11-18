import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http'; // <-- Para llamar a la API
import { Router } from '@angular/router'; // <-- Para redirigir
import { AuthService } from '../../services/auth'; // <-- El servicio de Diego

// Importaciones necesarias para Standalone + Forms
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule], // <-- ¡Añadir CommonModule y FormsModule!
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  // --- Variables para el formulario ---
  correo: string = '';
  password: string = '';
  error: string = '';

  // --- Inyectar los servicios ---
  http = inject(HttpClient);
  router = inject(Router);
  authService = inject(AuthService); // Tarea de Diego

  // --- URL del Backend de Axel ---
  // (Asegúrate de que el puerto 7110 sea el de tu API)
  private apiUrl = 'https://localhost:7110/api/Auth/login'; 

  login() {
    this.error = ''; // Limpia errores anteriores

    const loginDto = {
      correo: this.correo,
      password: this.password
    };

    // 1. Llama a la API de .NET
    this.http.post<any>(this.apiUrl, loginDto).subscribe({
      next: (respuesta) => {
        // 2. Si la API responde OK, guarda el token (usando el servicio de Diego)
        this.authService.saveToken(respuesta.token);
        
        // 3. Redirige al Dashboard (usando el router de Rene)
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        // 4. Si la API da error (ej. 401 Unauthorized)
        this.error = 'Credenciales inválidas. Por favor, inténtalo de nuevo.';
        console.error('Error en el login:', err);
      }
    });
  }
}
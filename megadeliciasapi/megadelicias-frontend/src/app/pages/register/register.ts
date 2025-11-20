
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http'; // <-- Para llamar a la API
import { Router } from '@angular/router'; // <-- Para redirigir

// Importaciones necesarias para Standalone + Forms
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule], // <-- ¡Añadir CommonModule y FormsModule!
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  // --- Variables para el formulario ---
  nombre: string = '';
  correo: string = '';
  password: string = '';
  rol: string = 'mesero'; // Rol por defecto
  error: string = '';

  // --- Inyectar los servicios ---
  http = inject(HttpClient);
  router = inject(Router);

  // --- URL del Backend de Axel ---
  // (Asegúrate de que el puerto 7110 sea el de tu API)
  private apiUrl = 'http://localhost:5143/api/Auth/register'; 

  register() {
    this.error = ''; // Limpia errores anteriores

    const registerDto = {
      nombre: this.nombre,
      correo: this.correo,
      password: this.password,
      rol: this.rol
    };

    // 1. Llama a la API de .NET que Axel probó
    this.http.post(this.apiUrl, registerDto).subscribe({
      next: (respuesta) => {
        // 2. Si la API responde OK (201 Created)
        console.log('Usuario registrado:', respuesta);

        // 3. Redirige al Login
        this.router.navigate(['/login']);
      },
      error: (err) => {
        // 4. Si la API da error (ej. 400 Bad Request)
        this.error = 'Error al registrar. El correo ya podría existir.';
        console.error('Error en el registro:', err);
      }
    });
  }
}

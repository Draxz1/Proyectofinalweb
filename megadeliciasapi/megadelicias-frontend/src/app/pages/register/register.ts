
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http'; 
import { Router } from '@angular/router'; 


import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule], 
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  
  nombre: string = '';
  correo: string = '';
  password: string = '';
  rol: string = 'mesero'; 
  error: string = '';

  // Inyectar  servicios 
  http = inject(HttpClient);
  router = inject(Router);

  
  
  private apiUrl = 'https://localhost:7110/api/Auth/register'; 

  register() {
    this.error = ''; // Limpiar errores

    const registerDto = {
      nombre: this.nombre,
      correo: this.correo,
      password: this.password,
      rol: this.rol
    };

    // 1. Llama a la API de .NET que Axel probó
    this.http.post(this.apiUrl, registerDto).subscribe({
      next: (respuesta) => {
        //  Si la API responde OK (201 Created)
        console.log('Usuario registrado:', respuesta);

        //  Redirige al Login
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

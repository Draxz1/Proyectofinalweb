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
  // 🚨 FIX 1: template y style deben ser './login.html' y './login.css'
  templateUrl: './login.html', 
  styleUrl: './login.css'
})
export class LoginComponent { // <-- FIX 2: La clase debe ser EXPORTADA
  
  correo: string = '';
  password: string = '';
  error: string = '';

  http = inject(HttpClient);
  router = inject(Router);
  authService = inject(AuthService); 

  private apiUrl = 'http://localhost:5143/api/Auth/login'; // (Revisa tu puerto)

  login() {
    this.error = ''; 
    const loginDto = { correo: this.correo, password: this.password };

    this.http.post<any>(this.apiUrl, loginDto).subscribe({
      next: (respuesta) => {
        this.authService.saveToken(respuesta.token);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.error = 'Credenciales inválidas. Por favor, inténtalo de nuevo.';
        console.error('Error en el login:', err);
      }
    });
  }
}
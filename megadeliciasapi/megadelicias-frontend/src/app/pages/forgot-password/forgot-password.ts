import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css'
})
export class ForgotPasswordComponent {
  
  correo: string = '';
  mensaje: string = '';
  error: string = '';
  cargando: boolean = false;
  
  private http = inject(HttpClient);
  // Asegúrate de que este puerto sea el correcto de tu backend
  private apiUrl = 'http://localhost:5143/api/Auth/recuperar-password'; 

  enviar() {
    if (!this.correo) return;
    
    this.cargando = true;
    this.mensaje = '';
    this.error = '';

    this.http.post<any>(this.apiUrl, { correo: this.correo }).subscribe({
      next: (res) => {
        this.mensaje = '✅ ' + res.message;
        this.cargando = false;
      },
      error: (err) => {
        this.error = '❌ ' + (err.error?.message || 'Ocurrió un error al intentar enviar el correo.');
        this.cargando = false;
      }
    });
  }
}
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth';
import { LucideAngularModule } from 'lucide-angular';

interface Usuario {
  id: number;
  nombre: string;
  correo: string;
  rol: string;
  creadoEn: string;
  requiereCambioPassword: boolean;
}

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.css'
})
export class AdminUsersComponent implements OnInit {
  
  usuarios: Usuario[] = [];
  usuariosFiltrados: Usuario[] = [];
  loading: boolean = false;
  mensaje: string = '';
  error: string = '';
  
  // Modal estados
  mostrarModalCrear: boolean = false;
  mostrarModalEditar: boolean = false;
  mostrarModalEliminar: boolean = false;
  mostrarModalPassword: boolean = false;
  
  // Formularios
  nuevoUsuario = {
    nombre: '',
    correo: '',
    password: '',
    rol: 'mesero'
  };
  
  usuarioEditando: Usuario | null = null;
  usuarioEliminando: Usuario | null = null;
  usuarioCambiandoPassword: Usuario | null = null;
  nuevaPassword: string = '';
  
  // Filtros
  buscarTexto: string = '';
  filtroRol: string = 'Todos';
  
  roles = ['admin', 'mesero', 'cocinero', 'cajero', 'contable'];
  
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = 'http://localhost:5143/api/Usuarios';

  ngOnInit() {
    this.cargarUsuarios();
  }

  private getHeaders() {
    const token = this.authService.getToken();
    return {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      })
    };
  }

  cargarUsuarios() {
    this.loading = true;
    this.error = '';
    
    this.http.get<Usuario[]>(this.apiUrl, this.getHeaders()).subscribe({
      next: (data) => {
        this.usuarios = data;
        this.aplicarFiltros();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al cargar usuarios';
        this.loading = false;
        console.error('Error:', err);
      }
    });
  }

  aplicarFiltros() {
    let filtrados = [...this.usuarios];
    
    // Filtro por texto
    if (this.buscarTexto.trim()) {
      const texto = this.buscarTexto.toLowerCase();
      filtrados = filtrados.filter(u => 
        u.nombre.toLowerCase().includes(texto) ||
        u.correo.toLowerCase().includes(texto) ||
        u.rol.toLowerCase().includes(texto)
      );
    }
    
    // Filtro por rol
    if (this.filtroRol !== 'Todos') {
      filtrados = filtrados.filter(u => u.rol.toLowerCase() === this.filtroRol.toLowerCase());
    }
    
    this.usuariosFiltrados = filtrados;
  }

  abrirModalCrear() {
    this.nuevoUsuario = { nombre: '', correo: '', password: '', rol: 'mesero' };
    this.mostrarModalCrear = true;
    this.error = '';
    this.mensaje = '';
  }

  cerrarModalCrear() {
    this.mostrarModalCrear = false;
    this.nuevoUsuario = { nombre: '', correo: '', password: '', rol: 'mesero' };
  }

  crearUsuario() {
    if (!this.nuevoUsuario.nombre || !this.nuevoUsuario.correo || !this.nuevoUsuario.password) {
      this.error = 'Por favor complete todos los campos';
      return;
    }

    this.loading = true;
    this.error = '';
    
    this.http.post<any>(this.apiUrl, this.nuevoUsuario, this.getHeaders()).subscribe({
      next: (response) => {
        this.mensaje = 'Usuario creado exitosamente';
        this.cerrarModalCrear();
        this.cargarUsuarios();
        setTimeout(() => this.mensaje = '', 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al crear usuario';
        this.loading = false;
      }
    });
  }

  abrirModalEditar(usuario: Usuario) {
    this.usuarioEditando = { ...usuario };
    this.mostrarModalEditar = true;
    this.error = '';
    this.mensaje = '';
  }

  cerrarModalEditar() {
    this.mostrarModalEditar = false;
    this.usuarioEditando = null;
  }

  actualizarUsuario() {
    if (!this.usuarioEditando) return;
    
    if (!this.usuarioEditando.nombre || !this.usuarioEditando.correo) {
      this.error = 'Por favor complete todos los campos';
      return;
    }

    this.loading = true;
    this.error = '';
    
    const payload = {
      nombre: this.usuarioEditando.nombre,
      correo: this.usuarioEditando.correo,
      rol: this.usuarioEditando.rol
    };
    
    this.http.put<any>(`${this.apiUrl}/${this.usuarioEditando.id}`, payload, this.getHeaders()).subscribe({
      next: (response) => {
        this.mensaje = 'Usuario actualizado exitosamente';
        this.cerrarModalEditar();
        this.cargarUsuarios();
        setTimeout(() => this.mensaje = '', 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al actualizar usuario';
        this.loading = false;
      }
    });
  }

  abrirModalEliminar(usuario: Usuario) {
    this.usuarioEliminando = usuario;
    this.mostrarModalEliminar = true;
    this.error = '';
  }

  cerrarModalEliminar() {
    this.mostrarModalEliminar = false;
    this.usuarioEliminando = null;
  }

  eliminarUsuario() {
    if (!this.usuarioEliminando) return;

    this.loading = true;
    this.error = '';
    
    this.http.delete<any>(`${this.apiUrl}/${this.usuarioEliminando.id}`, this.getHeaders()).subscribe({
      next: (response) => {
        this.mensaje = 'Usuario eliminado exitosamente';
        this.cerrarModalEliminar();
        this.cargarUsuarios();
        setTimeout(() => this.mensaje = '', 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al eliminar usuario';
        this.loading = false;
      }
    });
  }

  abrirModalPassword(usuario: Usuario) {
    this.usuarioCambiandoPassword = usuario;
    this.nuevaPassword = '';
    this.mostrarModalPassword = true;
    this.error = '';
    this.mensaje = '';
  }

  cerrarModalPassword() {
    this.mostrarModalPassword = false;
    this.usuarioCambiandoPassword = null;
    this.nuevaPassword = '';
  }

  cambiarPassword() {
    if (!this.usuarioCambiandoPassword) return;
    
    if (!this.nuevaPassword || this.nuevaPassword.length < 6) {
      this.error = 'La contraseña debe tener al menos 6 caracteres';
      return;
    }

    this.loading = true;
    this.error = '';
    
    const payload = { nuevaPassword: this.nuevaPassword };
    
    this.http.post<any>(`${this.apiUrl}/${this.usuarioCambiandoPassword.id}/cambiar-password`, payload, this.getHeaders()).subscribe({
      next: (response) => {
        this.mensaje = 'Contraseña actualizada exitosamente';
        this.cerrarModalPassword();
        this.cargarUsuarios();
        setTimeout(() => this.mensaje = '', 3000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Error al cambiar contraseña';
        this.loading = false;
      }
    });
  }

  getRolBadgeClass(rol: string): string {
    const roles: { [key: string]: string } = {
      'admin': 'bg-purple-100 text-purple-800',
      'mesero': 'bg-blue-100 text-blue-800',
      'cocinero': 'bg-orange-100 text-orange-800',
      'cajero': 'bg-green-100 text-green-800',
      'contable': 'bg-indigo-100 text-indigo-800'
    };
    return roles[rol.toLowerCase()] || 'bg-gray-100 text-gray-800';
  }

  formatearFecha(fecha: string): string {
    return new Date(fecha).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
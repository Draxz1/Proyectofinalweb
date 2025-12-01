import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-cocina-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cocina-panel.html',
  styleUrl: './cocina-panel.css'
})
export class CocinaPanelComponent implements OnInit, OnDestroy {
  
  ordenes: any[] = [];
  loading: boolean = false;
  filtroActual: string = 'Todos'; 

  private intervalId: any;
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = 'http://localhost:5143/api/cocina'; 

  private getHeaders() {
    const token = this.authService.getToken();
    return {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    };
  }

  ngOnInit() {
    this.fetchOrdenes();
    this.intervalId = setInterval(() => this.fetchOrdenes(), 5000);
  }

  ngOnDestroy() {
    if (this.intervalId) clearInterval(this.intervalId);
  }

  fetchOrdenes() {
    this.http.get<any[]>(this.apiUrl, this.getHeaders()).subscribe({
      next: (data) => {
        // Normalizar estados para evitar problemas de mayúsculas
        data.forEach(orden => {
          if (orden.estado) {
            orden.estado = orden.estado.toUpperCase().trim();
          }
        });
        
        if (JSON.stringify(data) !== JSON.stringify(this.ordenes)) {
          this.ordenes = data;
        }
      },
      error: (err) => console.error("Error conectando a cocina:", err)
    });
  }

  trackByOrden(index: number, item: any): number {
    return item.id;
  }

  get ordenesFiltradas() {
    if (this.filtroActual === 'Todos') {
      return this.ordenes;
    }
    return this.ordenes.filter(o => o.estado === this.filtroActual);
  }

  contarOrdenes(estado: string): number {
    if (estado === 'Todos') return this.ordenes.length;
    return this.ordenes.filter(o => o.estado === estado).length;
  }

  setFiltro(filtro: string) {
    this.filtroActual = filtro;
  }

  // ✅ MÉTODO PRINCIPAL: Cambiar estado de la orden
  cambiarEstado(ordenId: number, nuevoEstado: string) {
    // ⚠️ Confirmación SOLO para cancelar (acción destructiva)
    if (nuevoEstado === 'CANCELADO') {
      const confirmar = confirm('⚠️ ¿Seguro que deseas cancelar esta orden?');
      if (!confirmar) return;
    }

    this.loading = true;
    const url = `${this.apiUrl}/${ordenId}/estado`;
    
    this.http.put(url, { estado: nuevoEstado }, this.getHeaders()).subscribe({
      next: (response: any) => {
        this.fetchOrdenes(); 
        this.loading = false;
        
        // ✅ Mensaje de éxito solo DESPUÉS de procesar exitosamente
        if (nuevoEstado === 'EN_PROCESO') {
          this.mostrarMensajeExito(
            '✅ Orden en Proceso',
            'Los ingredientes han sido descontados del inventario correctamente.'
          );
        } else if (nuevoEstado === 'LISTO') {
          this.mostrarMensajeExito(
            '✅ Orden Lista',
            'La orden está lista para ser recogida por el mesero.'
          );
        } else if (nuevoEstado === 'CANCELADO') {
          this.mostrarMensajeExito(
            '⚠️ Orden Cancelada',
            'La orden ha sido cancelada. No se descontó inventario.'
          );
        } else {
          this.mostrarMensajeExito(
            '✅ Estado Actualizado',
            `La orden #${ordenId} ahora está en estado: ${nuevoEstado}`
          );
        }
      },
      error: (err) => {
        this.loading = false;
        
        // ❌ Manejo detallado de errores
        if (err.error && err.error.message) {
          const titulo = err.error.message;
          const detalles = err.error.detalles || '';
          
          this.mostrarError(titulo, detalles);
        } else if (err.status === 0) {
          this.mostrarError(
            '❌ Error de Conexión',
            'No se pudo conectar con el servidor. Verifica que el backend esté ejecutándose.'
          );
        } else if (err.status === 401) {
          this.mostrarError(
            '❌ Sesión Expirada',
            'Tu sesión ha expirado. Por favor, inicia sesión nuevamente.'
          );
        } else {
          this.mostrarError(
            '❌ Error al Actualizar',
            'Ocurrió un error inesperado. Intenta nuevamente.'
          );
        }
      }
    });
  }

  // 🆕 MÉTODO AUXILIAR: Mostrar mensaje de éxito
  private mostrarMensajeExito(titulo: string, mensaje: string) {
    alert(`${titulo}\n\n${mensaje}`);
  }

  // 🆕 MÉTODO AUXILIAR: Mostrar error detallado
  private mostrarError(titulo: string, detalles: string) {
    if (detalles && detalles.trim() !== '') {
      // Error con detalles (ej: falta de inventario)
      alert(
        `${titulo}\n\n` +
        `━━━━━━━━━━━━━━━━━━━━━━━━━━\n` +
        `${detalles}\n` +
        `━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n` +
        `💡 Sugerencia: Verifica el inventario antes de procesar esta orden.`
      );
    } else {
      // Error simple
      alert(titulo);
    }
  }

  // FUNCIÓN: Notificar al mesero
  notificarMesero(orden: any) {
    const mensaje = `¡Orden #${orden.id} lista para ${orden.mesero}!`;
    
    if (confirm(mensaje + '\n\n¿Deseas marcar como notificado?')) {
      console.log('Mesero notificado:', orden.mesero);
      
      // Opcional: Cambiar automáticamente a ENTREGADO después de notificar
      // this.cambiarEstado(orden.id, 'ENTREGADO');
    }
  }

  getColorEstado(estado: string) {
    switch(estado) {
      case 'PENDIENTE': return 'border-l-4 border-l-yellow-500 bg-yellow-50/50';
      case 'EN_PROCESO': return 'border-l-4 border-l-blue-500 bg-blue-50/50';
      case 'LISTO': return 'border-l-4 border-l-green-500 bg-green-50/50';
      case 'ENTREGADO': return 'border-l-4 border-l-gray-400 bg-white opacity-60 grayscale';
      case 'CANCELADO': return 'border-l-4 border-l-red-500 bg-red-50 opacity-60';
      default: return 'bg-white';
    }
  }
  
  getBadgeColor(estado: string) {
    switch(estado) {
      case 'PENDIENTE': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'EN_PROCESO': return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'LISTO': return 'bg-green-100 text-green-800 border-green-200';
      case 'ENTREGADO': return 'bg-gray-100 text-gray-600 border-gray-200';
      case 'CANCELADO': return 'bg-red-100 text-red-800 border-red-200';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}
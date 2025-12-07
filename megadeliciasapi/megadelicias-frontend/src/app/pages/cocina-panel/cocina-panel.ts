import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth'; // Asegúrate que la ruta sea correcta

@Component({
  selector: 'app-cocina-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cocina-panel.html', // Corregido nombre estándar
  styleUrl: './cocina-panel.css' // Corregido nombre estándar
})
export class CocinaPanelComponent implements OnInit, OnDestroy {
  
  ordenes: any[] = [];
  loading: boolean = false;
  filtroActual: string = 'Todos'; 
  mensajeError: string = ''; // Para mostrar error en UI si es necesario

  private intervalId: any;
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = 'http://localhost:5143/api/cocina'; 

  // Headers con Token
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
    // Iniciamos el polling cada 5 segundos
    this.intervalId = setInterval(() => this.fetchOrdenes(), 5000);
  }

  ngOnDestroy() {
    this.detenerActualizacionAutomatica();
  }

  // ✅ NUEVO: Método para detener el bucle si hay error crítico
  detenerActualizacionAutomatica() {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
      console.warn('🔄 Sincronización automática detenida.');
    }
  }

  fetchOrdenes() {
    // Si ya sabemos que no hay permiso, no intentamos más (protección extra)
    if (this.mensajeError.includes('permiso')) return;

    this.http.get<any[]>(this.apiUrl, this.getHeaders()).subscribe({
      next: (data) => {
        this.mensajeError = ''; // Limpiar errores previos si conecta bien

        // Normalizar estados para evitar problemas de mayúsculas
        data.forEach(orden => {
          if (orden.estado) {
            orden.estado = orden.estado.toUpperCase().trim();
          }
        });
        
        // Solo actualizamos si la data cambió para evitar parpadeos
        if (JSON.stringify(data) !== JSON.stringify(this.ordenes)) {
          this.ordenes = data;
        }
      },
      error: (err) => {
        // 🛑 CORRECCIÓN PRINCIPAL AQUÍ
        if (err.status === 403) {
          console.error("⛔ ACCESO DENEGADO (403): Deteniendo actualizaciones.");
          this.mensajeError = 'No tienes permiso para ver la cocina.';
          this.detenerActualizacionAutomatica(); // <--- ESTO EVITA EL BUCLE INFINITO
          
          // Opcional: Mostrar alerta solo una vez
          if (!this.loading) { // Usamos loading como flag temporal para no spamear alertas
             alert('⛔ No tienes permisos para acceder al panel de cocina.\n\nEl sistema dejará de intentar conectarse.');
          }
        } else {
          console.error("Error conectando a cocina:", err);
        }
      }
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
        
        // ✅ Mensaje de éxito
        if (nuevoEstado === 'EN_PROCESO') {
          this.mostrarMensajeExito('✅ Orden en Proceso', 'Los ingredientes han sido descontados del inventario.');
        } else if (nuevoEstado === 'LISTO') {
          this.mostrarMensajeExito('✅ Orden Lista', 'La orden está lista para ser recogida.');
        } else if (nuevoEstado === 'CANCELADO') {
          this.mostrarMensajeExito('⚠️ Orden Cancelada', 'La orden ha sido cancelada.');
        } else {
          this.mostrarMensajeExito('✅ Estado Actualizado', `La orden #${ordenId} ahora está: ${nuevoEstado}`);
        }
      },
      error: (err) => {
        this.loading = false;
        
        // ❌ Manejo detallado de errores
        if (err.error && err.error.message) {
          this.mostrarError(err.error.message, err.error.detalles || '');
        } else if (err.status === 0) {
          this.mostrarError('❌ Error de Conexión', 'No se pudo conectar con el servidor.');
        } else if (err.status === 401 || err.status === 403) {
           // Si falla al cambiar estado por permisos, también detenemos el polling
           this.detenerActualizacionAutomatica();
           this.mostrarError('❌ Sin Permisos', 'Tu sesión expiró o no tienes permiso para realizar esta acción.');
        } else {
          this.mostrarError('❌ Error al Actualizar', 'Ocurrió un error inesperado.');
        }
      }
    });
  }

  // 🆕 MÉTODO AUXILIAR: Mostrar mensaje de éxito
  private mostrarMensajeExito(titulo: string, mensaje: string) {
    // Usamos setTimeout para asegurar que la UI se actualice antes del alert
    setTimeout(() => alert(`${titulo}\n\n${mensaje}`), 100);
  }

  // 🆕 MÉTODO AUXILIAR: Mostrar error detallado
  private mostrarError(titulo: string, detalles: string) {
    let msg = titulo;
    if (detalles && detalles.trim() !== '') {
      msg += `\n━━━━━━━━━━━━━━━━━━━━━━━━━━\n${detalles}\n━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n💡 Sugerencia: Verifica el inventario.`;
    }
    setTimeout(() => alert(msg), 100);
  }

  // FUNCIÓN: Notificar al mesero
  notificarMesero(orden: any) {
    const mensaje = `¡Orden #${orden.id} lista para ${orden.mesero || 'el mesero'}!`;
    if (confirm(mensaje + '\n\n¿Deseas marcar como notificado?')) {
      console.log('Mesero notificado:', orden.mesero);
      // Opcional: Llamada al backend para notificar
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
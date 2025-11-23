import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-caja-panel',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './caja-panel.html',
  styleUrls: ['./caja-panel.css']
})
export class CajaPanelComponent implements OnInit, OnDestroy {

  ordenes: any[] = [];
  movimientos: any[] = [];
  filtroActual: string = 'Todos';
  loading: boolean = false;
  metodosPago: string[] = [];
  metodoSeleccionadoPorOrden: { [ordenId: number]: string } = {};

  private intervalId: any;
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = 'http://localhost:5143/api/caja';

  private getHeaders() {
    const token = this.authService.getToken();
    return { headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` }) };
  }

ngOnInit() {
  this.fetchMetodosPago(); // <-- Añadido
  this.fetchOrdenes();
  this.fetchMovimientos();

  this.intervalId = setInterval(() => {
    this.fetchOrdenes();
    this.fetchMovimientos();
  }, 5000);
}

  ngOnDestroy() {
    if (this.intervalId) clearInterval(this.intervalId);
  }

fetchMetodosPago() {
  this.http.get<any[]>(`http://localhost:5143/api/caja/metodos-pago`, this.getHeaders()).subscribe({
    next: (data) => {
      this.metodosPago = data.map(m => m.Nombre); // Asegúrate que es "Nombre", no "nombre"
      // Inicializar con "Efectivo" si existe
      if (this.metodosPago.length > 0) {
        this.metodosPago.forEach(m => {
          // No inicializamos aquí; lo hacemos dinámicamente en el HTML
        });
      }
    },
    error: (err) => console.error('Error obteniendo métodos de pago:', err)
  });
}

  fetchOrdenes() {
    this.http.get<any[]>(`${this.apiUrl}/ordenes-pendientes`, this.getHeaders()).subscribe({
      next: (data) => {
        if (JSON.stringify(data) !== JSON.stringify(this.ordenes)) {
          this.ordenes = data;
        }
      },
      error: (err) => console.error('Error obteniendo órdenes:', err)
    });
  }

  fetchMovimientos() {
    this.http.get<any[]>(`${this.apiUrl}/movimientos`, this.getHeaders()).subscribe({
      next: (data) => {
        if (JSON.stringify(data) !== JSON.stringify(this.movimientos)) {
          this.movimientos = data;
        }
      },
      error: (err) => console.error('Error obteniendo movimientos:', err)
    });
  }

  get ordenesFiltradas() {
    if (this.filtroActual === 'Todos') return this.ordenes;
    return this.ordenes.filter(o => o.estado === this.filtroActual);
  }

  getCantidadPorEstado(estado: string) {
    if (estado === 'Todos') return this.ordenes.length;
    return this.ordenes.filter(o => o.estado === estado).length;
  }

  setFiltro(filtro: string) {
    this.filtroActual = filtro;
  }

  trackByOrden(index: number, item: any) {
    return item.id;
  }

  trackByMovimiento(index: number, item: any) {
    return item.id || index;
  }

 registrarPago(orden: any) {
  const metodo = this.metodoSeleccionadoPorOrden[orden.id] || 'Efectivo'; // Usa el seleccionado o "Efectivo" por defecto
  this.loading = true;
  const url = `${this.apiUrl}/ordenes/${orden.id}/pagar`;
  this.http.post(url, { metodoPago: metodo }, this.getHeaders()).subscribe({
    next: () => {
      // Eliminar la orden de la lista (porque ya fue pagada)
      this.ordenes = this.ordenes.filter(o => o.id !== orden.id);
      this.fetchMovimientos(); // Actualiza movimientos
      delete this.metodoSeleccionadoPorOrden[orden.id]; // Limpia el seleccionado
      this.loading = false;
    },
    error: () => {
      alert('Error al registrar el pago');
      this.loading = false;
    }
  });
}

  getColorEstado(estado: string) {
    switch (estado) {
      case 'PENDIENTE': return 'border-l-4 border-l-yellow-500 bg-yellow-50/50';
      case 'EN_PROCESO': return 'border-l-4 border-l-blue-500 bg-blue-50/50';
      case 'LISTO': return 'border-l-4 border-l-green-500 bg-green-50/50';
      case 'ENTREGADO': return 'border-l-4 border-l-gray-400 bg-white opacity-60 grayscale';
      case 'CANCELADO': return 'border-l-4 border-l-red-500 bg-red-50 opacity-60';
      default: return 'bg-white';
    }
  }

  getBadgeColor(estado: string) {
    switch (estado) {
      case 'PENDIENTE': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'EN_PROCESO': return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'LISTO': return 'bg-green-100 text-green-800 border-green-200';
      case 'ENTREGADO': return 'bg-gray-100 text-gray-600 border-gray-200';
      case 'CANCELADO': return 'bg-red-100 text-red-800 border-red-200';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}

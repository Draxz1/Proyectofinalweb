import { Component, OnInit, OnDestroy, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';

// ✅ Interfaces tipadas
interface MetodoPago {
  id: number;
  nombre: string;
}

interface Orden {
  id: number;
  mesaId?: number;
  meseroNombre?: string;
  total: number;
  fechaCreacion: string | Date;
  estado: string;
}

interface Movimiento {
  id: number;
  monto: number;
  tipo: string;
  fecha: string | Date;
  usuario?: { nombre: string };
  metodoPago?: { nombre: string };
  descripcion?: string;
}

@Component({
  selector: 'app-caja-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './caja-panel.html',
  styleUrls: ['./caja-panel.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CajaPanelComponent implements OnInit, OnDestroy {

  ordenes: Orden[] = [];
  movimientos: Movimiento[] = [];
  metodosPago: MetodoPago[] = []; // ✅ CORRECTO: Array de objetos
  
  filtroActual: string = 'Todos';
  loading: boolean = false;
  metodoSeleccionadoPorOrden: { [ordenId: number]: number } = {}; // ✅ Guarda el ID del método

  private intervalId: any;
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);
  private apiUrl = 'http://localhost:5143/api/caja';

  private getHeaders() {
    const token = this.authService.getToken();
    return { headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` }) };
  }

  ngOnInit() {
    this.fetchMetodosPago();
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

  // ✅ Obtener métodos de pago como objetos {id, nombre}
  fetchMetodosPago() {
    this.http.get<any[]>(`${this.apiUrl}/metodos-pago`, this.getHeaders()).subscribe({
      next: (data) => {
        // Normalizar a minúsculas para compatibilidad
        this.metodosPago = data.map(m => ({
          id: m.id || m.Id,
          nombre: m.nombre || m.Nombre
        }));
        
        console.log('✅ Métodos de pago cargados:', this.metodosPago);
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('❌ Error obteniendo métodos de pago:', err);
        this.cdr.markForCheck();
      }
    });
  }

  fetchOrdenes() {
    this.http.get<any[]>(`${this.apiUrl}/ordenes-pendientes`, this.getHeaders()).subscribe({
      next: (data) => {
        if (this.hasChangedOrdenes(data, this.ordenes)) {
          this.ordenes = data.map(o => ({
            id: o.id || o.Id,
            mesaId: o.mesaId || o.MesaId,
            meseroNombre: o.meseroNombre || o.MeseroNombre,
            total: o.total || o.Total,
            fechaCreacion: o.fechaCreacion || o.FechaCreacion,
            estado: o.estado || o.Estado
          }));
          this.cdr.markForCheck();
        }
      },
      error: (err) => {
        console.error('❌ Error obteniendo órdenes:', err);
        this.cdr.markForCheck();
      }
    });
  }

  fetchMovimientos() {
    this.http.get<any[]>(`${this.apiUrl}/movimientos`, this.getHeaders()).subscribe({
      next: (data) => {
        if (this.hasChangedMovimientos(data, this.movimientos)) {
          this.movimientos = data;
          this.cdr.markForCheck();
        }
      },
      error: (err) => {
        console.error('❌ Error obteniendo movimientos:', err);
        this.cdr.markForCheck();
      }
    });
  }

  private hasChangedOrdenes(newData: any[], oldData: any[]): boolean {
    if (newData.length !== oldData.length) return true;
    
    return newData.some((newOrden, i) => {
      const oldOrden = oldData[i];
      return !oldOrden || 
             newOrden.id !== oldOrden.id || 
             newOrden.estado !== oldOrden.estado ||
             newOrden.total !== oldOrden.total;
    });
  }

  private hasChangedMovimientos(newData: any[], oldData: any[]): boolean {
    if (newData.length !== oldData.length) return true;
    
    return newData.some((newMov, i) => {
      const oldMov = oldData[i];
      return !oldMov || newMov.id !== oldMov.id;
    });
  }

  ordenesFiltradas(): Orden[] {
    if (this.filtroActual === 'Todos') return this.ordenes;
    return this.ordenes.filter(o => o.estado === this.filtroActual);
  }

  getCantidadPorEstado(estado: string): number {
    if (estado === 'Todos') return this.ordenes.length;
    return this.ordenes.filter(o => o.estado === estado).length;
  }

  setFiltro(filtro: string) {
    this.filtroActual = filtro;
    this.cdr.markForCheck();
  }

  trackByOrden(index: number, item: any): number {
    return item.id;
  }

  trackByMovimiento(index: number, item: any): number {
    return item.id || index;
  }

  // ✅ MÉTODO CORREGIDO: Registrar pago
  registrarPago(orden: Orden) {
    const metodoPagoId = this.metodoSeleccionadoPorOrden[orden.id];
    
    if (!metodoPagoId) {
      alert('❌ Por favor selecciona un método de pago');
      return;
    }

    console.log('💳 Registrando pago:', { 
      ordenId: orden.id, 
      metodoPagoId, 
      monto: orden.total 
    });

    this.loading = true;
    this.cdr.markForCheck();

    const url = `${this.apiUrl}/ordenes/${orden.id}/pagar`;
    const payload = {
      metodoPagoId: metodoPagoId,
      monto: orden.total
    };

    this.http.post(url, payload, this.getHeaders()).subscribe({
      next: (response: any) => {
        console.log('✅ Pago registrado exitosamente:', response);
        
        // Eliminar orden de la lista
        this.ordenes = this.ordenes.filter(o => o.id !== orden.id);
        
        // Recargar movimientos
        this.fetchMovimientos();
        
        // Limpiar selección
        delete this.metodoSeleccionadoPorOrden[orden.id];
        
        this.loading = false;
        this.cdr.markForCheck();
        
        alert('✅ ' + (response.message || 'Pago registrado correctamente'));
      },
      error: (err) => {
        console.error('❌ Error al registrar pago:', err);
        
        const errorMsg = err.error?.message || 'Error al registrar el pago';
        alert(`❌ ${errorMsg}`);
        
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  getColorEstado(estado: string): string {
    switch (estado) {
      case 'PENDIENTE': return 'border-l-4 border-l-yellow-500 bg-yellow-50/50';
      case 'EN_PROCESO': return 'border-l-4 border-l-blue-500 bg-blue-50/50';
      case 'LISTO': return 'border-l-4 border-l-green-500 bg-green-50/50';
      case 'ENTREGADO': return 'border-l-4 border-l-gray-400 bg-white opacity-60 grayscale';
      case 'CANCELADO': return 'border-l-4 border-l-red-500 bg-red-50 opacity-60';
      default: return 'bg-white';
    }
  }

  getBadgeColor(estado: string): string {
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
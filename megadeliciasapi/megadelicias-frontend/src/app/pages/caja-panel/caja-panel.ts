import { Component, ChangeDetectionStrategy, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { BehaviorSubject, timer, forkJoin, Subject, of } from 'rxjs';
import { switchMap, catchError, takeUntil, tap } from 'rxjs/operators';
import { ChangeDetectorRef } from '@angular/core';

interface Plato {
  nombre: string;
  cantidad: number;
}

interface Orden {
  id: number;
  mesaId?: number;
  meseroNombre?: string;
  total: number;
  fechaCreacion: string | Date;
  estado: string;
  platos: Plato[];
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
  imports: [CommonModule, FormsModule, CurrencyPipe],
  templateUrl: './caja-panel.html',
  styleUrls: ['./caja-panel.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CajaPanelComponent implements OnInit, OnDestroy {

  private ordenesSubject = new BehaviorSubject<Orden[]>([]);
  ordenes$ = this.ordenesSubject.asObservable();

  private movimientosSubject = new BehaviorSubject<Movimiento[]>([]);
  movimientos$ = this.movimientosSubject.asObservable();

  private metodosPagoSubject = new BehaviorSubject<{ id: number; nombre: string }[]>([]);
  metodosPago$ = this.metodosPagoSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingSubject.asObservable();

  filtroActual = 'Todos';
  metodoSeleccionadoPorOrden: { [ordenId: number]: string } = {};

  get ordenesFiltradas() {
    const ordenes = this.ordenesSubject.value || [];
    if (this.filtroActual === 'Todos') return ordenes;
    return ordenes.filter(o => o.estado === this.filtroActual);
  }

  get movimientos() {
    return this.movimientosSubject.value || [];
  }

  get metodosPago() {
    return this.metodosPagoSubject.value || [];
  }

  private destroy$ = new Subject<void>();
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);
  private apiUrl = 'http://localhost:5143/api/caja';
  private pollIntervalMs = 5000;

  private getHeaders() {
    const token = this.authService.getToken();
    return { headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` }) };
  }

  ngOnInit(): void {
    this.fetchMetodosPago();

    timer(0, this.pollIntervalMs).pipe(
      takeUntil(this.destroy$),
      switchMap(() => {
        const o$ = this.http.get<Orden[]>(`${this.apiUrl}/ordenes-pendientes`, this.getHeaders()).pipe(
          catchError(err => {
            console.error('Error obteniendo órdenes:', err);
            return of([] as Orden[]);
          })
        );
        const m$ = this.http.get<Movimiento[]>(`${this.apiUrl}/movimientos`, this.getHeaders()).pipe(
          catchError(err => {
            console.error('Error obteniendo movimientos:', err);
            return of([] as Movimiento[]);
          })
        );
        return forkJoin([o$, m$]);
      }),
      tap(([ordenes, movimientos]) => {
        this.ordenesSubject.next(ordenes);
        this.movimientosSubject.next(movimientos);
        this.cdr.markForCheck();
      })
    ).subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  fetchMetordesPago() {
    // Endpoint sin auth
    this.http.get<any[]>('http://localhost:5143/api/caja/metodos-pago').pipe(
      takeUntil(this.destroy$),
      catchError(err => {
        console.error('Error obteniendo métodos de pago:', err);
        return of([]);
      }),
      tap(data => {
        const metodos = (data || []).map(m => ({
          id: m.Id ?? m.id,
          nombre: (m.Nombre ?? m.nombre).toString()
        }));
        this.metodosPagoSubject.next(metodos);
        this.cdr.markForCheck();
      })
    ).subscribe();
  }

  fetchMetodosPago() {
    this.http.get<any[]>('http://localhost:5143/api/caja/metodos-pago').subscribe({
      next: (data) => {
        const metodos = data.map(m => ({
          id: m.Id ?? m.id,
          nombre: m.Nombre ?? m.nombre
        }));
        this.metodosPagoSubject.next(metodos);
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Error métodos de pago:', err)
    });
  }

  registrarPago(orden: Orden) {
    const metodoNombre = this.metodoSeleccionadoPorOrden[orden.id];
    if (!metodoNombre) return;

    const metodos = this.metodosPagoSubject.value;
    const metodo = metodos.find(m => m.nombre === metodoNombre);
    if (!metodo) {
      alert('Método de pago no válido');
      return;
    }

    this.loadingSubject.next(true);
    const url = `${this.apiUrl}/ordenes/${orden.id}/pagar`;

    this.http.post(url, { metodoPagoId: metodo.id, monto: orden.total }, this.getHeaders()).pipe(
      takeUntil(this.destroy$),
      catchError(err => {
        console.error('Error al registrar el pago', err);
        alert('Error al registrar el pago');
        this.loadingSubject.next(false);
        return of(null);
      }),
      tap(res => {
        if (res !== null) {
          // ✅ Refrescar TODO tras el pago
          const o$ = this.http.get<Orden[]>(`${this.apiUrl}/ordenes-pendientes`, this.getHeaders()).pipe(
            catchError(err => {
              console.error('Error recargando órdenes:', err);
              return of([] as Orden[]);
            })
          );
          const m$ = this.http.get<Movimiento[]>(`${this.apiUrl}/movimientos`, this.getHeaders()).pipe(
            catchError(err => {
              console.error('Error recargando movimientos:', err);
              return of([] as Movimiento[]);
            })
          );
          forkJoin([o$, m$]).subscribe(([nuevasOrdenes, nuevosMovimientos]) => {
            this.ordenesSubject.next(nuevasOrdenes);
            this.movimientosSubject.next(nuevosMovimientos);
            delete this.metodoSeleccionadoPorOrden[orden.id];
            this.cdr.markForCheck();
          });
        }
        this.loadingSubject.next(false);
      })
    ).subscribe();
  }

  setFiltro(filtro: string) {
    this.filtroActual = filtro;
  }

  trackByOrden(_index: number, item: Orden) {
    return item.id;
  }

  trackByMovimiento(_index: number, item: Movimiento) {
    return item.id;
  }

  getCantidadPorEstado(estado: string) {
    const ordenes = this.ordenesSubject.value || [];
    if (estado === 'Todos') return ordenes.length;
    return ordenes.filter(o => o.estado === estado).length;
  }

  getColorEstado(estado?: string) {
    switch (estado) {
      case 'PENDIENTE': return 'border-l-4 border-l-yellow-500 bg-yellow-50/50';
      case 'EN_PROCESO': return 'border-l-4 border-l-blue-500 bg-blue-50/50';
      case 'LISTO': return 'border-l-4 border-l-green-500 bg-green-50/50';
      case 'ENTREGADO': return 'border-l-4 border-l-gray-400 bg-white opacity-60 grayscale';
      case 'CANCELADO': return 'border-l-4 border-l-red-500 bg-red-50 opacity-60';
      default: return 'bg-white';
    }
  }

  getBadgeColor(estado?: string) {
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
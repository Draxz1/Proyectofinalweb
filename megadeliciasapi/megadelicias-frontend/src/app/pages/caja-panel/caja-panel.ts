import { Component, ChangeDetectionStrategy, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { BehaviorSubject, timer, forkJoin, Subject, of } from 'rxjs';
import { switchMap, catchError, takeUntil, tap } from 'rxjs/operators';
import { ChangeDetectorRef } from '@angular/core';

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
  imports: [CommonModule, FormsModule, CurrencyPipe],
  templateUrl: './caja-panel.html',
  styleUrls: ['./caja-panel.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CajaPanelComponent implements OnInit, OnDestroy {

  // --- Estado reactivo ---
  private ordenesSubject = new BehaviorSubject<Orden[]>([]);
  ordenes$ = this.ordenesSubject.asObservable();

  private movimientosSubject = new BehaviorSubject<Movimiento[]>([]);
  movimientos$ = this.movimientosSubject.asObservable();

  private metodosPagoSubject = new BehaviorSubject<string[]>([]);
  metodosPago$ = this.metodosPagoSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingSubject.asObservable();

  // Estado no reactivo/UX
  filtroActual = 'Todos';
  metodoSeleccionadoPorOrden: { [ordenId: number]: string } = {};

  // Compatibilidad para tu template actual
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

  // Internals
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

    // Polling reactivo para órdenes y movimientos
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
        if (this._isDifferentOrdenes(this.ordenesSubject.value, ordenes)) {
          this.ordenesSubject.next(ordenes);
        }
        if (this._isDifferentMovimientos(this.movimientosSubject.value, movimientos)) {
          this.movimientosSubject.next(movimientos);
        }
      })
    ).subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  fetchMetodosPago() {
    this.http.get<any[]>(`${this.apiUrl}/metodos-pago`, this.getHeaders()).pipe(
      takeUntil(this.destroy$),
      catchError(err => {
        console.error('Error obteniendo métodos de pago:', err);
        return of([]);
      }),
      tap(data => {
        const nombres = (data || []).map(m => (m.Nombre ?? m.nombre ?? m).toString());
        this.metodosPagoSubject.next(nombres);
      })
    ).subscribe();
  }

  registrarPago(orden: Orden) {
    const metodo = this.metodoSeleccionadoPorOrden[orden.id] || 'Efectivo';
    const url = `${this.apiUrl}/ordenes/${orden.id}/pagar`;

    this.loadingSubject.next(true);

    this.http.post(url, { metodoPago: metodo }, this.getHeaders()).pipe(
      takeUntil(this.destroy$),
      catchError(err => {
        console.error('Error al registrar el pago', err);
        alert('Error al registrar el pago');
        return of(null);
      }),
      tap(res => {
        if (res !== null) {
          const actuales = this.ordenesSubject.value.filter(o => o.id !== orden.id);
          this.ordenesSubject.next(actuales);

          // Recarga movimientos inmediatamente
          this.http.get<Movimiento[]>(`${this.apiUrl}/movimientos`, this.getHeaders()).pipe(
            catchError(err => {
              console.error('Error recargando movimientos:', err);
              return of([]);
            }),
            takeUntil(this.destroy$)
          ).subscribe(movs => this.movimientosSubject.next(movs));

          delete this.metodoSeleccionadoPorOrden[orden.id];
        }
        this.loadingSubject.next(false);
      })
    ).subscribe();
  }

  // --- Métodos requeridos por template ---
  setFiltro(filtro: string) { this.filtroActual = filtro; }

  trackByOrden(_index: number, item: Orden) { return item.id; }
  trackByMovimiento(_index: number, item: Movimiento) { return item.id ?? _index; }

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

  // --- Helpers privados ---
  private _isDifferentOrdenes(a: Orden[], b: Orden[]) {
    if (!a || !b) return true;
    if (a.length !== b.length) return true;
    if (a.length === 0) return false;
    const lastA = a[a.length - 1];
    const lastB = b[b.length - 1];
    return lastA.id !== lastB.id || lastA.estado !== lastB.estado || lastA.total !== lastB.total;
  }

  private _isDifferentMovimientos(a: Movimiento[], b: Movimiento[]) {
    if (!a || !b) return true;
    if (a.length !== b.length) return true;
    if (a.length === 0) return false;

    const lastA = a[a.length - 1];
    const lastB = b[b.length - 1];

    // Comparamos el nombre del método de pago en lugar de 'metodo'
    const metodoA = lastA.metodoPago?.nombre ?? '';
    const metodoB = lastB.metodoPago?.nombre ?? '';

    return lastA.monto !== lastB.monto || lastA.fecha !== lastB.fecha || metodoA !== metodoB;
  }

  // Dev util: obtiene cantidad por estado desde la fuente reactiva
  getCantidadPorEstadoDev(estado: string) {
    const ordenes = this.ordenesSubject.value || [];
    if (estado === 'Todos') return ordenes.length;
    return ordenes.filter(o => o.estado === estado).length;
  }
}

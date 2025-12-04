import { Component, ChangeDetectionStrategy, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
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
  imports: [CommonModule, FormsModule],
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

  // Factura actual para modal / impresión
  facturaActual: any = null;
  showFacturaModal = false;

  get ordenesFiltradas() {
    const ordenes = this.ordenesSubject.value || [];
    if (this.filtroActual === 'Todos') return ordenes;
    return ordenes.filter(o => o.estado === this.filtroActual);
  }

  get movimientosFiltrados(): Movimiento[] {
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
        const o$ = this.http.get<any[]>(`${this.apiUrl}/ordenes-pendientes`, this.getHeaders()).pipe(
          catchError(err => {
            console.error('Error obteniendo órdenes:', err);
            return of([] as any[]);
          })
        );
        const m$ = this.http.get<any[]>(`${this.apiUrl}/movimientos`, this.getHeaders()).pipe(
          catchError(err => {
            console.error('Error obteniendo movimientos:', err);
            return of([] as any[]);
          })
        );
        return forkJoin([o$, m$]);
      }),
      tap(([ordenesRaw, movimientosRaw]) => {
        // Normalizar Órdenes (backend puede devolver PascalCase)
        const ordenes = (ordenesRaw || []).map(o => ({
          id: o.id ?? o.Id,
          mesaId: o.mesaId ?? o.MesaId,
          meseroNombre: o.meseroNombre ?? o.MeseroNombre ?? (o.usuario ? (o.usuario.nombre ?? o.usuario.Nombre) : undefined),
          total: o.total ?? o.Total ?? o.TotalOrden,
          fechaCreacion: o.fechaCreacion ?? o.FechaCreacion ?? o.fecha,
          estado: o.estado ?? o.Estado,
          platos: o.platos ?? o.Platos ?? []
        })) as Orden[];

        // Normalizar Movimientos (backend puede devolver PascalCase)
        const movimientos = (movimientosRaw || []).map(m => ({
          id: m.id ?? m.Id,
          monto: m.monto ?? m.Monto,
          tipo: m.tipo ?? m.Tipo,
          fecha: m.fecha ?? m.Fecha ?? m.fechaPago ?? m.FechaPago,
          usuario: {
            nombre: (m.usuario && (m.usuario.nombre ?? m.usuario.Nombre)) ?? (m.usuarioNombre ?? m.UsuarioNombre) ?? null
          },
          metodoPago: {
            nombre: (m.metodoPago && (m.metodoPago.nombre ?? m.metodoPago.Nombre)) ?? (m.metodoPagoNombre ?? null)
          },
          descripcion: m.descripcion ?? m.Descripcion,
        })) as Movimiento[];

        // Push al Subject
        this.ordenesSubject.next(ordenes);
        this.movimientosSubject.next(movimientos);
        this.cdr.markForCheck();
      })
    ).subscribe({
      error: err => console.error('Error en polling caja:', err)
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // --------- Métodos para métodos de pago ----------
  fetchMetodosPago() {
    this.http.get<any[]>(`${this.apiUrl}/metodos-pago`).pipe(
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

  // --------- Registrar Pago (modificado para recibir y mostrar factura) ----------
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

    this.http.post<any>(url, { metodoPagoId: metodo.id, monto: orden.total }, this.getHeaders()).pipe(
      takeUntil(this.destroy$),
      catchError(err => {
        console.error('Error al registrar el pago', err);
        alert(err?.error?.message ?? 'Error al registrar el pago');
        this.loadingSubject.next(false);
        return of(null);
      }),
      tap(res => {
        if (res !== null) {
          // Si viene factura en la respuesta, mostrar modal
          this.handlePagoSuccess(res);

          // Refrescar órdenes y movimientos
          const o$ = this.http.get<any[]>(`${this.apiUrl}/ordenes-pendientes`, this.getHeaders()).pipe(
            catchError(err => {
              console.error('Error recargando órdenes:', err);
              return of([] as any[]);
            })
          );
          const m$ = this.http.get<any[]>(`${this.apiUrl}/movimientos`, this.getHeaders()).pipe(
            catchError(err => {
              console.error('Error recargando movimientos:', err);
              return of([] as any[]);
            })
          );
          forkJoin([o$, m$]).subscribe({
            next: ([nuevasOrdenesRaw, nuevosMovimientosRaw]) => {
              const ordenes = (nuevasOrdenesRaw || []).map(o => ({
                id: o.id ?? o.Id,
                mesaId: o.mesaId ?? o.MesaId,
                meseroNombre: o.meseroNombre ?? o.MeseroNombre ?? (o.usuario ? (o.usuario.nombre ?? o.usuario.Nombre) : undefined),
                total: o.total ?? o.Total ?? o.TotalOrden,
                fechaCreacion: o.fechaCreacion ?? o.FechaCreacion ?? o.fecha,
                estado: o.estado ?? o.Estado,
                platos: o.platos ?? o.Platos ?? []
              })) as Orden[];

              const movimientos = (nuevosMovimientosRaw || []).map(m => ({
                id: m.id ?? m.Id,
                monto: m.monto ?? m.Monto,
                tipo: m.tipo ?? m.Tipo,
                fecha: m.fecha ?? m.Fecha ?? m.fechaPago ?? m.FechaPago,
                usuario: {
                  nombre: (m.usuario && (m.usuario.nombre ?? m.usuario.Nombre)) ?? (m.usuarioNombre ?? m.UsuarioNombre) ?? null
                },
                metodoPago: {
                  nombre: (m.metodoPago && (m.metodoPago.nombre ?? m.metodoPago.Nombre)) ?? (m.metodoPagoNombre ?? null)
                },
                descripcion: m.descripcion ?? m.Descripcion,
              })) as Movimiento[];

              this.ordenesSubject.next(ordenes);
              this.movimientosSubject.next(movimientos);
              delete this.metodoSeleccionadoPorOrden[orden.id];
              this.cdr.markForCheck();
            },
            error: err => console.error('Error en forkJoin recarga post-pago:', err)
          });
        }
        this.loadingSubject.next(false);
      })
    ).subscribe();
  }

  // Manejo de respuesta exitosa de pago (muestra factura en modal)
  private handlePagoSuccess(response: any) {
    if (response && response.factura) {
      this.facturaActual = response.factura;
      this.showFacturaModal = true;
      this.cdr.markForCheck();
    } else {
      // Si backend no devuelve la factura, continuar pero avisar
      console.warn('Pago registrado pero no se devolvió factura en la respuesta.');
    }
  }

  // Abrir nueva ventana con HTML de la factura y lanzar print()
  imprimirFactura() {
    if (!this.facturaActual) return;

    const f = this.facturaActual;
    const numero = f.NumeroFactura ?? f.numeroFactura ?? f.Numero ?? f.numero;
    const subtotal = Number(f.Subtotal ?? f.subtotal ?? 0).toFixed(2);
    const impuesto = Number(f.Impuesto ?? f.impuesto ?? 0).toFixed(2);
    const total = Number(f.Total ?? f.total ?? 0).toFixed(2);
    const cai = f.Cai ?? f.cai ?? '';
    const rango = f.RangoAutorizado ?? f.rangoAutorizado ?? '';
    const fechaLimite = f.FechaLimiteEmision ?? f.fechaLimiteEmision ?? f.FechaLimite ?? null;
    const fechaEmision = f.FechaEmision ?? f.fechaEmision ?? new Date().toISOString();

    const html = `
      <html>
        <head>
          <title>Factura ${numero}</title>
          <style>
            body { font-family: Arial, Helvetica, sans-serif; padding: 20px; }
            .header { text-align:center; margin-bottom: 10px; }
            .header h2 { margin: 0; }
            .meta { font-size: 12px; color: #444; margin-top: 6px; }
            .items { margin-top:20px; width:100%; border-collapse: collapse; font-size: 14px;}
            .items td, .items th { border: 1px solid #ddd; padding: 8px; }
            .totals { margin-top: 12px; float:right; font-size: 14px; }
            .small { font-size: 12px; color:#666; }
            @media print {
              body { -webkit-print-color-adjust: exact; }
            }
          </style>
        </head>
        <body>
          <div class="header">
            <h2>MegaDelicias</h2>
            <div class="meta">Factura: <strong>${numero}</strong></div>
            <div class="meta">Fecha: ${new Date(fechaEmision).toLocaleString()}</div>
            <div class="meta small">CAI: ${cai}</div>
          </div>

          <table class="items">
            <thead>
              <tr><th>Concepto</th><th>Valor</th></tr>
            </thead>
            <tbody>
              <tr><td>Subtotal</td><td>L. ${subtotal}</td></tr>
              <tr><td>Impuesto</td><td>L. ${impuesto}</td></tr>
              <tr><td>Total</td><td><strong>L. ${total}</strong></td></tr>
            </tbody>
          </table>

          <div class="totals small">
            <div>Rango autorizado: ${rango}</div>
            <div>Válida hasta: ${fechaLimite ? new Date(fechaLimite).toLocaleDateString() : ''}</div>
          </div>

          <script>
            window.onload = function() {
              setTimeout(() => { window.print(); }, 300);
            };
          </script>
        </body>
      </html>
    `;

    const w = window.open('', '_blank', 'width=800,height=600');
    if (w) {
      w.document.write(html);
      w.document.close();
    } else {
      alert('El navegador bloqueó la apertura de la ventana de impresión. Permite popups para este sitio.');
    }
  }

  setFiltro(filtro: string) {
    this.filtroActual = filtro;
    this.cdr.markForCheck(); // Forzar actualización de la vista
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

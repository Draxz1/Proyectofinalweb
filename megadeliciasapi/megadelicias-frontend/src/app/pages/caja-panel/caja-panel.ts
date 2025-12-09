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
  fechaActual = new Date();

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

  // Para hacer track de los detalles en la tabla
  trackByDetalle(index: number, detalle: any): string {
    const nombre = detalle.PlatoNombre || detalle.platoNombre || '';
    const cantidad = detalle.Cantidad || detalle.cantidad || 0;
    return `${nombre}-${cantidad}-${index}`;
  }
  getFechaActual() {
    return new Date();
  }
  //Inicializacion y polling
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
    console.log('🔍 Respuesta completa del pago:', response);
    
    if (response && response.factura) {
      console.log('📄 Factura recibida:', response.factura);
      console.log('📋 Detalles de la factura:', response.factura.Detalles || response.factura.detalles);
      
      // Actualizar fecha actual
      this.fechaActual = new Date();
      
      this.facturaActual = {
        // Información básica
        ...response.factura,
        
        // Asegurar que los detalles estén disponibles en ambas propiedades
        Detalles: response.factura.Detalles || response.factura.detalles || [],
        detalles: response.factura.detalles || response.factura.Detalles || [],
        
        // Asegurar otras propiedades en ambos formatos
        NumeroFactura: response.factura.NumeroFactura || response.factura.numeroFactura,
        numeroFactura: response.factura.numeroFactura || response.factura.NumeroFactura,
        FechaEmision: response.factura.FechaEmision || response.factura.fechaEmision,
        fechaEmision: response.factura.fechaEmision || response.factura.FechaEmision,
        Subtotal: response.factura.Subtotal || response.factura.subtotal,
        subtotal: response.factura.subtotal || response.factura.Subtotal,
        Impuesto: response.factura.Impuesto || response.factura.impuesto,
        impuesto: response.factura.impuesto || response.factura.Impuesto,
        Total: response.factura.Total || response.factura.total,
        total: response.factura.total || response.factura.Total,
        Cai: response.factura.Cai || response.factura.cai,
        cai: response.factura.cai || response.factura.Cai,
        RangoAutorizado: response.factura.RangoAutorizado || response.factura.rangoAutorizado,
        rangoAutorizado: response.factura.rangoAutorizado || response.factura.RangoAutorizado,
        FechaLimiteEmision: response.factura.FechaLimiteEmision || response.factura.fechaLimiteEmision,
        fechaLimiteEmision: response.factura.fechaLimiteEmision || response.factura.FechaLimiteEmision,
        Mesero: response.factura.Mesero || response.factura.mesero,
        mesero: response.factura.mesero || response.factura.Mesero,
        Mesa: response.factura.Mesa || response.factura.mesa,
        mesa: response.factura.mesa || response.factura.Mesa
      };
      
      console.log('✅ Factura actual procesada:', this.facturaActual);
      this.showFacturaModal = true;
      this.cdr.markForCheck();
    } else {
      console.warn('⚠️ Pago registrado pero no se devolvió factura completa');
      console.log('Respuesta recibida:', response);
      alert('Pago registrado exitosamente, pero no se pudo generar la factura completa.');
    }
  }

  // Abrir nueva ventana con HTML de la factura y lanzar print()
 imprimirFactura() {
  console.log('🖨️ Iniciando impresión de factura...');
  console.log('📊 Datos de facturaActual:', this.facturaActual);
  if (!this.facturaActual) {
  console.error('❌ No hay factura actual para imprimir');
  return;
}

  const f = this.facturaActual;
  console.log('🔍 Detalles disponibles:', f.Detalles || f.detalles);
  console.log('🔍 Número de detalles:', (f.Detalles || f.detalles)?.length);
  
  // Datos básicos de la factura - usando la nueva estructura
  const numero = f.NumeroFactura ?? f.numeroFactura ?? f.numero ?? '00000000';
  const subtotal = Number(f.Subtotal ?? f.subtotal ?? 0).toFixed(2);
  const impuesto = Number(f.Impuesto ?? f.impuesto ?? 0).toFixed(2);
  const total = Number(f.Total ?? f.total ?? 0).toFixed(2);
  const cai = f.Cai ?? f.cai ?? 'DEMO-CAI-000000000';
  const rango = f.RangoAutorizado ?? f.rangoAutorizado ?? '00000001-00001000';
  const fechaLimite = f.FechaLimiteEmision ?? f.fechaLimiteEmision ?? null;
  const fechaEmision = f.FechaEmision ?? f.fechaEmision ?? new Date().toISOString();
  
  // Nuevos campos de la factura - usando minúsculas primero (como viene del backend)
  const mesero = f.Mesero ?? f.mesero ?? 'No especificado';
  const mesa = f.Mesa ?? f.mesa ?? 'No especificado';
  
  // Detalles de productos - IMPORTANTE: usar la propiedad Detalles (con D mayúscula)
  const detalles = f.Detalles ?? f.detalles ?? [];
  
  console.log('Cantidad de detalles encontrados:', detalles.length);
  console.log('Detalles completos:', detalles);

  // Generar tabla de detalles
  let detallesHTML = '';
  if (detalles && detalles.length > 0) {
    let totalProductos = 0;
    detalles.forEach((detalle: any, index: number) => {
      // Usar propiedades en minúsculas como las envía el backend
      const nombre = detalle.platoNombre ?? detalle.PlatoNombre ?? 'Producto ' + (index + 1);
      const cantidad = detalle.cantidad ?? detalle.Cantidad ?? 0;
      const precioUnitario = Number(detalle.precioUnitario ?? detalle.PrecioUnitario ?? 0).toFixed(2);
      const subtotalItem = Number(detalle.subtotal ?? detalle.Subtotal ?? 0).toFixed(2);
      const nota = detalle.notaPlato ?? detalle.NotaPlato ?? '';
      
      totalProductos += cantidad;
      
      detallesHTML += `
        <tr>
          <td>${nombre}${nota ? `<br><small style="color: #666; font-style: italic;">Nota: ${nota}</small>` : ''}</td>
          <td class="text-center">${cantidad}</td>
          <td class="text-right">L. ${precioUnitario}</td>
          <td class="text-right">L. ${subtotalItem}</td>
        </tr>`;
    });
    
    // Agregar fila de resumen de productos
    detallesHTML += `
      <tr style="border-top: 2px solid #333;">
        <td colspan="4" class="text-right" style="padding-top: 10px;">
          <strong>Total productos: ${totalProductos}</strong>
        </td>
      </tr>`;
  } else {
    console.warn('No se encontraron detalles en la factura');
    detallesHTML = '<tr><td colspan="4" class="text-center" style="padding: 20px; color: #999;">No hay detalles del pedido</td></tr>';
  }

  const html = `
    <html>
      <head>
        <title>Factura ${numero} - MegaDelicias</title>
        <style>
          body { 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            padding: 25px;
            max-width: 800px;
            margin: 0 auto;
            background-color: #fff;
            color: #333;
            line-height: 1.4;
          }
          
          /* Encabezado */
          .header { 
            text-align: center; 
            margin-bottom: 25px;
            border-bottom: 3px solid #2c3e50;
            padding-bottom: 20px;
          }
          .header h1 { 
            margin: 0 0 10px 0; 
            font-size: 28px;
            color: #2c3e50;
            font-weight: 700;
          }
          .header h2 { 
            margin: 5px 0; 
            font-size: 18px;
            color: #7f8c8d;
            font-weight: 400;
          }
          .empresa-info {
            font-size: 13px;
            color: #7f8c8d;
            margin-top: 8px;
            line-height: 1.5;
          }
          
          /* Información de factura */
          .factura-info {
            display: flex;
            justify-content: space-between;
            margin: 20px 0;
            font-size: 13px;
            background: #f8f9fa;
            padding: 15px;
            border-radius: 8px;
            border: 1px solid #e9ecef;
          }
          .factura-info strong {
            color: #2c3e50;
          }
          
          /* Información del cliente */
          .cliente-info {
            background: #e8f4f8;
            padding: 15px;
            border-radius: 8px;
            margin: 20px 0;
            font-size: 13px;
            border-left: 4px solid #3498db;
          }
          .cliente-info div {
            margin: 4px 0;
          }
          
          /* Tabla de productos */
          .items { 
            margin-top: 25px; 
            width: 100%; 
            border-collapse: collapse; 
            font-size: 13px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
          }
          .items thead {
            background: #2c3e50;
            color: white;
          }
          .items th { 
            text-align: left;
            padding: 12px 15px;
            font-weight: 600;
            border: none;
          }
          .items td { 
            padding: 10px 15px;
            border-bottom: 1px solid #eee;
            vertical-align: top;
          }
          .items tbody tr:hover {
            background-color: #f9f9f9;
          }
          .items .text-center { text-align: center; }
          .items .text-right { text-align: right; }
          
          /* Totales */
          .totals { 
            margin-top: 30px;
            float: right;
            width: 320px;
            font-size: 15px;
          }
          .totals table {
            width: 100%;
            border-collapse: collapse;
            background: #f8f9fa;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
          }
          .totals td {
            padding: 12px 15px;
            border-bottom: 1px solid #e9ecef;
          }
          .totals .total-row {
            font-weight: 700;
            background: #2c3e50;
            color: white;
            font-size: 16px;
            border: none;
          }
          
          /* Pie de página */
          .footer { 
            margin-top: 50px;
            font-size: 11px;
            color: #7f8c8d;
            text-align: center;
            border-top: 1px solid #eee;
            padding-top: 15px;
            line-height: 1.6;
          }
          
          /* Logos/emblemas */
          .badge {
            display: inline-block;
            padding: 4px 8px;
            background: #e74c3c;
            color: white;
            border-radius: 4px;
            font-size: 10px;
            font-weight: bold;
            margin-left: 5px;
          }
          
          /* Estilos para impresión */
          @media print {
            body { 
              -webkit-print-color-adjust: exact !important;
              print-color-adjust: exact !important;
              padding: 15px;
              font-size: 12pt;
            }
            .no-print { 
              display: none !important; 
            }
            .header {
              border-bottom: 2px solid #000;
            }
            .items {
              box-shadow: none;
              border: 1px solid #ddd;
            }
            .items thead {
              background: #000 !important;
              color: #fff !important;
              -webkit-print-color-adjust: exact;
            }
            .totals .total-row {
              background: #000 !important;
              color: #fff !important;
              -webkit-print-color-adjust: exact;
            }
          }
          
          /* Estilos responsivos */
          @media (max-width: 600px) {
            body {
              padding: 15px;
            }
            .factura-info {
              flex-direction: column;
              gap: 10px;
            }
            .totals {
              float: none;
              width: 100%;
            }
          }
        </style>
      </head>
      <body>
        <!-- Encabezado -->
        <div class="header">
          <h1>MegaDelicias Restaurant</h1>
          <h2>¡Sabor que deleita!</h2>
          <div class="empresa-info">
            <div>📞 Teléfono: (504) 2234-5678</div>
            <div>📍 Dirección: Tegucigalpa, Honduras</div>
            <div>📋 RTN: 0801-1999-12345</div>
            <div>🏢 Registro Mercantil: 12345-2024</div>
          </div>
        </div>
        
        <!-- Información de la factura -->
        <div class="factura-info">
          <div>
            <strong>FACTURA No:</strong> <span style="font-size: 16px; font-weight: bold; color: #2c3e50;">${numero}</span><br>
            <strong>CAI:</strong> ${cai}<br>
            <strong>Fecha Emisión:</strong> ${new Date(fechaEmision).toLocaleString('es-HN', {
              year: 'numeric',
              month: '2-digit',
              day: '2-digit',
              hour: '2-digit',
              minute: '2-digit'
            })}
          </div>
          <div style="text-align: right;">
            <strong>Rango Autorizado:</strong><br>
            <span style="font-size: 11px;">${rango}</span><br><br>
            <strong>Válida hasta:</strong><br>
            ${fechaLimite ? new Date(fechaLimite).toLocaleDateString('es-HN', {
              year: 'numeric',
              month: 'long',
              day: 'numeric'
            }) : 'No especificada'}
          </div>
        </div>
        
        <!-- Información del cliente/mesa -->
        <div class="cliente-info">
          <div><strong>👨‍🍳 Atendido por:</strong> ${mesero}</div>
          <div><strong>🍽️ Mesa:</strong> ${mesa}</div>
          <div><strong>📅 Fecha:</strong> ${new Date(fechaEmision).toLocaleDateString('es-HN', { 
            weekday: 'long', 
            year: 'numeric', 
            month: 'long', 
            day: 'numeric' 
          })}</div>
          <div><strong>⏰ Hora:</strong> ${new Date(fechaEmision).toLocaleTimeString('es-HN', {
            hour: '2-digit',
            minute: '2-digit'
          })}</div>
        </div>
        
        <!-- Tabla de productos -->
        <table class="items">
          <thead>
            <tr>
              <th width="45%">Producto / Descripción</th>
              <th width="15%">Cantidad</th>
              <th width="20%">Precio Unitario</th>
              <th width="20%">Subtotal</th>
            </tr>
          </thead>
          <tbody>
            ${detallesHTML}
          </tbody>
        </table>
        
        <!-- Totales -->
        <div class="totals">
          <table>
            <tr>
              <td>Subtotal:</td>
              <td class="text-right">L. ${subtotal}</td>
            </tr>
            <tr>
              <td>Impuesto (15%):</td>
              <td class="text-right">L. ${impuesto}</td>
            </tr>
            <tr class="total-row">
              <td>TOTAL A PAGAR:</td>
              <td class="text-right">L. ${total}</td>
            </tr>
          </table>
        </div>
        
        <!-- Pie de página -->
        <div class="footer">
          <div style="margin-bottom: 10px; font-size: 12px;">
            <strong>Información importante:</strong>
          </div>
          <div>✅ Factura electrónica autorizada por la DEI</div>
          <div>✅ Este documento es un comprobante fiscal</div>
          <div>✅ Conserve este documento para cualquier aclaración</div>
          <div>✅ Original: Cliente | Copia: Establecimiento</div>
          <div style="margin-top: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px;">
            <strong>¡Gracias por su preferencia! <span style="color: #e74c3c;">❤️</span></strong><br>
            <em>Esperamos volver a servirle pronto</em>
          </div>
          <div style="margin-top: 15px; font-size: 10px; color: #95a5a6;">
            Sistema de gestión MegaDelicias v1.0 | Factura generada: ${new Date().toLocaleString('es-HN')}
          </div>
        </div>
        
        <!-- Botón de impresión (solo visible en navegador) -->
        <div class="no-print" style="margin-top: 40px; text-align: center; padding: 20px; background: #f8f9fa; border-radius: 10px;">
          <div style="margin-bottom: 15px; font-size: 14px; color: #7f8c8d;">
            ¿Listo para imprimir la factura?
          </div>
          <button onclick="window.print()" style="
            background: linear-gradient(to right, #27ae60, #2ecc71);
            color: white;
            border: none;
            padding: 12px 30px;
            border-radius: 8px;
            cursor: pointer;
            font-size: 16px;
            font-weight: 600;
            transition: all 0.3s;
            box-shadow: 0 4px 6px rgba(39, 174, 96, 0.2);
          "
          onmouseover="this.style.transform='translateY(-2px)'; this.style.boxShadow='0 6px 12px rgba(39, 174, 96, 0.3)';"
          onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow='0 4px 6px rgba(39, 174, 96, 0.2)';">
            🖨️ Imprimir Factura
          </button>
          <button onclick="window.close()" style="
            background: linear-gradient(to right, #e74c3c, #c0392b);
            color: white;
            border: none;
            padding: 12px 30px;
            border-radius: 8px;
            cursor: pointer;
            font-size: 16px;
            font-weight: 600;
            margin-left: 15px;
            transition: all 0.3s;
            box-shadow: 0 4px 6px rgba(231, 76, 60, 0.2);
          "
          onmouseover="this.style.transform='translateY(-2px)'; this.style.boxShadow='0 6px 12px rgba(231, 76, 60, 0.3)';"
          onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow='0 4px 6px rgba(231, 76, 60, 0.2)';">
            ❌ Cerrar Ventana
          </button>
          <div style="margin-top: 15px; font-size: 12px; color: #95a5a6;">
            La factura se autoimprimirá en 3 segundos...
          </div>
        </div>
        
        <script>
          // Auto-imprimir después de 3 segundos
          window.onload = function() {
            setTimeout(() => { 
              console.log('Auto-impresión iniciada...');
              window.print(); 
            }, 3000);
            
            // Opcional: cerrar después de 10 segundos si no se imprime
            setTimeout(() => { 
              if (!document.hasFocus()) {
                console.log('Cerrando ventana automáticamente...');
                window.close(); 
              }
            }, 10000);
          };
          
          // Detectar cuando se completa la impresión
          window.onafterprint = function() {
            console.log('Impresión completada');
            setTimeout(() => { 
              if (confirm('¿Desea cerrar la ventana de la factura?')) {
                window.close();
              }
            }, 1000);
          };
        </script>
      </body>
    </html>
  `;

  const w = window.open('', '_blank', 'width=950,height=700,scrollbars=yes,resizable=yes');
  if (w) {
    w.document.write(html);
    w.document.close();
    
    // Enfocar la ventana
    w.focus();
  } else {
    alert('El navegador bloqueó la apertura de la ventana de impresión. Por favor, permite popups para este sitio.');
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

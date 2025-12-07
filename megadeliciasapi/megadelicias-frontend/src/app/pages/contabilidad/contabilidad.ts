import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth';

interface CierreFecha {
  fecha: string;
  totalEfectivo: number;
  totalTarjeta: number;
  totalTransferencia: number;
  cajaInicial: number;
  efectivoContado: number;
  efectivoEsperado: number;
  diferencia: number;
  cuadro: boolean;
}

interface ResumenIngresosGastos {
  fecha: string;
  totalIngresos: number;
  totalGastos: number;
  resultado: number;
  estaEnNegativo: boolean;
}

interface BalanceGeneral {
  fechaCorte: string;
  activo: number;
  pasivo: number;
  patrimonioNeto: number;
  totalActivos: number;
  totalPasivoPatrimonio: number;
  cuadra: boolean;
}

interface LibroDiarioMovimiento {
  fecha: Date;
  tipo: string;
  monto: number;
}

interface LibroDiario {
  desde: string;
  hasta: string;
  movimientos: LibroDiarioMovimiento[];
  totalCargos: number;
  totalAbonos: number;
}

interface MayorMovimiento {
  fecha: Date;
  tipo: string;
  cargo: number;
  abono: number;
  saldo: number;
}

interface Mayor {
  cuenta: string;
  movimientos: MayorMovimiento[];
  totalCargos: number;
  totalAbonos: number;
  saldoFinal: number;
}

interface BalanzaComprobacionCuenta {
  cuenta: string;
  totalCargos: number;
  totalAbonos: number;
}

interface BalanzaComprobacion {
  desde: string;
  hasta: string;
  cuentas: BalanzaComprobacionCuenta[];
  totalCargos: number;
  totalAbonos: number;
  cuadra: boolean;
}

@Component({
  selector: 'app-contabilidad',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contabilidad.html',
  styleUrls: ['./contabilidad.css'],
})
export class ContabilidadComponent implements OnInit {

  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiBaseUrl = 'http://localhost:5143/api/Contabilidad';

  tabActual: 'cierre' | 'ingresos-gastos' | 'balance' | 'libro' | 'mayor' | 'balanza' = 'cierre';

  error = '';
  mensajeOk = '';

  cargandoCierreHoy = false;
  cargandoBusqueda = false;
  cargandoResumen = false;
  guardandoCierre = false;
  cargandoBalance = false;
  cargandoLibro = false;
  cargandoMayor = false;
  cargandoBalanza = false;

  cierreHoy: CierreFecha | null = null;
  cierreBuscado: CierreFecha | null = null;
  resumenIngresosGastos: ResumenIngresosGastos | null = null;
  balanceGeneral: BalanceGeneral | null = null;
  libroDiario: LibroDiario | null = null;
  mayor: Mayor | null = null;
  balanzaComprobacion: BalanzaComprobacion | null = null;

  fechaBusqueda: string = '';
  resumenFecha: string = '';
  resumenGastos: number | null = null;
  crearCierreFecha: string = '';
  crearCierreCajaInicial: number | null = null;
  crearCierreEfectivoContado: number | null = null;
  crearCierreObservaciones: string = '';
  fechaBalance: string = new Date().toISOString().split('T')[0];
  
  libroDesde: string = '';
  libroHasta: string = '';
  mayorDesde: string = '';
  mayorHasta: string = '';
  balanzaDesde: string = '';
  balanzaHasta: string = '';

  ngOnInit(): void {
    const hoy = new Date().toISOString().split('T')[0];
    const inicioMes = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0];

    this.crearCierreFecha = hoy;
    this.resumenFecha = hoy;
    this.libroDesde = inicioMes;
    this.libroHasta = hoy;
    this.mayorDesde = inicioMes;
    this.mayorHasta = hoy;
    this.balanzaDesde = inicioMes;
    this.balanzaHasta = hoy;

    this.cargarCierreHoy();
    this.cargarBalanceGeneral();
  }

  private getHeaders() {
    const token = this.authService.getToken();
    return {
      headers: new HttpHeaders({
        Authorization: `Bearer ${token ?? ''}`,
      }),
    };
  }

  private limpiarMensajes() {
    this.error = '';
    this.mensajeOk = '';
  }

  cambiarTab(tab: typeof this.tabActual) {
    this.tabActual = tab;
    this.limpiarMensajes();
  }

  cargarCierreHoy() {
    this.limpiarMensajes();
    this.cargandoCierreHoy = true;
    this.cierreHoy = null;

    this.http.get<CierreFecha>(`${this.apiBaseUrl}/cierre-diario`, this.getHeaders())
      .subscribe({
        next: (resp) => {
          this.cierreHoy = resp;
          this.cargandoCierreHoy = false;
        },
        error: (err) => {
          console.error('Error al cargar cierre diario', err);
          if (err.status === 404) {
            this.cierreHoy = null;
          } else {
            this.error = 'No se pudo cargar el cierre diario.';
          }
          this.cargandoCierreHoy = false;
        },
      });
  }

  buscarCierrePorFecha() {
    this.limpiarMensajes();
    this.cierreBuscado = null;

    if (!this.fechaBusqueda) {
      this.error = 'Debes seleccionar una fecha para buscar el cierre.';
      return;
    }

    this.cargandoBusqueda = true;

    this.http.get<CierreFecha>(`${this.apiBaseUrl}/cierre-por-fecha`, {
      ...this.getHeaders(),
      params: { fecha: this.fechaBusqueda },
    }).subscribe({
      next: (resp) => {
        this.cierreBuscado = resp;
        this.cargandoBusqueda = false;
      },
      error: (err) => {
        console.error('Error al buscar cierre por fecha', err);
        if (err.status === 404) {
          this.cierreBuscado = null;
          this.error = 'No se encontró cierre para la fecha seleccionada.';
        } else {
          this.error = 'No se pudo buscar el cierre por fecha.';
        }
        this.cargandoBusqueda = false;
      },
    });
  }

  crearCierre() {
    this.limpiarMensajes();

    if (this.crearCierreCajaInicial == null || this.crearCierreCajaInicial < 0) {
      this.error = 'Debes ingresar la caja inicial (no puede ser negativa).';
      return;
    }

    if (this.crearCierreEfectivoContado == null || this.crearCierreEfectivoContado < 0) {
      this.error = 'Debes ingresar el efectivo contado (no puede ser negativo).';
      return;
    }

    const token = this.authService.getToken();
    let usuarioId = 0;
    
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        usuarioId = parseInt(payload.sub || payload.nameid || '0');
      } catch (e) {
        console.error('Error al obtener usuario ID', e);
      }
    }

    if (usuarioId === 0) {
      this.error = 'No se pudo obtener el ID del usuario.';
      return;
    }

    this.guardandoCierre = true;

    const body = {
      usuarioId: usuarioId,
      fecha: this.crearCierreFecha,
      cajaInicial: this.crearCierreCajaInicial,
      efectivoContado: this.crearCierreEfectivoContado,
      observaciones: this.crearCierreObservaciones || null
    };

    this.http.post<any>(`${this.apiBaseUrl}/crear-cierre`, body, this.getHeaders())
      .subscribe({
        next: (resp) => {
          this.mensajeOk = resp.message || 'Cierre creado correctamente.';
          this.guardandoCierre = false;

          const hoy = new Date().toISOString().split('T')[0];
          if (this.crearCierreFecha === hoy) {
            this.cargarCierreHoy();
          }

          this.crearCierreCajaInicial = null;
          this.crearCierreEfectivoContado = null;
          this.crearCierreObservaciones = '';
        },
        error: (err) => {
          console.error('Error al crear cierre', err);
          if (err.status === 400 && err.error?.message) {
            this.error = err.error.message;
          } else {
            this.error = 'No se pudo crear el cierre.';
          }
          this.guardandoCierre = false;
        },
      });
  }

  calcularResumenIngresosGastos() {
    this.limpiarMensajes();
    this.resumenIngresosGastos = null;

    if (!this.resumenFecha) {
      this.error = 'No se pudo determinar la fecha del resumen.';
      return;
    }

    if (this.resumenGastos == null) {
      this.error = 'Debes ingresar el total de gastos.';
      return;
    }

    if (this.resumenGastos < 0) {
      this.error = 'Los gastos no pueden ser negativos.';
      return;
    }

    this.cargandoResumen = true;

    this.http.get<ResumenIngresosGastos>(`${this.apiBaseUrl}/resumen-ingresos-gastos`, {
      ...this.getHeaders(),
      params: {
        fecha: this.resumenFecha,
        gastos: String(this.resumenGastos),
      },
    }).subscribe({
      next: (resp) => {
        this.resumenIngresosGastos = resp;
        this.mensajeOk = 'Resumen calculado correctamente.';
        this.cargandoResumen = false;
      },
      error: (err) => {
        console.error('Error al calcular resumen', err);
        this.error = err.error?.message || 'No se pudo calcular el resumen.';
        this.cargandoResumen = false;
      },
    });
  }

  cargarBalanceGeneral() {
    this.limpiarMensajes();
    this.balanceGeneral = null;

    if (!this.fechaBalance) {
      this.error = 'Debes seleccionar la fecha del balance general.';
      return;
    }

    this.cargandoBalance = true;

    this.http.get<BalanceGeneral>(`${this.apiBaseUrl}/balance-general`, {
      ...this.getHeaders(),
      params: { fecha: this.fechaBalance },
    }).subscribe({
      next: (resp) => {
        this.balanceGeneral = resp;
        this.cargandoBalance = false;
      },
      error: (err) => {
        console.error('Error al obtener balance general', err);
        this.error = 'No se pudo obtener el balance general.';
        this.cargandoBalance = false;
      },
    });
  }

  cargarLibroDiario() {
    this.limpiarMensajes();
    this.libroDiario = null;

    if (!this.libroDesde || !this.libroHasta) {
      this.error = 'Debes seleccionar el rango de fechas.';
      return;
    }

    this.cargandoLibro = true;

    this.http.get<LibroDiario>(`${this.apiBaseUrl}/libro-diario`, {
      ...this.getHeaders(),
      params: { desde: this.libroDesde, hasta: this.libroHasta },
    }).subscribe({
      next: (resp) => {
        this.libroDiario = resp;
        this.cargandoLibro = false;
      },
      error: (err) => {
        console.error('Error al cargar libro diario', err);
        this.error = 'No se pudo cargar el libro diario.';
        this.cargandoLibro = false;
      },
    });
  }

  cargarMayor() {
    this.limpiarMensajes();
    this.mayor = null;

    if (!this.mayorDesde || !this.mayorHasta) {
      this.error = 'Debes seleccionar el rango de fechas.';
      return;
    }

    this.cargandoMayor = true;

    this.http.get<Mayor>(`${this.apiBaseUrl}/mayor`, {
      ...this.getHeaders(),
      params: { desde: this.mayorDesde, hasta: this.mayorHasta },
    }).subscribe({
      next: (resp) => {
        this.mayor = resp;
        this.cargandoMayor = false;
      },
      error: (err) => {
        console.error('Error al cargar mayor', err);
        this.error = 'No se pudo cargar el mayor.';
        this.cargandoMayor = false;
      },
    });
  }

  cargarBalanza() {
    this.limpiarMensajes();
    this.balanzaComprobacion = null;

    if (!this.balanzaDesde || !this.balanzaHasta) {
      this.error = 'Debes seleccionar el rango de fechas.';
      return;
    }

    this.cargandoBalanza = true;

    this.http.get<BalanzaComprobacion>(`${this.apiBaseUrl}/balanza-comprobacion`, {
      ...this.getHeaders(),
      params: { desde: this.balanzaDesde, hasta: this.balanzaHasta },
    }).subscribe({
      next: (resp) => {
        this.balanzaComprobacion = resp;
        this.cargandoBalanza = false;
      },
      error: (err) => {
        console.error('Error al cargar balanza', err);
        this.error = 'No se pudo cargar la balanza de comprobación.';
        this.cargandoBalanza = false;
      },
    });
  }

  get totalVentasHoy(): number {
    if (!this.cierreHoy) return 0;
    return this.cierreHoy.totalEfectivo + this.cierreHoy.totalTarjeta + this.cierreHoy.totalTransferencia;
  }

  get porcentajeCuadre(): number {
    if (!this.cierreHoy || this.cierreHoy.efectivoEsperado === 0) return 0;
    return Math.abs((this.cierreHoy.diferencia / this.cierreHoy.efectivoEsperado) * 100);
  }
}
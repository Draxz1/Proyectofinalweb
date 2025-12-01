import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { LucideAngularModule } from 'lucide-angular';
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

@Component({
  selector: 'app-contabilidad',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './contabilidad.html',
  styleUrls: ['./contabilidad.css'],
})
export class ContabilidadComponent implements OnInit {

  private http = inject(HttpClient);
  private authService = inject(AuthService);

  private apiBaseUrl = 'http://localhost:5143/api/Contabilidad';

  // Estado general
  error = '';
  mensajeOk = '';

  // Carga
  cargandoCierreHoy = false;
  cargandoBusqueda = false;
  cargandoResumen = false;
  guardandoCierre = false;
  cargandoBalance = false;

  // Datos
  cierreHoy: CierreFecha | null = null;
  cierreBuscado: CierreFecha | null = null;
  resumenIngresosGastos: ResumenIngresosGastos | null = null;
  balanceGeneral: BalanceGeneral | null = null;

  // Formularios
  fechaBusqueda: string = '';

  resumenFecha: string = '';
  resumenGastos: number | null = null;


  // Formulario para crear cierre
crearCierreFecha: string = '';
crearCierreCajaInicial: number | null = null;
crearCierreEfectivoContado: number | null = null;
crearCierreObservaciones: string = '';

  // Fecha del balance general
  fechaBalance: string = new Date().toISOString().split('T')[0];

 ngOnInit(): void {
  const hoy = new Date().toISOString().split('T')[0];

  this.crearCierreFecha = hoy;   // ya lo usábamos para el cierre
  this.resumenFecha = hoy;       // la misma fecha para el resumen

  this.cargarCierreHoy();
  this.cargarBalanceGeneral?.(); // si tienes balance general
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

  // ==========================
  // 1. Cierre diario (hoy)
  // ==========================
  cargarCierreHoy() {
    this.limpiarMensajes();
    this.cargandoCierreHoy = true;
    this.cierreHoy = null;

    this.http
      .get<CierreFecha>(`${this.apiBaseUrl}/cierre-diario`, this.getHeaders())
      .subscribe({
        next: (resp) => {
          this.cierreHoy = resp;
          this.cargandoCierreHoy = false;
        },
        error: (err) => {
          console.error('Error al cargar cierre diario', err);
          // Si no hay cierre, no es un error, simplemente no hay datos
          if (err.status === 404) {
            this.cierreHoy = null;
          } else {
            this.error = 'No se pudo cargar el cierre diario.';
          }
          this.cargandoCierreHoy = false;
        },
      });
  }

  // ==========================
  // 2. Cierre por fecha
  // ==========================
  buscarCierrePorFecha() {
    this.limpiarMensajes();
    this.cierreBuscado = null;

    if (!this.fechaBusqueda) {
      this.error = 'Debes seleccionar una fecha para buscar el cierre.';
      return;
    }

    this.cargandoBusqueda = true;

    this.http
      .get<CierreFecha>(
        `${this.apiBaseUrl}/cierre-por-fecha`,
        {
          ...this.getHeaders(),
          params: { fecha: this.fechaBusqueda },
        }
      )
      .subscribe({
        next: (resp) => {
          this.cierreBuscado = resp;
          this.cargandoBusqueda = false;
        },
        error: (err) => {
          console.error('Error al buscar cierre por fecha', err);
          // Si no hay cierre, no es un error, simplemente no hay datos
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

 // ==========================
// 3. Resumen ingresos vs gastos (UN SOLO DÍA)
// ==========================
calcularResumenIngresosGastos() {
  this.limpiarMensajes();
  this.resumenIngresosGastos = null;

  // La fecha del resumen siempre es un solo día (por defecto hoy)
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

  this.http
    .get<ResumenIngresosGastos>(
      `${this.apiBaseUrl}/resumen-ingresos-gastos`,
      {
        ...this.getHeaders(),
        params: {
          fecha: this.resumenFecha,                 
          gastos: String(this.resumenGastos),
        },
      }
    )
    .subscribe({
      next: (resp) => {
        this.resumenIngresosGastos = resp;
        this.mensajeOk = 'Resumen de ingresos y gastos calculado correctamente.';
        this.cargandoResumen = false;
      },
      error: (err) => {
        console.error('Error al calcular resumen de ingresos y gastos', err);
        if (err.status === 400 && err.error?.message) {
          this.error = err.error.message;
        } else {
          this.error = 'No se pudo calcular el resumen de ingresos y gastos.';
        }
        this.cargandoResumen = false;
      },
    });
}


  // ==========================
  // 4. Crear/Guardar cierre diario
  // ==========================
  crearCierre() {
  this.limpiarMensajes();

  // YA no preguntamos si eligió fecha, porque forzamos la de hoy
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
    fecha: this.crearCierreFecha,              // siempre hoy
    cajaInicial: this.crearCierreCajaInicial,
    efectivoContado: this.crearCierreEfectivoContado,
    observaciones: this.crearCierreObservaciones || null
  };

  this.http
    .post<any>(`${this.apiBaseUrl}/crear-cierre`, body, this.getHeaders())
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


  // ==========================
  // 5. Balance general
  // ==========================
  cargarBalanceGeneral() {
    this.limpiarMensajes();
    this.balanceGeneral = null;

    if (!this.fechaBalance) {
      this.error = 'Debes seleccionar la fecha del balance general.';
      return;
    }

    this.cargandoBalance = true;

    this.http
      .get<BalanceGeneral>(
        `${this.apiBaseUrl}/balance-general`,
        {
          ...this.getHeaders(),
          params: { fecha: this.fechaBalance },
        }
      )
      .subscribe({
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
}

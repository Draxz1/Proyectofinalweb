import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface ResumenContable {
  fechaInicio: string;
  fechaFin: string;
  totalVentas: number;
  totalFacturado: number;
  totalImpuesto: number;
  comentario: string;
}

interface CierreContableSolicitud {
  fechaCierre: string;      // en formato yyyy-MM-dd
  usuario: string;
  observaciones: string;
}

interface CierreContableRespuesta {
  fechaCierre: string;
  usuario: string;
  totalVentasCerradas: number;
  mensaje: string;
}

@Component({
  selector: 'app-contabilidad',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './contabilidad.html',
  styleUrl: './contabilidad.css',
})
export class ContabilidadComponent implements OnInit {

  private http = inject(HttpClient);

  // 👇 ajusta el puerto si tu API corre en otro
  private apiBaseUrl = 'http://localhost:5143/api/Contabilidad';

  cargandoResumen = false;
  cargandoCierre = false;
  error = '';
  mensajeOk = '';

  resumen: ResumenContable | null = null;
  cierreRespuesta: CierreContableRespuesta | null = null;

  // campos del formulario de cierre
  cierreFecha: string = '';
  cierreObservaciones: string = '';

  ngOnInit(): void {
    this.cargarResumenDiario();
  }

  private getAuthOptions() {
    const token = localStorage.getItem('token'); // o como se guarde en tu app
    if (!token) return {};

    return {
      headers: new HttpHeaders({
        Authorization: `Bearer ${token}`,
      }),
    };
  }

  cargarResumenDiario() {
    this.cargandoResumen = true;
    this.error = '';
    this.mensajeOk = '';

    this.http
      .get<ResumenContable>(`${this.apiBaseUrl}/resumen-diario`, this.getAuthOptions())
      .subscribe({
        next: (data) => {
          this.resumen = data;
          this.cargandoResumen = false;
        },
        error: (err) => {
          console.error('Error al cargar resumen contable:', err);
          this.error = 'No se pudo cargar el resumen contable.';
          this.cargandoResumen = false;
        },
      });
  }

  registrarCierre() {
    this.cargandoCierre = true;
    this.error = '';
    this.mensajeOk = '';
    this.cierreRespuesta = null;

    if (!this.cierreFecha) {
      this.error = 'Debe seleccionar una fecha de cierre.';
      this.cargandoCierre = false;
      return;
    }

    const solicitud: CierreContableSolicitud = {
      fechaCierre: this.cierreFecha,  // el input date ya viene como yyyy-MM-dd
      usuario: 'admin',               // puedes cambiarlo según tu lógica
      observaciones: this.cierreObservaciones || '',
    };

    this.http
      .post<CierreContableRespuesta>(`${this.apiBaseUrl}/cierre`, solicitud, this.getAuthOptions())
      .subscribe({
        next: (resp) => {
          this.cierreRespuesta = resp;
          this.mensajeOk = 'Cierre contable registrado correctamente.';
          this.cargandoCierre = false;
        },
        error: (err) => {
          console.error('Error al registrar cierre:', err);
          this.error = 'No se pudo registrar el cierre contable.';
          this.cargandoCierre = false;
        },
      });
  }
}


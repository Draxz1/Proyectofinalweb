import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth';

interface Producto {
  id: number;
  nombre: string;
  descripcion: string | null;
  categoria: string;
  stock: number;
  stockMinimo: number;
  precioUnitario: number; // ✅ CORREGIDO
  unidadMedida: string;
  activo: boolean;
  fechaCreacion: Date;
  fechaActualizacion: Date | null;
}

interface Movimiento {
  id: number;
  fecha: Date;
  productoNombre: string;
  tipo: string;
  cantidad: number;
  costoUnitario: number;
  motivo: string;
}

interface RegistrarMovimientoDto {
  productoId: number;
  tipo: 'Entrada' | 'Salida';
  cantidad: number;
  costoUnitario: number | null;
  motivo: string;
  usuarioId: number;
}

interface CrearProductoDto {
  codigo: string;
  nombre: string;
  categoria: string;
  stock: number;
  stockMinimo: number;
  costoUnitario: number;
  unidadMedida: string;
  descripcion?: string;
}

@Component({
  selector: 'app-inventario',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventario-panel.html',
  styleUrls: ['./inventario-panel.css'],
})
export class InventarioComponent implements OnInit {
  
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiBaseUrl = 'http://localhost:5143/api/Inventario';

  // Estado
  error = '';
  mensajeOk = '';
  cargando = false;
  mostrarModalNuevo = false;

  // Datos
  productos: Producto[] = [];
  movimientos: Movimiento[] = [];
  productoSeleccionado: Producto | null = null;

  // Filtros
  busqueda: string = '';

  // Formulario de movimiento
  movimientoForm: RegistrarMovimientoDto = {
    productoId: 0,
    tipo: 'Entrada',
    cantidad: 1,
    costoUnitario: null,
    motivo: '',
    usuarioId: 0
  };

  // Formulario nuevo producto
  nuevoProducto: CrearProductoDto = {
    codigo: '',
    nombre: '',
    categoria: '',
    stock: 0,
    stockMinimo: 0,
    costoUnitario: 0,
    unidadMedida: 'Unidad',
    descripcion: ''
  };

  ngOnInit(): void {
    this.cargarProductos();
    this.cargarMovimientos();
    this.movimientoForm.usuarioId = this.obtenerUsuarioId();
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

  private obtenerUsuarioId(): number {
    const token = this.authService.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        return parseInt(payload.sub || payload.nameid || '0');
      } catch (e) {
        return 0;
      }
    }
    return 0;
  }

  // ==========================
  // CARGAR DATOS
  // ==========================
  cargarProductos() {
    this.cargando = true;
    this.http.get<Producto[]>(`${this.apiBaseUrl}/productos`, this.getHeaders())
      .subscribe({
        next: (resp) => {
          this.productos = resp;
          this.cargando = false;
        },
        error: (err) => {
          console.error('Error al cargar productos', err);
          this.error = 'No se pudo cargar el inventario.';
          this.cargando = false;
        },
      });
  }

  cargarMovimientos() {
    this.http.get<Movimiento[]>(`${this.apiBaseUrl}/movimientos-recientes?cantidad=20`, this.getHeaders())
      .subscribe({
        next: (resp) => {
          this.movimientos = resp;
        },
        error: (err) => {
          console.error('Error al cargar movimientos', err);
        },
      });
  }

  // ==========================
  // REGISTRAR MOVIMIENTO
  // ==========================
  onProductoSeleccionado() {
    const producto = this.productos.find(p => p.id === this.movimientoForm.productoId);
    if (producto) {
      this.productoSeleccionado = producto;
      if (this.movimientoForm.tipo === 'Entrada' && this.movimientoForm.costoUnitario === null) {
        this.movimientoForm.costoUnitario = producto.precioUnitario;
      }
    }
  }

  onTipoChanged() {
    if (this.movimientoForm.tipo === 'Salida') {
      this.movimientoForm.costoUnitario = null;
    } else if (this.productoSeleccionado) {
      this.movimientoForm.costoUnitario = this.productoSeleccionado.precioUnitario;
    }
  }

  guardarMovimiento() {
    this.limpiarMensajes();

    if (!this.movimientoForm.productoId) {
      this.error = 'Debe seleccionar un producto';
      return;
    }

    if (this.movimientoForm.cantidad <= 0) {
      this.error = 'La cantidad debe ser mayor a 0';
      return;
    }

    if (this.movimientoForm.tipo === 'Entrada' && (!this.movimientoForm.costoUnitario || this.movimientoForm.costoUnitario <= 0)) {
      this.error = 'El costo unitario es requerido para entradas';
      return;
    }

    this.cargando = true;

    this.http.post<any>(`${this.apiBaseUrl}/registrar-movimiento`, this.movimientoForm, this.getHeaders())
      .subscribe({
        next: (resp) => {
          this.mensajeOk = resp.message || 'Movimiento registrado exitosamente';
          this.resetFormularioMovimiento();
          this.cargarProductos();
          this.cargarMovimientos();
          this.cargando = false;
        },
        error: (err) => {
          console.error('Error al registrar movimiento', err);
          this.error = err.error?.message || 'No se pudo registrar el movimiento';
          this.cargando = false;
        },
      });
  }

  resetFormularioMovimiento() {
    this.movimientoForm = {
      productoId: 0,
      tipo: 'Entrada',
      cantidad: 1,
      costoUnitario: null,
      motivo: '',
      usuarioId: this.obtenerUsuarioId()
    };
    this.productoSeleccionado = null;
  }

  // ==========================
  // CREAR PRODUCTO
  // ==========================
  abrirModalNuevo() {
    this.mostrarModalNuevo = true;
    this.limpiarMensajes();
  }

  cerrarModalNuevo() {
    this.mostrarModalNuevo = false;
    this.nuevoProducto = {
      codigo: '',
      nombre: '',
      categoria: '',
      stock: 0,
      stockMinimo: 0,
      costoUnitario: 0,
      unidadMedida: 'Unidad',
      descripcion: ''
    };
  }

  guardarNuevoProducto() {
    this.limpiarMensajes();

    if (!this.nuevoProducto.codigo || !this.nuevoProducto.nombre || !this.nuevoProducto.categoria) {
      this.error = 'Código, nombre y categoría son obligatorios';
      return;
    }

    if (this.nuevoProducto.costoUnitario <= 0) {
      this.error = 'El costo unitario debe ser mayor a 0';
      return;
    }

    this.cargando = true;

    this.http.post<any>(`${this.apiBaseUrl}/crear-producto`, this.nuevoProducto, this.getHeaders())
      .subscribe({
        next: (resp) => {
          this.mensajeOk = resp.message || 'Producto creado exitosamente';
          this.cerrarModalNuevo();
          this.cargarProductos();
          this.cargando = false;
        },
        error: (err) => {
          console.error('Error al crear producto', err);
          this.error = err.error?.message || 'No se pudo crear el producto';
          this.cargando = false;
        },
      });
  }

  // ==========================
  // FILTROS
  // ==========================
  get productosFiltrados(): Producto[] {
    if (!this.busqueda) return this.productos;
    
    const busquedaLower = this.busqueda.toLowerCase();
    return this.productos.filter(p => 
      p.nombre.toLowerCase().includes(busquedaLower) ||
      p.categoria.toLowerCase().includes(busquedaLower)
    );
  }

  // ==========================
  // UTILIDADES
  // ==========================
  calcularTotal(producto: Producto): number {
    return producto.stock * producto.precioUnitario;
  }

  getStockClass(producto: Producto): string {
    if (producto.stock <= producto.stockMinimo) {
      return 'text-red-600 font-bold';
    }
    return 'text-green-600 font-bold';
  }

  getTipoClass(tipo: string): string {
    return tipo === 'Entrada' || tipo === 'ENTRADA' ? 
      'bg-green-100 text-green-700' : 
      'bg-red-100 text-red-700';
  }
}
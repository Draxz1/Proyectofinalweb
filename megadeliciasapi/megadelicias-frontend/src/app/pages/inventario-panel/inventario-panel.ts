import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { FormsModule } from '@angular/forms'; 
// Al renombrar el archivo en el paso 1, este import funcionará:
import { InventarioService } from '../../services/inventario';

@Component({
  selector: 'app-inventario-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventario-panel.html'
})
export class InventarioPanelComponent implements OnInit {
  // --- LISTAS DE DATOS ---
  inventarioItems: any[] = [];
  movimientosRecientes: any[] = [];
  categorias: any[] = []; 

  // --- VARIABLES DE CONTROL UI ---
  busqueda: string = '';
  mensaje: string = '';
  error: string = '';
  itemSeleccionado: any = null; 
  mostrarModal: boolean = false; 

  // --- OBJETO PARA FORMULARIO MOVIMIENTOS ---
  nuevoMovimiento = {
    itemId: 0,
    tipo: 'Entrada',
    cantidad: 1,
    costoUnitario: 0,
    motivo: ''
  };

  // --- OBJETO PARA FORMULARIO NUEVO PRODUCTO ---
  nuevoProducto = {
    codigo: '',
    nombre: '',
    categoriaId: 0,
    unidadMedida: 'Unidad',
    stockMinimo: 5
  };

  constructor(private inventarioService: InventarioService) {}

  ngOnInit(): void {
    this.cargarDatos();
    this.cargarCategorias(); 
  }

  // 1. CARGA DE DATOS GENERALES
  cargarDatos() {
    // Inventario
    this.inventarioService.getInventario(this.busqueda).subscribe({
      next: (data: any) => { // <--- CORRECCIÓN: Agregado : any
        this.inventarioItems = data;
        if (this.itemSeleccionado) {
          const actualizado = data.find((x: any) => x.id === this.itemSeleccionado.id); // <--- CORRECCIÓN
          if (actualizado) this.itemSeleccionado = actualizado;
        }
      },
      error: (err: any) => console.error('Error Inventario:', err) // <--- CORRECCIÓN
    });

    // Historial
    this.inventarioService.getMovimientosRecientes().subscribe({
      next: (data: any) => this.movimientosRecientes = data, // <--- CORRECCIÓN
      error: (err: any) => console.error('Error Movimientos:', err) // <--- CORRECCIÓN
    });
  }

  // 2. CARGAR CATEGORÍAS
  cargarCategorias() {
    this.inventarioService.getCategorias().subscribe({
      next: (data: any) => this.categorias = data, // <--- CORRECCIÓN
      error: (err: any) => console.error('Error Categorías:', err) // <--- CORRECCIÓN
    });
  }

  // 3. LÓGICA TARJETA DE DETALLES
  onProductoSeleccionado() {
    const id = Number(this.nuevoMovimiento.itemId);
    // CORRECCIÓN: x: any
    this.itemSeleccionado = this.inventarioItems.find((x: any) => x.id === id);

    if (this.itemSeleccionado) {
      this.nuevoMovimiento.costoUnitario = this.itemSeleccionado.costoUnitario;
    } else {
      this.nuevoMovimiento.costoUnitario = 0;
    }
  }

  // 4. GUARDAR MOVIMIENTO
  guardarMovimiento() {
    this.error = '';
    this.mensaje = '';

    if (Number(this.nuevoMovimiento.itemId) === 0) {
      this.error = '⚠️ Selecciona un producto.';
      return;
    }
    if (this.nuevoMovimiento.cantidad <= 0) {
      this.error = '⚠️ La cantidad debe ser mayor a 0.';
      return;
    }

    this.inventarioService.registrarMovimiento(this.nuevoMovimiento).subscribe({
      next: () => {
        this.mensaje = '✅ Movimiento registrado con éxito';
        this.cargarDatos(); 
        this.nuevoMovimiento.cantidad = 1;
        this.nuevoMovimiento.motivo = '';
        setTimeout(() => this.mensaje = '', 3000);
      },
      error: (err: any) => { // <--- CORRECCIÓN
        this.error = err.error?.mensaje || '❌ Error al guardar movimiento.';
      }
    });
  }

  // 5. FUNCIONES DEL MODAL
  abrirModalCrear() {
    this.mostrarModal = true;
    this.nuevoProducto = { codigo: '', nombre: '', categoriaId: 0, unidadMedida: 'Unidad', stockMinimo: 5 };
  }

  cerrarModal() {
    this.mostrarModal = false;
  }

  guardarNuevoProducto() {
    if (!this.nuevoProducto.nombre || this.nuevoProducto.categoriaId === 0) {
      alert('⚠️ Completa el Nombre y la Categoría.');
      return;
    }

    this.inventarioService.crearItem(this.nuevoProducto).subscribe({
      next: () => {
        this.mensaje = '✨ Nuevo producto creado exitosamente';
        this.cerrarModal();
        this.cargarDatos(); 
        setTimeout(() => this.mensaje = '', 3000);
      },
      error: (err: any) => alert('Error: ' + (err.error?.mensaje || 'No se pudo crear.')) // <--- CORRECCIÓN
    });
  }
}
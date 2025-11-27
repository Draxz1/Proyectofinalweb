import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface InventarioItem {
  id: number;
  codigo: string;
  nombre: string;
  stockActual: number;
  stockMinimo: number;
  costoUnitario: number;
  unidadMedida: string;
  activo: boolean;
  creadoEn: string;
  categoriaId: number;
}

interface Categoria {
  id: number;
  nombre: string;
}

interface Movimiento {
  id?: number;
  itemId: number;
  itemNombre?: string;
  tipo: 'Entrada' | 'Salida';
  cantidad: number;
  costoUnitario?: number;
  motivo: string;
  fecha?: string;
}

@Component({
  selector: 'app-inventario-panel',
  templateUrl: './inventario-panel.html',
  styleUrls: ['./inventario-panel.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class InventarioPanelComponent implements OnInit {

  inventario: InventarioItem[] = [];
  categorias: Categoria[] = [];
  movimientosRecientes: Movimiento[] = [];
  filtro: string = "";
  cargando: boolean = false;

  nuevoMovimiento: Movimiento = { itemId: 0, tipo: 'Entrada', cantidad: 1, costoUnitario: 0, motivo: '' };
  itemSeleccionado: InventarioItem | null = null;
  nuevoProducto: Partial<InventarioItem> & { categoriaId: number, unidadMedida: string, stockMinimo: number } = {
    codigo: '', nombre: '', categoriaId: 0, unidadMedida: 'Unidad', stockMinimo: 0
  };
  mostrarModal = false;

  apiUrl = "https://localhost:7013/api/Inventario";

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarInventario();
    this.cargarCategorias();
    this.cargarMovimientosRecientes();
  }

  cargarInventario() {
    this.cargando = true;
    this.http.get<InventarioItem[]>(this.apiUrl)
      .subscribe({
        next: data => { this.inventario = data; this.cargando = false; },
        error: err => { console.error(err); this.cargando = false; }
      });
  }

  cargarCategorias() {
    this.http.get<Categoria[]>(this.apiUrl + '/categorias')
      .subscribe({ next: data => this.categorias = data, error: err => console.error(err) });
  }

  cargarMovimientosRecientes() {
    this.http.get<Movimiento[]>(this.apiUrl + '/movimientos-recientes')
      .subscribe({ next: data => this.movimientosRecientes = data, error: err => console.error(err) });
  }

  get inventarioFiltrado() {
    return this.inventario.filter(item =>
      item.nombre.toLowerCase().includes(this.filtro.toLowerCase()) ||
      (item.codigo ?? "").toLowerCase().includes(this.filtro.toLowerCase())
    );
  }

  get inventarioItems() {
    return this.inventarioFiltrado.map(item => ({
      ...item,
      valorTotal: item.costoUnitario * item.stockActual,
      categoria: this.getNombreCategoria(item.categoriaId)
    }));
  }
  getCategoriaSeleccionado(): string {
  if (!this.itemSeleccionado) return '';
  const cat = this.categorias.find(c => c.id === this.itemSeleccionado!.categoriaId);
  return cat ? cat.nombre : 'Sin categoría';
}

  getNombreCategoria(id: number) {
    const cat = this.categorias.find(c => c.id === id);
    return cat ? cat.nombre : 'Sin categoría';
  }

  editarItem(item: InventarioItem) { alert('Editar: ' + item.nombre); }

  eliminarItem(id: number) {
    if (!confirm('¿Seguro que deseas eliminar este producto?')) return;
    this.http.delete(`${this.apiUrl}/${id}`).subscribe({
      next: () => { alert('Producto eliminado'); this.cargarInventario(); },
      error: err => { console.error(err); alert('No se pudo eliminar el producto.'); }
    });
  }

  onProductoSeleccionado() {
    this.itemSeleccionado = this.inventario.find(i => i.id === this.nuevoMovimiento.itemId) || null;
  }

  abrirModalCrear() { this.mostrarModal = true; }
  cerrarModal() { this.mostrarModal = false; }

  guardarMovimiento() {
    if (!this.nuevoMovimiento.itemId) { alert('Selecciona un producto'); return; }
    const item = this.inventario.find(i => i.id === this.nuevoMovimiento.itemId);
    if (!item) return;

    // Ajustar stock
    if (this.nuevoMovimiento.tipo === 'Entrada') {
      item.stockActual += this.nuevoMovimiento.cantidad;
      item.costoUnitario = this.nuevoMovimiento.costoUnitario || item.costoUnitario;
    } else {
      item.stockActual -= this.nuevoMovimiento.cantidad;
    }

    // Guardar movimiento
    this.http.post(this.apiUrl + '/movimiento', this.nuevoMovimiento)
      .subscribe({
        next: () => { this.cargarMovimientosRecientes(); this.nuevoMovimiento = { itemId: 0, tipo: 'Entrada', cantidad: 1, costoUnitario: 0, motivo: '' }; this.itemSeleccionado = null; },
        error: err => console.error(err)
      });
  }

  guardarNuevoProducto() {
    this.http.post(this.apiUrl, this.nuevoProducto)
      .subscribe({
        next: () => { this.cargarInventario(); this.cerrarModal(); this.nuevoProducto = { codigo:'', nombre:'', categoriaId:0, unidadMedida:'Unidad', stockMinimo:0 }; },
        error: err => console.error(err)
      });
  }
}

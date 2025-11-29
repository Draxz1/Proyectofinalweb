import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
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
  categoria?: string;
  valorTotal?: number;
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

  nuevoMovimiento: Movimiento = { 
    itemId: 0, 
    tipo: 'Entrada', 
    cantidad: 1, 
    costoUnitario: 0, 
    motivo: '' 
  };
  
  itemSeleccionado: InventarioItem | null = null;
  
  nuevoProducto: Partial<InventarioItem> & { 
    categoriaId: number, 
    unidadMedida: string, 
    stockMinimo: number 
  } = {
    codigo: '', 
    nombre: '', 
    categoriaId: 0, 
    unidadMedida: 'Unidad', 
    stockMinimo: 0
  };
  
  mostrarModal = false;

  // ✅ URL CORREGIDA - Usa el mismo puerto que cocina
  apiUrl = "http://localhost:5143/api/Inventario";

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    console.log('🔍 Inicializando Inventario Panel...');
    console.log('📡 API URL:', this.apiUrl);
    this.cargarInventario();
    this.cargarCategorias();
    this.cargarMovimientosRecientes();
  }

  // ✅ Método mejorado con logs de depuración
  cargarInventario() {
    this.cargando = true;
    console.log('📦 Cargando inventario desde:', this.apiUrl);
    
    this.http.get<InventarioItem[]>(this.apiUrl)
      .subscribe({
        next: (data) => {
          console.log('✅ Inventario cargado:', data);
          this.inventario = data;
          this.cargando = false;
        },
        error: (err) => {
          console.error('❌ Error cargando inventario:', err);
          console.error('📄 Detalles del error:', {
            status: err.status,
            statusText: err.statusText,
            message: err.message,
            url: err.url
          });
          this.cargando = false;
          
          // Mensaje de error al usuario
          if (err.status === 0) {
            alert('❌ Error de conexión: No se pudo conectar con el servidor.\n\nVerifica que el backend esté corriendo en: http://localhost:5143');
          } else if (err.status === 401) {
            alert('❌ No autorizado: Tu sesión ha expirado. Por favor, inicia sesión nuevamente.');
          } else {
            alert(`❌ Error al cargar inventario: ${err.message}`);
          }
        }
      });
  }

  cargarCategorias() {
    console.log('📂 Cargando categorías...');
    this.http.get<Categoria[]>(this.apiUrl + '/categorias')
      .subscribe({ 
        next: (data) => {
          console.log('✅ Categorías cargadas:', data);
          this.categorias = data;
        },
        error: (err) => {
          console.error('❌ Error cargando categorías:', err);
        }
      });
  }

  cargarMovimientosRecientes() {
    console.log('📋 Cargando movimientos recientes...');
    this.http.get<Movimiento[]>(this.apiUrl + '/movimientos')
      .subscribe({ 
        next: (data) => {
          console.log('✅ Movimientos cargados:', data);
          this.movimientosRecientes = data;
        },
        error: (err) => {
          console.error('❌ Error cargando movimientos:', err);
        }
      });
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

  editarItem(item: InventarioItem) { 
    alert('Editar: ' + item.nombre); 
  }

  eliminarItem(id: number) {
    if (!confirm('¿Seguro que deseas eliminar este producto?')) return;
    this.http.delete(`${this.apiUrl}/${id}`).subscribe({
      next: () => { 
        alert('Producto eliminado'); 
        this.cargarInventario(); 
      },
      error: (err) => { 
        console.error(err); 
        alert('No se pudo eliminar el producto.'); 
      }
    });
  }

  onProductoSeleccionado() {
    this.itemSeleccionado = this.inventario.find(i => i.id === this.nuevoMovimiento.itemId) || null;
    console.log('🎯 Producto seleccionado:', this.itemSeleccionado);
  }

  abrirModalCrear() { 
    this.mostrarModal = true; 
  }

  cerrarModal() { 
    this.mostrarModal = false; 
  }

  guardarMovimiento() {
    if (!this.nuevoMovimiento.itemId) { 
      alert('Selecciona un producto'); 
      return; 
    }
    
    console.log('💾 Guardando movimiento:', this.nuevoMovimiento);
    
    this.http.post(this.apiUrl + '/movimiento', this.nuevoMovimiento)
      .subscribe({
        next: () => { 
          console.log('✅ Movimiento guardado');
          this.cargarInventario();
          this.cargarMovimientosRecientes(); 
          this.nuevoMovimiento = { 
            itemId: 0, 
            tipo: 'Entrada', 
            cantidad: 1, 
            costoUnitario: 0, 
            motivo: '' 
          }; 
          this.itemSeleccionado = null;
          alert('✅ Movimiento registrado correctamente');
        },
        error: (err) => {
          console.error('❌ Error guardando movimiento:', err);
          alert('❌ Error al guardar el movimiento');
        }
      });
  }

  guardarNuevoProducto() {
    console.log('💾 Guardando nuevo producto:', this.nuevoProducto);
    
    this.http.post(this.apiUrl, this.nuevoProducto)
      .subscribe({
        next: () => { 
          console.log('✅ Producto creado');
          this.cargarInventario(); 
          this.cerrarModal(); 
          this.nuevoProducto = { 
            codigo:'', 
            nombre:'', 
            categoriaId:0, 
            unidadMedida:'Unidad', 
            stockMinimo:0 
          };
          alert('✅ Producto creado correctamente');
        },
        error: (err) => {
          console.error('❌ Error creando producto:', err);
          alert('❌ Error al crear el producto');
        }
      });
  }
}
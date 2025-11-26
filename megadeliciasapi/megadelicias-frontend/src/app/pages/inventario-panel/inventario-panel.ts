import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

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

@Component({
  selector: 'app-inventario-panel',
  templateUrl: './inventario-panel.html',
  styleUrls: ['./inventario-panel.css'],
})
export class InventarioPanelComponent implements OnInit {

  inventario: InventarioItem[] = [];
  filtro: string = "";
  cargando: boolean = false;

  apiUrl = "https://localhost:7013/api/Inventario";

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarInventario();
  }

  cargarInventario() {
    this.cargando = true;

    this.http.get<InventarioItem[]>(this.apiUrl)
      .subscribe({
        next: data => {
          this.inventario = data;
          this.cargando = false;
        },
        error: err => {
          console.error("Error cargando inventario:", err);
          this.cargando = false;
        }
      });
  }

  get inventarioFiltrado() {
    return this.inventario.filter(item =>
      item.nombre.toLowerCase().includes(this.filtro.toLowerCase()) ||
      (item.codigo ?? "").toLowerCase().includes(this.filtro.toLowerCase())
    );
  }

  editarItem(item: InventarioItem) {
    console.log("Editar → ", item);
    alert("Aquí vas a abrir un formulario o modal para editar:\n\n" + item.nombre);
  }

  eliminarItem(id: number) {
    if (!confirm("¿Seguro que deseas eliminar este producto?")) return;

    this.http.delete(`${this.apiUrl}/${id}`).subscribe({
      next: () => {
        alert("Producto eliminado correctamente.");
        this.cargarInventario(); // refrescar lista
      },
      error: err => {
        console.error("Error al eliminar:", err);
        alert("No se pudo eliminar el producto.");
      }
    });
  }
}

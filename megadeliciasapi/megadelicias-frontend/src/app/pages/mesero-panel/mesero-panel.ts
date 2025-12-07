import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth';
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-mesero-panel',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    LucideAngularModule
  ],
  templateUrl: './mesero-panel.html',
  styleUrl: './mesero-panel.css'
})
export class MeseroPanelComponent implements OnInit, OnDestroy {
  
  // --- VARIABLES DE ESTADO ---
  vistaActual: 'TOMAR_ORDEN' | 'ENTREGAS' = 'TOMAR_ORDEN';
  
  // Datos
  platos: any[] = [];
  mesas: any[] = [];
  items: any[] = []; 
  ordenesListas: any[] = []; // ✅ Lista de órdenes listas para entregar
  
  // Filtros
  buscar: string = "";
  cat: string = "Todas";
  categoriasFijas: string[] = ["Desayuno", "Almuerzo y Cena", "Bebidas", "Postres"];
  
  mapaCategorias: Record<string, string[]> = {
    "Desayuno": ["desayuno", "huevos", "omelet", "tostadas", "panqueques", "tipico", "frijoles"],
    "Almuerzo y Cena": [
      "almuerzo", "cena", "plato", "fuerte", "carne", "pollo", "cerdo",
      "bistec", "filete", "chuleta", "costilla", "pescado", "sopa", "marisco", "camaron",
      "hamburguesa", "burger", "alitas", "wings", "finger", "dedos", "baleada", "tacos", "flautas", "enchiladas"
    ],
    "Bebidas": ["bebida", "jugo", "refresco", "soda", "agua", "cafe", "café", "té", "tea", "ice tea", "batido", "licuado", "cerveza", "granita", "frozen"],
    "Postres": ["postre", "pastel", "dulce", "helado", "cheesecake", "flan", "tres leches", "tiramisu", "pie"]
  };

  // Selección
  platoSel: any = null;
  mesaIdSeleccionada: number | null = null;
  cant: number = 1;
  nota: string = "";

  // Feedback
  msg: string = "";
  loading: boolean = false;
  intervalo: any;

  // Inyecciones
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private cd = inject(ChangeDetectorRef);
  private apiUrl = 'http://localhost:5143/api'; 

  ngOnInit() {
    this.fetchPlatos();
    this.fetchMesas();
    this.fetchOrdenesListas(); 
    
    // ⚡ Polling cada 10 segundos (más frecuente para notificaciones en tiempo real)
    this.intervalo = setInterval(() => {
      this.fetchOrdenesListas();
    }, 10000);
  }

  ngOnDestroy() {
    if (this.intervalo) clearInterval(this.intervalo);
  }

  // --- GETTERS ---
  get total() {
    return this.items.reduce((acc, it) => acc + (it.cantidad * it.precio), 0);
  }

  get platosFiltrados() {
    const q = this.buscar.trim().toLowerCase();
    return this.platos.filter(p => {
      const nombrePlato = (p.nombre || '').toLowerCase();
      const catPlato = (p.categoria || '').toLowerCase();
      const matchSearch = !q || nombrePlato.includes(q);

      let matchCat = false;
      if (this.cat === "Todas") {
        matchCat = true;
      } else {
        if (this.cat === "Almuerzo y Cena") {
            const esPostre = this.mapaCategorias["Postres"].some(k => nombrePlato.includes(k) || catPlato.includes(k));
            const esBebida = this.mapaCategorias["Bebidas"].some(k => nombrePlato.includes(k) || catPlato.includes(k));
            if (esPostre || esBebida) return false; 
        }
        const palabrasClave = this.mapaCategorias[this.cat] || [this.cat.toLowerCase()];
        matchCat = palabrasClave.some(clave => catPlato.includes(clave) || nombrePlato.includes(clave));
      }
      return matchCat && matchSearch;
    });
  }

  // --- HTTP HELPERS ---
  private getHeaders() {
    const token = this.authService.getToken();
    return { headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` }) };
  }

  // --- CARGA DE DATOS ---
  fetchPlatos() {
    this.http.get<any[]>(`${this.apiUrl}/Plato`, this.getHeaders()).subscribe({
      next: (data) => this.platos = data,
      error: (err) => console.error("Error platos:", err)
    });
  }

  fetchMesas() {
    this.http.get<any[]>(`${this.apiUrl}/Mesa`, this.getHeaders()).subscribe({
      next: (data) => this.mesas = data.filter(m => m.activa),
      error: (err) => console.error("Error mesas:", err)
    });
  }

  // ✅ ENTREGAS - Obtener órdenes listas
  fetchOrdenesListas() {
    this.http.get<any[]>(`${this.apiUrl}/Mesero/ordenes`, this.getHeaders()).subscribe({
      next: (data) => {
        console.log('📥 Órdenes recibidas del backend:', data);
        
        // ✅ El backend ya filtra por estado "LISTO", solo asignamos directamente
        this.ordenesListas = data;
        
        console.log(`✅ Total de órdenes listas: ${this.ordenesListas.length}`);
        this.cd.detectChanges(); 
      },
      error: (err) => {
        console.error("❌ Error buscando entregas:", err);
        this.ordenesListas = []; // En caso de error, limpiar la lista
      }
    });
  }

  // ✅ Marcar orden como entregada
  marcarEntregado(ordenId: number) {
    // ✅ Mensaje simplificado
    const confirmar = confirm(`Confirmar entrega de Orden #${ordenId}`);
    if (!confirmar) return;

    this.loading = true; // Mostrar loading

    this.http.post(`${this.apiUrl}/Mesero/entregar/${ordenId}`, {}, this.getHeaders()).subscribe({
      next: () => {
        console.log(`✅ Orden #${ordenId} marcada como entregada`);
        
        // ⚡ CRÍTICO: Remover la orden de la lista INMEDIATAMENTE (sin esperar al servidor)
        this.ordenesListas = this.ordenesListas.filter(o => o.id !== ordenId);
        
        // ✅ Actualizar desde el servidor para sincronizar
        this.fetchOrdenesListas();
        
        this.loading = false;
        this.msg = "✅ Entregada";
        setTimeout(() => this.msg = "", 2000);
      },
      error: (err) => {
        console.error('❌ Error al marcar entrega:', err);
        this.loading = false;
        this.msg = "❌ Error al entregar";
        
        // ⚡ Recargar lista en caso de error para sincronizar estado real
        this.fetchOrdenesListas();
        
        setTimeout(() => this.msg = "", 3000);
      }
    });
  }

  // --- CARRITO ---
  selectPlato(p: any) { this.platoSel = p; }
  seleccionarCategoria(c: string) { this.cat = c; }

  addItem() {
    if (!this.platoSel) return;
    const existe = this.items.find(it => it.platoId === this.platoSel.id && it.nota === this.nota);
    if (existe) existe.cantidad += this.cant;
    else this.items.push({ 
      platoId: this.platoSel.id, 
      nombre: this.platoSel.nombre, 
      precio: this.platoSel.precio, 
      cantidad: this.cant, 
      nota: this.nota 
    });
    this.cant = 1; this.nota = "";
  }

  removeItem(index: number) { this.items.splice(index, 1); }

  enviarOrden() {
    if (this.items.length === 0) return;
    if (!this.mesaIdSeleccionada) {
      this.msg = "⚠️ Selecciona una mesa primero";
      setTimeout(() => this.msg = "", 3000);
      return;
    }

    this.loading = true;
    const payload = { 
      mesaId: this.mesaIdSeleccionada,
      detalles: this.items.map(i => ({ platoId: i.platoId, cantidad: i.cantidad, nota: i.nota })) 
    };

    this.http.post(`${this.apiUrl}/Mesero/ordenes`, payload, this.getHeaders()).subscribe({
      next: () => {
        this.msg = "✅ Orden enviada a cocina";
        this.items = [];
        this.mesaIdSeleccionada = null;
        this.loading = false;
        setTimeout(() => this.msg = "", 3000);
      },
      error: () => {
        this.msg = "❌ Error al enviar orden";
        this.loading = false;
      }
    });
  }
}
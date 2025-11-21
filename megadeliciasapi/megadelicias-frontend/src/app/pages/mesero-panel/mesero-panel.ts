import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http'; // Importar HttpHeaders
import { AuthService } from '../../services/auth';
import { 
  LucideAngularModule, 
  List, Edit3, CreditCard, Send, PlusCircle, Search, Trash2, ShoppingCart 
} from 'lucide-angular'; // 1. Importar iconos individuales

@Component({
  selector: 'app-mesero-panel',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    // 2. Registrar iconos con .pick()
    LucideAngularModule 
  ],
  providers: [],
  templateUrl: './mesero-panel.html',
  styleUrl: './mesero-panel.css'
})
export class MeseroPanelComponent implements OnInit {
  
  // ... (Tus variables de estado siguen igual) ...
  platos: any[] = [];
  ordenes: any[] = [];
  items: any[] = [];
  buscar: string = "";
  cat: string = "Todas";
  categorias: string[] = [];
  platoSel: any = null;
  cant: number = 1;
  nota: string = "";
  msg: string = "";
  loading: boolean = false;
  editMode: boolean = false;
  metodoPagoCaja: string = "Efectivo";
  descCaja: string = "Venta por orden del mesero";
  metodosPago = ["Efectivo", "Tarjeta", "Transferencia", "Yape/QR", "Otro"];

  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = 'http://localhost:5143/api'; 

  // 3. IMPORTANTE: Iconos disponibles para el HTML (si usas <lucide-icon>)
  // Pero la forma moderna es registrarlos en el módulo. 
  // Para este fix rápido, vamos a importar el módulo con pick en el @Component arriba.
  // Espera, en Standalone es mejor importarlos en providers o usar el módulo configurado.
  // VAMOS A HACERLO EN EL CONSTRUCTOR PARA ASEGURARNOS.
  
  constructor() {
    // Hack para registrar iconos si .pick falla en tu versión
    // (Mejor solución: ver imports abajo)
  }

  get total() {
    return this.items.reduce((acc, it) => acc + (it.cantidad * it.precio), 0);
  }

  get platosFiltrados() {
    const q = this.buscar.trim().toLowerCase();
    return this.platos.filter(p => {
      const matchCat = this.cat === "Todas" || p.categoria === this.cat;
      const matchSearch = !q || p.nombre.toLowerCase().includes(q);
      return matchCat && matchSearch;
    });
  }

  ngOnInit() {
    this.fetchPlatos();
    this.fetchOrdenes();
  }

  // --- 4. HELPER PARA HEADERS (TOKEN) ---
  private getHeaders() {
    const token = this.authService.getToken();
    return {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    };
  }

  fetchPlatos() {
    // CORRECCIÓN DE RUTA: /api/Plato (Singular, como el controlador)
    this.http.get<any[]>(`${this.apiUrl}/Plato`, this.getHeaders()).subscribe({
      next: (data) => {
        this.platos = data;
        const cats = data.map(p => p.categoria).filter((c: any) => !!c);
        this.categorias = ["Todas", ...Array.from(new Set(cats)) as string[]];
      },
      error: (err) => console.error("Error platos:", err)
    });
  }

  fetchOrdenes() {
    this.http.get<any[]>(`${this.apiUrl}/mesero/ordenes`, this.getHeaders()).subscribe({
      next: (data) => this.ordenes = data,
      error: (err) => console.error("Error ordenes:", err)
    });
  }

  // ... (Resto de métodos: selectPlato, addItem, removeItem, updateQty igual que antes) ...
  selectPlato(p: any) { this.platoSel = p; }
  
  addItem() {
    if (!this.platoSel) return;
    const existe = this.items.find(it => it.platoId === this.platoSel.id && it.nota === this.nota);
    if (existe) existe.cantidad += this.cant;
    else this.items.push({ platoId: this.platoSel.id, nombre: this.platoSel.nombre, precio: this.platoSel.precio, cantidad: this.cant, nota: this.nota });
    this.cant = 1; this.nota = "";
  }

  removeItem(index: number) { this.items.splice(index, 1); }

  enviarOrden() {
    if (this.items.length === 0) return;
    this.loading = true;
    const payload = { detalles: this.items.map(i => ({ platoId: i.platoId, cantidad: i.cantidad, nota: i.nota })) };

    // AÑADIR HEADERS AQUÍ
    this.http.post(`${this.apiUrl}/mesero/ordenes`, payload, this.getHeaders()).subscribe({
      next: () => {
        this.msg = "✅ Orden enviada a cocina";
        this.items = [];
        this.fetchOrdenes();
        this.loading = false;
        setTimeout(() => this.msg = "", 3000);
      },
      error: () => {
        this.msg = "❌ Error al enviar orden";
        this.loading = false;
      }
    });
  }

  enviarACaja() {
    // Misma lógica, añadir this.getHeaders()
    // ... (Implementación pendiente si la usas)
  }
}
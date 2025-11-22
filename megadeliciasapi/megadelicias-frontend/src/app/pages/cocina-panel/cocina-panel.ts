import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { LucideAngularModule } from 'lucide-angular';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-cocina-panel',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './cocina-panel.html',
  styleUrl: './cocina-panel.css'
})
export class CocinaPanelComponent implements OnInit, OnDestroy {
  
  ordenes: any[] = [];
  loading: boolean = false;
  filtroActual: string = 'Todos'; 

  private intervalId: any;
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = 'http://localhost:5143/api/cocina'; 

  private getHeaders() {
    const token = this.authService.getToken();
    return {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    };
  }

  ngOnInit() {
    this.fetchOrdenes();
    // Polling cada 5 segundos
    this.intervalId = setInterval(() => this.fetchOrdenes(), 5000);
  }

  ngOnDestroy() {
    if (this.intervalId) clearInterval(this.intervalId);
  }

  fetchOrdenes() {
    this.http.get<any[]>(this.apiUrl, this.getHeaders()).subscribe({
      next: (data) => {
        // OPTIMIZACIÓN: Solo si los datos fueron cambiados
        // Esto evita que Angular redibuje la pantalla innecesariamente
        if (JSON.stringify(data) !== JSON.stringify(this.ordenes)) {
          this.ordenes = data;
        }
      },
      error: (err) => console.error("Error conectando a cocina:", err)
    });
  }

  // OPTIMIZACIÓN VISUAL: Función vital para evitar parpadeo en html
  trackByOrden(index: number, item: any): number {
    return item.id; // Angular usará el ID para identificar la tarjeta en el DOM
  }

  get ordenesFiltradas() {
    if (this.filtroActual === 'Todos') {
      return this.ordenes;
    }
    return this.ordenes.filter(o => o.estado === this.filtroActual);
  }

  contarOrdenes(estado: string): number {
    if (estado === 'Todos') return this.ordenes.length;
    return this.ordenes.filter(o => o.estado === estado).length;
  }

  setFiltro(filtro: string) {
    this.filtroActual = filtro;
  }

  cambiarEstado(ordenId: number, nuevoEstado: string) {
    this.loading = true;
    const url = `${this.apiUrl}/${ordenId}/estado`;
    
    this.http.put(url, { estado: nuevoEstado }, this.getHeaders()).subscribe({
      next: () => {
        // Forzamos una actualización inmediata
        this.fetchOrdenes(); 
        this.loading = false;
      },
      error: () => {
        alert("Error al actualizar la orden");
        this.loading = false;
      }
    });
  }

  getColorEstado(estado: string) {
    switch(estado) {
      case 'PENDIENTE': return 'border-l-4 border-l-yellow-500 bg-yellow-50/50';
      case 'EN_PROCESO': return 'border-l-4 border-l-blue-500 bg-blue-50/50';
      case 'LISTO': return 'border-l-4 border-l-green-500 bg-green-50/50';
      case 'ENTREGADO': return 'border-l-4 border-l-gray-400 bg-white opacity-60 grayscale';
      case 'CANCELADO': return 'border-l-4 border-l-red-500 bg-red-50 opacity-60';
      default: return 'bg-white';
    }
  }
  
  getBadgeColor(estado: string) {
      switch(estado) {
      case 'PENDIENTE': return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'EN_PROCESO': return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'LISTO': return 'bg-green-100 text-green-800 border-green-200';
      case 'ENTREGADO': return 'bg-gray-100 text-gray-600 border-gray-200';
      case 'CANCELADO': return 'bg-red-100 text-red-800 border-red-200';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}
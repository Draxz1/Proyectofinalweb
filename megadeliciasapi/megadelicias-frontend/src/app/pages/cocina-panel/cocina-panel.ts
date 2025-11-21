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
  private intervalId: any;
  
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  
  // Ajusta el puerto si es necesario (5143 o 7110)
  private apiUrl = 'http://localhost:5143/api/cocina'; 

  // Helper para enviar el token en cada petición
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
    // Auto-refrescar cada 10 segundos (Polling) para ver nuevos pedidos
    this.intervalId = setInterval(() => this.fetchOrdenes(), 10000);
  }

  ngOnDestroy() {
    if (this.intervalId) clearInterval(this.intervalId);
  }

  fetchOrdenes() {
    // No activamos 'loading' visual en cada refresco automático para no molestar
    this.http.get<any[]>(this.apiUrl, this.getHeaders()).subscribe({
      next: (data) => {
        this.ordenes = data;
      },
      error: (err) => console.error("Error conectando a cocina:", err)
    });
  }

  cambiarEstado(ordenId: number, nuevoEstado: string) {
    this.loading = true;
    const url = `${this.apiUrl}/${ordenId}/estado`;
    
    this.http.put(url, { estado: nuevoEstado }, this.getHeaders()).subscribe({
      next: () => {
        this.fetchOrdenes(); // Recargar inmediatamente
        this.loading = false;
      },
      error: () => {
        alert("Error al actualizar la orden");
        this.loading = false;
      }
    });
  }

  // Colores dinámicos según el estado (Estilo Semáforo)
  getColorEstado(estado: string) {
    switch(estado) {
      case 'PENDIENTE': return 'border-l-4 border-l-yellow-500 bg-yellow-50/30';
      case 'EN_PROCESO': return 'border-l-4 border-l-blue-500 bg-blue-50/30';
      case 'LISTO': return 'border-l-4 border-l-green-500 bg-green-50/30 opacity-75';
      default: return 'bg-white';
    }
  }
  
  getBadgeColor(estado: string) {
      switch(estado) {
      case 'PENDIENTE': return 'bg-yellow-100 text-yellow-800';
      case 'EN_PROCESO': return 'bg-blue-100 text-blue-800';
      case 'LISTO': return 'bg-green-100 text-green-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}
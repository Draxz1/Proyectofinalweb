import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InventarioService {
  // CONFIRMADO: Usamos el puerto 5143 que vimos en tu prueba exitosa
  private apiUrl = 'http://localhost:5143/api/Inventario'; 

  constructor(private http: HttpClient) { }

  getInventario(busqueda: string = ''): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}?busqueda=${busqueda}`);
  }

  registrarMovimiento(movimiento: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/movimiento`, movimiento);
  }

  getMovimientosRecientes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/movimientos`);
  }

  getCategorias(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/categorias`);
  }

  crearItem(item: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/item`, item);
  }
}
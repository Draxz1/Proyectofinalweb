import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; // <-- Importa esto

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule], // <-- Añade esto
  // ¡AQUÍ ESTÁ LA CORRECCIÓN!
  // Quita ".component" de los nombres de archivo
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent { // <-- El nombre de la CLASE SÍ lleva "Component"

}
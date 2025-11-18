import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; // <-- Importar CommonModule

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule], // <-- Añadir CommonModule
  // CORRECCIÓN: Los nombres de archivo no llevan .component
  templateUrl: './admin-users.html', 
  styleUrl: './admin-users.css'
})
export class AdminUsersComponent { // <-- CLAVE: La clase debe ser EXPORTADA

}
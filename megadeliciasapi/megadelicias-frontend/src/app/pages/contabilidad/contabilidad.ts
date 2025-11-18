import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router'; // <-- Lo necesitamos para el menú

@Component({
  selector: 'app-contabilidad',
  standalone: true,
  imports: [CommonModule, RouterLink], // <-- Agregamos RouterLink
  // CORRECCIÓN: Quitamos ".component"
  templateUrl: './contabilidad.html', 
  styleUrl: './contabilidad.css'
})
export class ContabilidadComponent { // <-- CLAVE: La clase debe ser EXPORTADA

}
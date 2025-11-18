import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; // <-- Importa CommonModule

@Component({
  selector: 'app-cocina-panel',
  standalone: true,
  imports: [CommonModule], // <-- Añade CommonModule
  templateUrl: './cocina-panel.html', // <-- Asegúrate que sea .html
  styleUrl: './cocina-panel.css'      // <-- Asegúrate que sea .css
})
export class CocinaPanelComponent {
  // ¡Ahora la clase "CocinaPanelComponent" existe y se exporta!
}
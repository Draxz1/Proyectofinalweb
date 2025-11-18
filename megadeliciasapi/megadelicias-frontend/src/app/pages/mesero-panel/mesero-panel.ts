import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; // Importa CommonModule

@Component({
  selector: 'app-mesero-panel',
  standalone: true,
  imports: [CommonModule], // Añade CommonModule
  templateUrl: './mesero-panel.html', // Asegúrate que el nombre coincida
  styleUrl: './mesero-panel.css' // Asegúrate que el nombre coincida
})
export class MeseroPanelComponent {
  // ¡Ahora la clase "MeseroPanelComponent" existe y se exporta!
}
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-inventario-panel',
  standalone: true,
  imports: [CommonModule],
  // CORRECCIÓN: Quitamos ".component"
  templateUrl: './inventario-panel.html', 
  styleUrl: './inventario-panel.css'
})
export class InventarioPanelComponent { // <-- FIX: La clase debe ser EXPORTADA

}
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-caja-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './caja-panel.html', // <-- Corregido
  styleUrl: './caja-panel.css'      // <-- Corregido
})
export class CajaPanelComponent {

}
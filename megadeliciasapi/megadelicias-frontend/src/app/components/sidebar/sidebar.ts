import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router'; // <-- ¡Importa esto!

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    RouterLink,         // <-- ¡Añádelo aquí!
    RouterLinkActive    // <-- ¡Añádelo aquí!
  ],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class SidebarComponent {

}

import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router'; // <-- ¡Importa RouterOutlet!
import { SidebarComponent } from '../sidebar/sidebar'; // <-- ¡Importa el Sidebar!

@Component({
  selector: 'app-app-layout',
  standalone: true,
  imports: [
    RouterOutlet,       // <-- ¡Añádelo aquí!
    SidebarComponent    // <-- ¡Añádelo aquí!
  ],
  templateUrl: './app-layout.html',
  styleUrl: './app-layout.css'
})
export class AppLayoutComponent {

}
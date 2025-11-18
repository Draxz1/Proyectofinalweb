import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router'; // <-- ¡Importa RouterOutlet!
import { SidebarComponent } from '../sidebar/sidebar.component'; // <-- ¡Importa el Sidebar!

@Component({
  selector: 'app-app-layout',
  standalone: true,
  imports: [
    RouterOutlet,       // <-- ¡Añádelo aquí!
    SidebarComponent    // <-- ¡Añádelo aquí!
  ],
  templateUrl: './app-layout.component.html',
  styleUrl: './app-layout.component.css'
})
export class AppLayoutComponent {

}

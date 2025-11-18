import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router'; // <-- ¡Importa RouterOutlet!
import { SidebarComponent } from '../sidebar/sidebar'; // <-- ¡Importa el Sidebar!
import { TopbarComponent } from '../topbar/topbar';

@Component({
  selector: 'app-app-layout',
  standalone: true,
  imports: [
    RouterOutlet,       // <-- ¡Añádelo aquí!
    SidebarComponent,
    TopbarComponent    // <-- ¡Añádelo aquí!
  ],
  templateUrl: './app-layout.html',
  styleUrl: './app-layout.css'
})
export class AppLayoutComponent {

}
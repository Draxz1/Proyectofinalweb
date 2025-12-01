import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core'; // <--- 1. Importar ChangeDetectorRef
import { CommonModule } from '@angular/common'; 
import { RouterLink, RouterLinkActive } from '@angular/router'; 
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive], 
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class SidebarComponent implements OnInit {

  private authService = inject(AuthService);
  private cd = inject(ChangeDetectorRef); // <--- 2. Inyectar el detector de cambios
  
  rol: string = '';

  ngOnInit() {
    // Nos suscribimos a los cambios del usuario
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.rol = user.rol?.toLowerCase() || '';
        console.log("✅ Sidebar recibió rol:", this.rol);
      } else {
        this.rol = '';
      }
      
      // 3. ¡EL TRUCO! Forzamos a Angular a repintar la vista INMEDIATAMENTE
      this.cd.detectChanges(); 
    });
  }

  // Getters para el HTML
  get isAdmin() { return this.rol === 'admin'; }
  get isMesero() { return this.rol === 'mesero'; }
  get isCocina() { return this.rol === 'cocinero' || this.rol === 'cocina'; }
  get isCaja() { return this.rol === 'cajero' || this.rol === 'caja'; }

  logout() {
    this.authService.logout();
  }
}
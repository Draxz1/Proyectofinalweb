import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, TitleCasePipe } from '@angular/common';
import { Router } from '@angular/router';

// Importaciones de Lucide y el Servicio
import { LucideAngularModule } from 'lucide-angular';
import { AuthService } from '../../services/auth'; 

interface DashboardOption {
  label: string;
  icon: string; // Nombre del icono en Lucide
  path: string;
  show: boolean; 
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    TitleCasePipe,
    LucideAngularModule 
  ],
  templateUrl: './dashboard.html', 
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {

  userName: string = 'Usuario';
  userRole: string = 'visitante';

  private authService = inject(AuthService);
  private router = inject(Router);

  options: DashboardOption[] = []; // Esto contendrá las tarjetas

  ngOnInit() {
    // Lógica para obtener el nombre y rol del usuario logueado
    const user = this.authService.getUserData();
    if (user) {
      this.userName = user.nombre;
      this.userRole = user.rol.toLowerCase();
      this.options = this.getOptions(this.userRole);
    } else {
      this.authService.logout();
    }
  }

  // Lógica migrada del antiguo Dashboard.jsx para mostrar tarjetas por rol
  getOptions(role: string): DashboardOption[] {
    const can = (roles: string[]) => role === 'admin' || roles.includes(role);

    // Mapeo de opciones (usamos nombres de Lucide como string)
    const items: DashboardOption[] = [
      { label: 'Administrador', icon: 'users', path: '/admin', show: can(['admin']) },
      { label: 'Caja POS', icon: 'calculator', path: '/caja', show: can(['admin', 'cajero', 'mesero']) },
      { label: 'Mesero / Pedidos', icon: 'utensils', path: '/mesero', show: can(['mesero']) },
      { label: 'Inventario', icon: 'package', path: '/inventario', show: can(['admin']) },
      { label: 'Cocina', icon: 'chef-hat', path: '/cocina', show: can(['admin', 'cocinero']) },
      { label: 'Contabilidad', icon: 'clipboard', path: '/contabilidad', show: can(['admin', 'contable']) },
    ];

    return items.filter(i => i.show);
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }
}
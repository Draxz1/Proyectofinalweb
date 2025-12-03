import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, TitleCasePipe } from '@angular/common';
import { Router } from '@angular/router';

// 1. Importamos LucideAngularModule y LOS ICONOS ESPECÍFICOS que vamos a usar
import { 
  LucideAngularModule, 
  Users, 
  Calculator, 
  Utensils, 
  Package, 
  ChefHat, 
  Clipboard 
} from 'lucide-angular';

import { AuthService } from '../../services/auth'; 

interface DashboardOption {
  label: string;
  icon: string; 
  path: string;
  show: boolean; 
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    TitleCasePipe,
    // 2. Usamos .pick() para registrar los iconos y que estén disponibles en el HTML
    LucideAngularModule
  ],
  templateUrl: './dashboard.html', 
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  
  // Registramos los iconos para que el HTML pueda usarlos
  readonly icons = { Users, Calculator, Utensils, Package, ChefHat, Clipboard };

  userName: string = 'Usuario';
  userRole: string = 'visitante';

  private authService = inject(AuthService);
  private router = inject(Router);

  options: DashboardOption[] = []; 

  constructor() {
    // Inicializamos los iconos (necesario en algunas versiones de lucide)
    // En versiones modernas, basta con importar el módulo con .pick en 'imports'
    // O hacer: imports: [LucideAngularModule.pick({ Users, ... })]
  }

  ngOnInit() {
    const user = this.authService.getUserData();
    if (user) {
      this.userName = user.nombre;
      this.userRole = user.rol.toLowerCase();
      this.options = this.getOptions(this.userRole);
    } else {
      this.authService.logout();
    }
  }

  getOptions(role: string): DashboardOption[] {
    const can = (roles: string[]) => role === 'admin' || roles.includes(role);

    // 3. RUTAS CORREGIDAS SEGÚN TU app.routes.ts
    const items: DashboardOption[] = [
      { label: 'Administrador', icon: 'users', path: '/admin', show: can(['admin']) },
      // Corregido: /caja -> /caja-panel
      { label: 'Caja POS', icon: 'calculator', path: '/caja-panel', show: can(['admin', 'cajero', 'mesero']) },
      // Corregido: /mesero -> /mesero-panel
      { label: 'Mesero / Pedidos', icon: 'utensils', path: '/mesero-panel', show: can(['mesero']) },
      { label: 'Inventario', icon: 'package', path: '/inventario', show: can(['admin']) },
      // Corregido: /cocina -> /cocina-panel
      { label: 'Cocina', icon: 'chef-hat', path: '/cocina-panel', show: can(['admin', 'cocinero']) },
      { label: 'Contabilidad', icon: 'clipboard', path: '/contabilidad', show: can(['admin', 'contable']) },
    ];

    return items.filter(i => i.show);
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }
}
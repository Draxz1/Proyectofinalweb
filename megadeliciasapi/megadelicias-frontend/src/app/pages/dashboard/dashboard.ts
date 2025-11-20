import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, TitleCasePipe } from '@angular/common';
import { Router } from '@angular/router';


import { LucideAngularModule } from 'lucide-angular';
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

  options: DashboardOption[] = []; 

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
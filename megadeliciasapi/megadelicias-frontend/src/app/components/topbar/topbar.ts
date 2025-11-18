import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule, TitleCasePipe } from '@angular/common';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';

// Importación de Lucide (Íconos)
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './topbar.html',
  styleUrl: './topbar.css'
})
export class TopbarComponent implements OnInit {

  // La página que envíe este componente puede darle un título
  @Input() title: string = 'Panel';

  userName: string = 'Usuario';
  userRole: string = 'Mesero';

  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit() {
    const user = this.authService.getUserData();
    if (user) {
      this.userName = user.nombre;
      this.userRole = user.rol;
    }
  }

  logout() {
    this.authService.logout();
  }
}

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router'; 

@Component({
  selector: 'app-contabilidad',
  standalone: true,
  imports: [CommonModule, RouterLink], 
  
  templateUrl: './contabilidad.html', 
  styleUrl: './contabilidad.css'
})
export class ContabilidadComponent { 

}
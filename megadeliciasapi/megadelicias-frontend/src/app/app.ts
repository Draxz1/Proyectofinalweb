import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html', 
  styleUrl: './app.css'
})
export class AppComponent { // <-- FIX: Se llama AppComponent y está exportado
  // La clase App ahora se llama AppComponent (convención estándar)
}
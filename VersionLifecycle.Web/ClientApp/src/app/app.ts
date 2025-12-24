import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthStore } from './core/stores/auth.store';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  title = 'Version Lifecycle';
  authStore = inject(AuthStore);
  showNavigation = true;

  logout(): void {
    this.authStore.logout();
  }
}

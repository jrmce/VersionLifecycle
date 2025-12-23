import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
import { AuthStore } from './core/stores/auth.store';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App implements OnInit {
  title = 'Version Lifecycle';
  private router = inject(Router);
  authStore = inject(AuthStore);
  showNavigation = false;

  ngOnInit(): void {
    // Always show navigation now that we have public pages like how-to-use
    this.showNavigation = true;
  }

  logout(): void {
    this.authStore.logout();
  }
}

import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
import { AuthStore } from './core/stores/auth.store';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './app.html'
})
export class App implements OnInit {
  title = 'Version Lifecycle';
  private router = inject(Router);
  authStore = inject(AuthStore);
  showNavigation = false;

  ngOnInit(): void {
    // Show navigation only on pages that aren't login/register
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        const url = event.urlAfterRedirects;
        this.showNavigation = !url.includes('/login') && !url.includes('/register');
      });
  }

  logout(): void {
    this.authStore.logout();
  }
}

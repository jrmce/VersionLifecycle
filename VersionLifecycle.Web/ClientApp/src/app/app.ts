import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthStore } from './core/stores/auth.store';
import { UserRole } from './core/enums';

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
  mobileMenuOpen = false;

  private readonly navLinks: Array<{ label: string; route: string; requiresAuth?: boolean; roles?: UserRole[] }> = [
    { label: 'How to Use', route: '/how-to-use' },
    { label: 'Dashboard', route: '/dashboard', requiresAuth: true },
    { label: 'Applications', route: '/applications', requiresAuth: true },
    { label: 'Deployments', route: '/deployments', requiresAuth: true },
    { label: 'Environments', route: '/environments', requiresAuth: true },
    { label: 'API Tokens', route: '/api-tokens', requiresAuth: true, roles: [UserRole.Admin, UserRole.SuperAdmin] },
    { label: 'Users', route: '/users', requiresAuth: true, roles: [UserRole.Admin, UserRole.SuperAdmin] },
    { label: 'Tenants', route: '/admin/tenants', requiresAuth: true, roles: [UserRole.SuperAdmin] },
  ];

  get visibleLinks() {
    const isAuthenticated = this.authStore.isAuthenticated();
    const role = this.authStore.user()?.role;

    return this.navLinks.filter((link) => {
      if (link.requiresAuth && !isAuthenticated) return false;
      if (link.roles && (!role || !link.roles.includes(role))) return false;
      if (!link.requiresAuth && isAuthenticated && link.route === '/how-to-use') return false;
      return true;
    });
  }

  logout(): void {
    this.authStore.logout();
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
  }
}

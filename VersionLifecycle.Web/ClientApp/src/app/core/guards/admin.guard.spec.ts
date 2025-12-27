import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { adminGuard } from './admin.guard';
import { AuthStore } from '../stores/auth.store';
import { AuthService } from '../services/auth.service';

describe('adminGuard', () => {
  let authStore: AuthStore;
  let navigateSpy: jasmine.Spy;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthStore,
        { provide: AuthService, useValue: { login: () => of(null), register: () => of(null) } },
        { provide: Router, useValue: { navigate: jasmine.createSpy('navigate') } },
      ],
    });

    authStore = TestBed.inject(AuthStore);
    navigateSpy = TestBed.inject(Router).navigate as jasmine.Spy;
  });

  it('allows SuperAdmin users to proceed', () => {
    spyOn(authStore, 'user').and.returnValue({ userId: '1', email: 'super@admin.com', role: 'SuperAdmin' });

    const result = TestBed.runInInjectionContext(() => adminGuard());

    expect(result).toBeTrue();
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('redirects non-superadmin users to dashboard', () => {
    spyOn(authStore, 'user').and.returnValue({ userId: '2', email: 'admin@example.com', role: 'Admin' });

    const result = TestBed.runInInjectionContext(() => adminGuard());

    expect(result).toBeFalse();
    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  });
});

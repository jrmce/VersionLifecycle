import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { LoginDto, RegisterDto, LoginResponseDto } from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private currentTenantSubject = new BehaviorSubject<string | null>(null);
  public currentTenant$ = this.currentTenantSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadFromStorage();
  }

  login(credentials: LoginDto): Observable<LoginResponseDto> {
    return this.http.post<LoginResponseDto>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => this.handleAuthSuccess(response, credentials.tenantId))
    );
  }

  register(data: RegisterDto): Observable<LoginResponseDto> {
    return this.http.post<LoginResponseDto>(`${this.apiUrl}/register`, data).pipe(
      tap(response => this.handleAuthSuccess(response, data.tenantId))
    );
  }

  refresh(): Observable<LoginResponseDto> {
    const refreshToken = this.getRefreshToken();
    return this.http.post<LoginResponseDto>(`${this.apiUrl}/refresh`, { refreshToken }).pipe(
      tap(response => {
        const tenantId = this.currentTenantSubject.value || '';
        this.handleAuthSuccess(response, tenantId);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('currentTenant');
    this.currentUserSubject.next(null);
    this.currentTenantSubject.next(null);
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  getCurrentTenant(): string | null {
    return this.currentTenantSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  private handleAuthSuccess(response: LoginResponseDto, tenantId: string): void {
    localStorage.setItem('accessToken', response.accessToken);
    localStorage.setItem('refreshToken', response.refreshToken);
    localStorage.setItem('currentTenant', tenantId);
    this.currentTenantSubject.next(tenantId);
    // Decode token to get user info
    const payload = this.parseJwt(response.accessToken);
    this.currentUserSubject.next(payload);
  }

  private loadFromStorage(): void {
    const token = this.getAccessToken();
    const tenant = localStorage.getItem('currentTenant');
    if (token && tenant) {
      const payload = this.parseJwt(token);
      this.currentUserSubject.next(payload);
      this.currentTenantSubject.next(tenant);
    }
  }

  private parseJwt(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      console.error('Error parsing JWT', error);
      return null;
    }
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from './api.config';

export interface UserDto {
  id: string;
  email: string;
  role: string;
  tenantId: string;
  createdAt: string;
}

export interface UpdateUserRoleDto {
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getTenantUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.apiUrl);
  }

  updateUserRole(userId: string, role: string): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.apiUrl}/${userId}/role`, { role });
  }

  deleteUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${userId}`);
  }
}

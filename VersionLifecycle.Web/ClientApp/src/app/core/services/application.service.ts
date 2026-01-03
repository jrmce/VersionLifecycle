import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  ApplicationDto, 
  CreateApplicationDto, 
  UpdateApplicationDto, 
  PaginatedResponse 
} from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class ApplicationService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/applications`;

  constructor(private http: HttpClient) {}

  getApplications(skip: number = 0, take: number = 25): Observable<PaginatedResponse<ApplicationDto>> {
    return this.http.get<PaginatedResponse<ApplicationDto>>(`${this.apiUrl}?skip=${skip}&take=${take}`);
  }

  getApplication(id: string): Observable<ApplicationDto> {
    return this.http.get<ApplicationDto>(`${this.apiUrl}/${id}`);
  }

  createApplication(dto: CreateApplicationDto): Observable<ApplicationDto> {
    return this.http.post<ApplicationDto>(this.apiUrl, dto);
  }

  updateApplication(id: string, dto: UpdateApplicationDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, dto);
  }

  deleteApplication(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}

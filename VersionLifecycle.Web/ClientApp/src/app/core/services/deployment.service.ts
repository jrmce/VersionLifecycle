import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  DeploymentDto, 
  CreatePendingDeploymentDto, 
  DeploymentEventDto,
  PaginatedResponse,
  DeploymentStatus 
} from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class DeploymentService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/deployments`;

  constructor(private http: HttpClient) {}

  getDeployments(
    skip: number = 0,
    take: number = 25,
    status?: DeploymentStatus
  ): Observable<PaginatedResponse<DeploymentDto>> {
    let url = `${this.apiUrl}?skip=${skip}&take=${take}`;
    if (status) {
      url += `&status=${status}`;
    }
    return this.http.get<PaginatedResponse<DeploymentDto>>(url);
  }

  getDeployment(id: number): Observable<DeploymentDto> {
    return this.http.get<DeploymentDto>(`${this.apiUrl}/${id}`);
  }

  createPendingDeployment(dto: CreatePendingDeploymentDto): Observable<DeploymentDto> {
    return this.http.post<DeploymentDto>(this.apiUrl, dto);
  }

  confirmDeployment(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/confirm`, {});
  }

  getDeploymentEvents(id: number): Observable<DeploymentEventDto[]> {
    return this.http.get<DeploymentEventDto[]>(`${this.apiUrl}/${id}/events`);
  }
}

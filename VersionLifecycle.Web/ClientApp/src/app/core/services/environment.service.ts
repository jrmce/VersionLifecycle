import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EnvironmentDto, CreateEnvironmentDto, UpdateEnvironmentDto } from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class EnvironmentService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/environments`;

  constructor(private http: HttpClient) {}

  getEnvironments(applicationId: number): Observable<EnvironmentDto[]> {
    return this.http.get<EnvironmentDto[]>(
      `${this.apiUrl}/${applicationId}/environments`
    );
  }

  getEnvironment(applicationId: number, environmentId: number): Observable<EnvironmentDto> {
    return this.http.get<EnvironmentDto>(
      `${this.apiUrl}/${applicationId}/environments/${environmentId}`
    );
  }

  createEnvironment(applicationId: number, dto: CreateEnvironmentDto): Observable<EnvironmentDto> {
    return this.http.post<EnvironmentDto>(
      `${this.apiUrl}/${applicationId}/environments`,
      dto
    );
  }

  updateEnvironment(
    applicationId: number,
    environmentId: number,
    dto: UpdateEnvironmentDto
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/${applicationId}/environments/${environmentId}`,
      dto
    );
  }

  deleteEnvironment(applicationId: number, environmentId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${applicationId}/environments/${environmentId}`
    );
  }
}

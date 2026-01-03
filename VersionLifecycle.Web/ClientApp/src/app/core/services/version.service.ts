import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { VersionDto, CreateVersionDto, UpdateVersionDto } from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class VersionService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/applications`;

  constructor(private http: HttpClient) {}

  getVersions(applicationId: string): Observable<VersionDto[]> {
    return this.http.get<VersionDto[]>(`${this.apiUrl}/${applicationId}/versions`);
  }

  getVersion(applicationId: string, versionId: string): Observable<VersionDto> {
    return this.http.get<VersionDto>(`${this.apiUrl}/${applicationId}/versions/${versionId}`);
  }

  createVersion(applicationId: string, dto: CreateVersionDto): Observable<VersionDto> {
    return this.http.post<VersionDto>(`${this.apiUrl}/${applicationId}/versions`, dto);
  }

  updateVersion(applicationId: string, versionId: string, dto: UpdateVersionDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${applicationId}/versions/${versionId}`, dto);
  }

  deleteVersion(applicationId: string, versionId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${applicationId}/versions/${versionId}`);
  }
}

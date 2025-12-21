import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { VersionDto, CreateVersionDto, UpdateVersionDto } from '../models/models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class VersionService {
  private readonly apiUrl = `${environment.apiUrl}/applications`;

  constructor(private http: HttpClient) {}

  getVersions(applicationId: number): Observable<VersionDto[]> {
    return this.http.get<VersionDto[]>(
      `${this.apiUrl}/${applicationId}/versions`
    );
  }

  getVersion(applicationId: number, versionId: number): Observable<VersionDto> {
    return this.http.get<VersionDto>(
      `${this.apiUrl}/${applicationId}/versions/${versionId}`
    );
  }

  createVersion(applicationId: number, dto: CreateVersionDto): Observable<VersionDto> {
    return this.http.post<VersionDto>(
      `${this.apiUrl}/${applicationId}/versions`,
      dto
    );
  }

  updateVersion(
    applicationId: number,
    versionId: number,
    dto: UpdateVersionDto
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/${applicationId}/versions/${versionId}`,
      dto
    );
  }

  deleteVersion(applicationId: number, versionId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${applicationId}/versions/${versionId}`
    );
  }
}

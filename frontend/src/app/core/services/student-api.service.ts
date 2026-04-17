import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL, API_ENDPOINTS } from './api.config';
import { ProposalDetailsDto, ProposalListItemDto, ResearchAreaDto, StudentDashboardDto } from '../models/api.models';

export interface ProposalUpsertRequest {
  title: string;
  abstract: string;
  technicalStack?: string;
  researchAreaId: number;
}

@Injectable({ providedIn: 'root' })
export class StudentApiService {
  private readonly http = inject(HttpClient);

  getDashboard(): Observable<StudentDashboardDto> {
    return this.http.get<StudentDashboardDto>(`${API_BASE_URL}${API_ENDPOINTS.student.dashboard}`);
  }

  getProposals(): Observable<ProposalListItemDto[]> {
    return this.http.get<ProposalListItemDto[]>(`${API_BASE_URL}${API_ENDPOINTS.student.proposals}`);
  }

  getProposal(id: number): Observable<ProposalDetailsDto> {
    return this.http.get<ProposalDetailsDto>(`${API_BASE_URL}${API_ENDPOINTS.student.proposals}/${id}`);
  }

  createProposal(payload: ProposalUpsertRequest): Observable<unknown> {
    return this.http.post(`${API_BASE_URL}${API_ENDPOINTS.student.proposals}`, payload);
  }

  updateProposal(id: number, payload: ProposalUpsertRequest): Observable<unknown> {
    return this.http.put(`${API_BASE_URL}${API_ENDPOINTS.student.proposals}/${id}`, payload);
  }

  withdrawProposal(id: number): Observable<unknown> {
    return this.http.post(`${API_BASE_URL}${API_ENDPOINTS.student.proposals}/${id}/withdraw`, {});
  }

  getResearchAreas(): Observable<ResearchAreaDto[]> {
    return this.http.get<ResearchAreaDto[]>(`${API_BASE_URL}${API_ENDPOINTS.student.researchAreas}`);
  }
}

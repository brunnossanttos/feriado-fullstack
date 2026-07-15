import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../http/api.tokens';
import {
  CreateFeriadoRequest,
  Feriado,
  FeriadoQuery,
  UpdateFeriadoRequest,
} from '../models/feriado.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class FeriadoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(API_BASE_URL)}/feriados`;

  list(query: FeriadoQuery = {}): Observable<PagedResult<Feriado>> {
    let params = new HttpParams();

    if (query.page != null) {
      params = params.set('page', query.page);
    }
    if (query.pageSize != null) {
      params = params.set('pageSize', query.pageSize);
    }
    if (query.title?.trim()) {
      params = params.set('title', query.title.trim());
    }
    if (query.date?.trim()) {
      params = params.set('date', query.date.trim());
    }

    return this.http.get<PagedResult<Feriado>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Feriado> {
    return this.http.get<Feriado>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateFeriadoRequest): Observable<Feriado> {
    return this.http.post<Feriado>(this.baseUrl, request);
  }

  update(id: string, request: UpdateFeriadoRequest): Observable<Feriado> {
    return this.http.put<Feriado>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

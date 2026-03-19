import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CryptoPrice, PagedResponse, StatsResponse } from '../models/crypto-price.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CryptoService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/crypto`;

  getLatest(page = 1, pageSize = 50): Observable<PagedResponse> {
    return this.http.get<PagedResponse>(`${this.base}?page=${page}&pageSize=${pageSize}`);
  }

  getTopGainers(limit = 10): Observable<CryptoPrice[]> {
    return this.http.get<CryptoPrice[]>(`${this.base}/top-gainers?limit=${limit}`);
  }

  getTopLosers(limit = 10): Observable<CryptoPrice[]> {
    return this.http.get<CryptoPrice[]>(`${this.base}/top-losers?limit=${limit}`);
  }

  getHistory(symbol: string): Observable<CryptoPrice[]> {
    return this.http.get<CryptoPrice[]>(`${this.base}/history/${symbol}`);
  }

  getStats(): Observable<StatsResponse> {
    return this.http.get<StatsResponse>(`${this.base}/stats`);
  }

  triggerScrape(): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/trigger`, {});
  }
}

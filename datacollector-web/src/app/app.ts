import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, registerables } from 'chart.js';
import { forkJoin, interval, merge, Subscription, startWith, switchMap, Subject } from 'rxjs';

import { CryptoService } from './services/crypto.service';
import { CryptoPrice, StatsResponse } from './models/crypto-price.model';
import { StatCardComponent } from './components/stat-card/stat-card.component';
import { CoinTableComponent } from './components/coin-table/coin-table.component';
import { MoversListComponent } from './components/movers-list/movers-list.component';
import { PriceChartComponent } from './components/price-chart/price-chart.component';

Chart.register(...registerables);

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    StatCardComponent,
    CoinTableComponent,
    MoversListComponent,
    PriceChartComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit, OnDestroy {
  private readonly service = inject(CryptoService);
  private sub?: Subscription;

  coins: CryptoPrice[] = [];
  gainers: CryptoPrice[] = [];
  losers: CryptoPrice[] = [];
  stats: StatsResponse | null = null;
  selectedCoin: CryptoPrice | null = null;
  selectedHistory: CryptoPrice[] = [];
  loading = true;
  triggering = false;
  lastUpdated = '';
  private readonly refresh$ = new Subject<void>();

  triggerScrape(): void {
    this.triggering = true;
    this.service.triggerScrape().subscribe({
      next: () => {
        // Aguarda 3s para o worker processar e então recarrega os dados
        setTimeout(() => {
          this.refresh$.next();
          this.triggering = false;
        }, 3000);
      },
      error: () => { this.triggering = false; }
    });
  }

  ngOnInit(): void {
    this.sub = merge(interval(30_000).pipe(startWith(0)), this.refresh$).pipe(
      switchMap(() => forkJoin({
        page: this.service.getLatest(1, 50),
        gainers: this.service.getTopGainers(10),
        losers: this.service.getTopLosers(10),
        stats: this.service.getStats()
      }))
    ).subscribe({
      next: ({ page, gainers, losers, stats }) => {
        this.coins = page.data;
        this.gainers = gainers;
        this.losers = losers;
        this.stats = stats;
        this.loading = false;
        this.lastUpdated = new Date().toLocaleTimeString('pt-BR');

        if (this.selectedCoin) {
          this.loadHistory(this.selectedCoin);
        }
      },
      error: () => { this.loading = false; }
    });
  }

  onSelectCoin(coin: CryptoPrice): void {
    this.selectedCoin = coin;
    this.loadHistory(coin);
  }

  private loadHistory(coin: CryptoPrice): void {
    this.service.getHistory(coin.symbol).subscribe(h => {
      this.selectedHistory = h;
    });
  }

  get totalRecords(): string {
    return this.stats?.totalRecords.toLocaleString('pt-BR') ?? '—';
  }

  get lastCollected(): string {
    if (!this.stats?.lastCollectedAt) return '—';
    return new Date(this.stats.lastCollectedAt).toLocaleTimeString('pt-BR');
  }

  get positiveCount(): number {
    return this.coins.filter(c => c.change24h >= 0).length;
  }

  get negativeCount(): number {
    return this.coins.filter(c => c.change24h < 0).length;
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}

import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { CryptoPrice } from '../../models/crypto-price.model';

@Component({
  selector: 'app-price-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  template: `
    <div class="bg-gray-800 rounded-xl border border-gray-700 p-5">
      <div class="flex items-center justify-between mb-4">
        <h3 class="text-white font-semibold text-lg">
          Histórico de Preço — <span class="text-indigo-400">{{ symbol }}</span>
        </h3>
        <span class="text-gray-400 text-sm">{{ history.length }} coletas</span>
      </div>

      <div *ngIf="history.length === 0" class="flex items-center justify-center h-48 text-gray-500">
        Nenhum histórico disponível ainda.
      </div>

      <canvas *ngIf="history.length > 0" baseChart
        [data]="chartData"
        [options]="chartOptions"
        [type]="'line'">
      </canvas>
    </div>
  `
})
export class PriceChartComponent implements OnChanges {
  @Input() history: CryptoPrice[] = [];
  @Input() symbol = '';

  chartData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };

  chartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    interaction: { intersect: false, mode: 'index' },
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: ctx => ` $${(ctx.raw as number).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 6 })}`
        }
      }
    },
    scales: {
      x: {
        ticks: { color: '#9ca3af', maxTicksLimit: 8 },
        grid: { color: '#374151' }
      },
      y: {
        ticks: {
          color: '#9ca3af',
          callback: v => `$${Number(v).toLocaleString('en-US', { minimumFractionDigits: 2 })}`
        },
        grid: { color: '#374151' }
      }
    }
  };

  ngOnChanges(): void {
    const sorted = [...this.history].sort(
      (a, b) => new Date(a.collectedAt).getTime() - new Date(b.collectedAt).getTime()
    );

    this.chartData = {
      labels: sorted.map(h =>
        new Date(h.collectedAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
      ),
      datasets: [{
        data: sorted.map(h => h.priceUsd),
        borderColor: '#6366f1',
        backgroundColor: 'rgba(99,102,241,0.15)',
        fill: true,
        tension: 0.4,
        pointRadius: 3,
        pointBackgroundColor: '#6366f1'
      }]
    };
  }
}

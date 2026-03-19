import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CryptoPrice } from '../../models/crypto-price.model';
import { LargeNumberPipe } from '../../pipes/large-number.pipe';

@Component({
  selector: 'app-coin-table',
  standalone: true,
  imports: [CommonModule, LargeNumberPipe],
  template: `
    <div class="overflow-x-auto rounded-xl border border-gray-700">
      <table class="w-full text-sm text-left">
        <thead class="bg-gray-800 text-gray-400 uppercase text-xs">
          <tr>
            <th class="px-4 py-3">#</th>
            <th class="px-4 py-3">Moeda</th>
            <th class="px-4 py-3 text-right">Preço</th>
            <th class="px-4 py-3 text-right">24h</th>
            <th class="px-4 py-3 text-right">Market Cap</th>
            <th class="px-4 py-3 text-right">Volume 24h</th>
            <th class="px-4 py-3 text-center">Histórico</th>
          </tr>
        </thead>
        <tbody>
          <tr
            *ngFor="let coin of coins; let i = index"
            class="border-t border-gray-700 hover:bg-gray-800 transition-colors cursor-pointer"
            [class.bg-gray-800]="selected?.coinId === coin.coinId"
          >
            <td class="px-4 py-3 text-gray-500">{{ i + 1 }}</td>
            <td class="px-4 py-3">
              <div class="flex items-center gap-2">
                <span class="font-semibold text-white">{{ coin.symbol }}</span>
                <span class="text-gray-400">{{ coin.name }}</span>
              </div>
            </td>
            <td class="px-4 py-3 text-right text-white font-mono">
              {{ coin.priceUsd | currency:'USD':'symbol':'1.2-6' }}
            </td>
            <td class="px-4 py-3 text-right font-semibold"
                [class.text-emerald-400]="coin.change24h >= 0"
                [class.text-red-400]="coin.change24h < 0">
              {{ coin.change24h >= 0 ? '+' : '' }}{{ coin.change24h | number:'1.2-2' }}%
            </td>
            <td class="px-4 py-3 text-right text-gray-300">{{ coin.marketCap | largeNumber }}</td>
            <td class="px-4 py-3 text-right text-gray-300">{{ coin.volume24h | largeNumber }}</td>
            <td class="px-4 py-3 text-center">
              <button
                (click)="selectCoin.emit(coin)"
                class="text-xs px-3 py-1 rounded-full bg-indigo-600 hover:bg-indigo-500 text-white transition-colors"
              >
                Ver
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class CoinTableComponent {
  @Input() coins: CryptoPrice[] = [];
  @Input() selected: CryptoPrice | null = null;
  @Output() selectCoin = new EventEmitter<CryptoPrice>();
}

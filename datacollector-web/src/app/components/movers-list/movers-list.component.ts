import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CryptoPrice } from '../../models/crypto-price.model';

@Component({
  selector: 'app-movers-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-gray-800 rounded-xl border border-gray-700 overflow-hidden">
      <div class="px-4 py-3 border-b border-gray-700 flex items-center gap-2">
        <span class="text-lg">{{ icon }}</span>
        <span class="font-semibold text-white">{{ title }}</span>
      </div>
      <ul>
        <li
          *ngFor="let coin of coins"
          class="flex items-center justify-between px-4 py-3 border-b border-gray-700 last:border-0 hover:bg-gray-750 transition-colors"
        >
          <div class="flex items-center gap-2">
            <span class="font-bold text-white text-sm w-12">{{ coin.symbol }}</span>
            <span class="text-gray-400 text-xs">{{ coin.name }}</span>
          </div>
          <span class="font-semibold text-sm"
                [class.text-emerald-400]="coin.change24h >= 0"
                [class.text-red-400]="coin.change24h < 0">
            {{ coin.change24h >= 0 ? '+' : '' }}{{ coin.change24h | number:'1.2-2' }}%
          </span>
        </li>
      </ul>
    </div>
  `
})
export class MoversListComponent {
  @Input() coins: CryptoPrice[] = [];
  @Input() title = '';
  @Input() icon = '';
}

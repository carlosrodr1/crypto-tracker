import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-gray-800 rounded-xl p-5 border border-gray-700 flex flex-col gap-1">
      <span class="text-gray-400 text-sm">{{ label }}</span>
      <span class="text-white text-2xl font-bold">{{ value }}</span>
      <span *ngIf="sub" class="text-gray-500 text-xs">{{ sub }}</span>
    </div>
  `
})
export class StatCardComponent {
  @Input() label = '';
  @Input() value = '';
  @Input() sub = '';
}

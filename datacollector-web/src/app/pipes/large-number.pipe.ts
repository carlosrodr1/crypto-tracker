import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'largeNumber', standalone: true })
export class LargeNumberPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    if (value == null) return '—';
    if (value >= 1_000_000_000_000) return `$${(value / 1_000_000_000_000).toFixed(2)}T`;
    if (value >= 1_000_000_000) return `$${(value / 1_000_000_000).toFixed(2)}B`;
    if (value >= 1_000_000) return `$${(value / 1_000_000).toFixed(2)}M`;
    return `$${value.toLocaleString()}`;
  }
}

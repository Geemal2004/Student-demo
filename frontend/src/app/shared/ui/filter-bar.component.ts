import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-filter-bar',
  standalone: true,
  template: '<section class="filter-bar"><ng-content></ng-content></section>',
  styles: [
    '.filter-bar{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:.8rem;background:#fff;border:1px solid var(--surface-border);border-radius:14px;padding:.8rem}',
    '@media (max-width:960px){.filter-bar{grid-template-columns:1fr}}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterBarComponent {}

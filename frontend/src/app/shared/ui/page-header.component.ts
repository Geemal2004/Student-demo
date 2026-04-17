import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [NgIf],
  template: `
    <header class="page-header">
      <div>
        <h1>{{ title }}</h1>
        <p *ngIf="subtitle">{{ subtitle }}</p>
      </div>
      <div class="actions">
        <ng-content select="[actions]"></ng-content>
      </div>
    </header>
  `,
  styles: [
    '.page-header{display:flex;justify-content:space-between;align-items:flex-end;gap:1rem;margin-bottom:1rem}',
    'h1{margin:0;font-size:1.5rem}',
    'p{margin:.25rem 0 0;color:var(--text-muted)}',
    '.actions{display:flex;gap:.6rem;flex-wrap:wrap}',
    '@media (max-width: 800px){.page-header{flex-direction:column;align-items:flex-start}}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PageHeaderComponent {
  @Input({ required: true }) title = '';
  @Input() subtitle = '';
}

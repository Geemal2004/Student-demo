import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <section class="empty-state">
      <mat-icon>{{ icon }}</mat-icon>
      <h3>{{ title }}</h3>
      <p>{{ message }}</p>
      <ng-content></ng-content>
    </section>
  `,
  styles: [
    '.empty-state{border:1px dashed var(--surface-border);background:#fff;border-radius:14px;padding:1.4rem;text-align:center;display:grid;place-items:center;gap:.4rem}',
    'mat-icon{color:var(--accent-blue)}',
    'h3{margin:.3rem 0 0}',
    'p{margin:0;color:var(--text-muted)}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmptyStateComponent {
  @Input() icon = 'inbox';
  @Input() title = 'Nothing here yet';
  @Input() message = 'No records found for the current filters.';
}

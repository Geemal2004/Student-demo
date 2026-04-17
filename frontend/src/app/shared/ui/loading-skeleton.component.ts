import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-loading-skeleton',
  standalone: true,
  imports: [NgFor],
  template: `
    <div class="skeleton-wrap">
      <div class="skeleton" *ngFor="let row of rowsArray"></div>
    </div>
  `,
  styles: [
    '.skeleton-wrap{display:grid;gap:.6rem}',
    '.skeleton{height:52px;border-radius:10px;background:linear-gradient(90deg,#edf2fb,#f8fbff,#edf2fb);background-size:220% 100%;animation:shimmer 1.4s infinite}',
    '@keyframes shimmer{0%{background-position:100% 0}100%{background-position:-100% 0}}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadingSkeletonComponent {
  @Input() rows = 4;

  get rowsArray(): number[] {
    return Array.from({ length: this.rows }, (_, i) => i);
  }
}

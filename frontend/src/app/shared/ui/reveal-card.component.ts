import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { DatePipe, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RevealDetailsDto } from '../../core/models/api.models';

@Component({
  selector: 'app-reveal-card',
  standalone: true,
  imports: [NgIf, DatePipe, MatCardModule, MatIconModule],
  template: `
    <mat-card class="reveal-card" *ngIf="details">
      <div class="title-row">
        <mat-icon>celebration</mat-icon>
        <h3>Match Revealed</h3>
      </div>
      <p><strong>Supervisor:</strong> {{ details.supervisorName }}</p>
      <p><strong>Email:</strong> {{ details.supervisorEmail }}</p>
      <p *ngIf="details.confirmedAt"><strong>Confirmed:</strong> {{ details.confirmedAt | date:'medium' }}</p>
    </mat-card>
  `,
  styles: [
    '.reveal-card{border-radius:14px;border:1px solid #d9c9ff;background:linear-gradient(120deg,#f8f2ff,#ffffff)}',
    '.title-row{display:flex;align-items:center;gap:.4rem;color:var(--accent-purple)}',
    'h3{margin:0}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RevealCardComponent {
  @Input() details: RevealDetailsDto | undefined;
}

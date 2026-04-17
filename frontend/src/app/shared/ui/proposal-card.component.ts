import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { NgIf } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { StatusBadgeComponent } from './status-badge.component';

@Component({
  selector: 'app-proposal-card',
  standalone: true,
  imports: [NgIf, MatCardModule, MatButtonModule, StatusBadgeComponent],
  template: `
    <mat-card class="proposal-card">
      <div class="header">
        <h3>{{ title }}</h3>
        <app-status-badge [status]="status"></app-status-badge>
      </div>
      <p class="abstract">{{ abstract }}</p>
      <small>{{ researchArea }}<span *ngIf="techStack"> • {{ techStack }}</span></small>
      <div class="actions">
        <ng-content select="[actions]"></ng-content>
        <button *ngIf="showPrimary" mat-flat-button color="primary" (click)="primaryClick.emit()">{{ primaryLabel }}</button>
      </div>
    </mat-card>
  `,
  styles: [
    '.proposal-card{border-radius:14px;display:grid;gap:.6rem}',
    '.header{display:flex;justify-content:space-between;align-items:flex-start;gap:.6rem}',
    'h3{margin:0;font-size:1.05rem}',
    '.abstract{margin:0;color:var(--text-muted)}',
    'small{color:var(--text-muted)}',
    '.actions{display:flex;justify-content:flex-end;gap:.6rem;flex-wrap:wrap}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProposalCardComponent {
  @Input({ required: true }) title = '';
  @Input({ required: true }) abstract = '';
  @Input({ required: true }) researchArea = '';
  @Input() techStack = '';
  @Input() status = '';
  @Input() primaryLabel = 'Open';
  @Input() showPrimary = false;
  @Output() primaryClick = new EventEmitter<void>();
}

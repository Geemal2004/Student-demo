import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { NgFor } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';
import { ResearchAreaDto } from '../../core/models/api.models';

@Component({
  selector: 'app-expertise-chip-selector',
  standalone: true,
  imports: [NgFor, MatChipsModule],
  template: `
    <mat-chip-set>
      <mat-chip *ngFor="let area of areas"
        [class.selected]="selectedIds.includes(area.id)"
        (click)="toggle(area.id)">
        {{ area.name }}
      </mat-chip>
    </mat-chip-set>
  `,
  styles: [
    'mat-chip{cursor:pointer}',
    'mat-chip.selected{background:var(--accent-blue);color:#fff}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExpertiseChipSelectorComponent {
  @Input() areas: ResearchAreaDto[] = [];
  @Input() selectedIds: number[] = [];
  @Output() selectedIdsChange = new EventEmitter<number[]>();

  toggle(id: number): void {
    const next = this.selectedIds.includes(id)
      ? this.selectedIds.filter((value) => value !== id)
      : [...this.selectedIds, id];

    this.selectedIdsChange.emit(next);
  }
}

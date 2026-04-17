import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-search-box',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatInputModule, MatIconModule],
  template: `
    <mat-form-field appearance="outline" class="search-box">
      <mat-label>{{ label }}</mat-label>
      <mat-icon matPrefix>search</mat-icon>
      <input matInput [placeholder]="placeholder" [ngModel]="value" (ngModelChange)="valueChange.emit($event)" />
    </mat-form-field>
  `,
  styles: ['.search-box{width:100%}'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchBoxComponent {
  @Input() label = 'Search';
  @Input() placeholder = 'Search';
  @Input() value = '';
  @Output() valueChange = new EventEmitter<string>();
}

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MatButtonModule],
  template: `
    <h2>{{ data.title }}</h2>
    <p>{{ data.message }}</p>
    <div class="dialog-actions">
      <button mat-stroked-button (click)="close(false)">Cancel</button>
      <button mat-flat-button color="warn" (click)="close(true)">{{ data.confirmText || 'Confirm' }}</button>
    </div>
  `,
  styles: [
    '.dialog-actions{display:flex;justify-content:flex-end;gap:.6rem;margin-top:1rem}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmDialogComponent {
  readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);

  close(result: boolean): void {
    this.dialogRef.close(result);
  }
}

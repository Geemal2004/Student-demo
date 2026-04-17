import { DatePipe } from '@angular/common';
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { StudentApiService } from '../../core/services/student-api.service';
import { StudentDashboardDto } from '../../core/models/api.models';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, RouterLink, DatePipe],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StudentDashboardComponent {
  private readonly api = inject(StudentApiService);
  readonly dashboard = signal<StudentDashboardDto | null>(null);

  constructor() {
    this.api.getDashboard().subscribe((data) => this.dashboard.set(data));
  }
}

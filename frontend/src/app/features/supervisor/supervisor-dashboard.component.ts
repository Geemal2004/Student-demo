import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { SupervisorApiService } from '../../core/services/supervisor-api.service';
import { SupervisorDashboardDto } from '../../core/models/api.models';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { StatCardComponent } from '../../shared/ui/stat-card.component';

@Component({
  standalone: true,
  selector: 'app-supervisor-dashboard',
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, PageHeaderComponent, StatCardComponent],
  template: `
    <app-page-header title="Supervisor Dashboard" subtitle="Manage interests, confirmations, and expertise settings."></app-page-header>

    <section class="stats-grid" *ngIf="dashboard() as d">
      <app-stat-card label="Interests" [value]="d.totalInterests" hint="Awaiting confirmation"></app-stat-card>
      <app-stat-card label="Confirmed" [value]="d.confirmedMatches" hint="Identity revealed"></app-stat-card>
      <app-stat-card label="Expertise Areas" [value]="d.expertiseAreas" hint="Influences recommendation quality"></app-stat-card>
    </section>

    <mat-card class="quick-actions">
      <h3>Quick Actions</h3>
      <div class="action-row">
        <a mat-stroked-button routerLink="/supervisor/browse">Browse proposals</a>
        <a mat-stroked-button routerLink="/supervisor/interests">Review interests</a>
        <a mat-stroked-button routerLink="/supervisor/confirmed">View confirmed</a>
        <a mat-flat-button color="primary" routerLink="/supervisor/expertise">Manage expertise</a>
      </div>
    </mat-card>
  `,
  styleUrl: './supervisor-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SupervisorDashboardComponent {
  private readonly api = inject(SupervisorApiService);
  readonly dashboard = signal<SupervisorDashboardDto | null>(null);

  constructor() {
    this.api.getDashboard().subscribe((data) => this.dashboard.set(data));
  }
}

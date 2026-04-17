import { CommonModule, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { StudentApiService } from '../../core/services/student-api.service';
import { ProposalDetailsDto } from '../../core/models/api.models';
import { ToastService } from '../../core/services/toast.service';
import { EmptyStateComponent } from '../../shared/ui/empty-state.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';
import { RevealCardComponent } from '../../shared/ui/reveal-card.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge.component';

@Component({
  selector: 'app-student-proposal-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    DatePipe,
    MatCardModule,
    MatButtonModule,
    PageHeaderComponent,
    RevealCardComponent,
    StatusBadgeComponent,
    EmptyStateComponent
  ],
  templateUrl: './student-proposal-details.component.html',
  styleUrl: './student-proposal-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StudentProposalDetailsComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(StudentApiService);
  private readonly toast = inject(ToastService);

  readonly proposal = signal<ProposalDetailsDto | null>(null);

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isFinite(id) || id <= 0) {
      this.toast.error('Invalid proposal id.');
      return;
    }

    this.api.getProposal(id).subscribe({
      next: (proposal) => this.proposal.set(proposal),
      error: (error) => this.toast.error(error?.error?.message ?? 'Unable to load proposal details.')
    });
  }

  isPending(status: string): boolean {
    return status.toLowerCase() === 'pending';
  }
}

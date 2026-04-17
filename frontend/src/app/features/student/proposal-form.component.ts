import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ProposalUpsertRequest, StudentApiService } from '../../core/services/student-api.service';
import { ResearchAreaDto } from '../../core/models/api.models';
import { ToastService } from '../../core/services/toast.service';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';

@Component({
  selector: 'app-proposal-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PageHeaderComponent,
    LoadingSkeletonComponent
  ],
  templateUrl: './proposal-form.component.html',
  styleUrl: './proposal-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProposalFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(StudentApiService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly areas = signal<ResearchAreaDto[]>([]);
  readonly loading = signal(false);
  readonly editing = signal(false);

  private proposalId: number | null = null;

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(150)]],
    abstract: ['', [Validators.required, Validators.minLength(100), Validators.maxLength(1000)]],
    technicalStack: ['', [Validators.maxLength(200)]],
    researchAreaId: [0, [Validators.required, Validators.min(1)]]
  });

  constructor() {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam !== null) {
      const parsedId = Number(idParam);
      if (!Number.isFinite(parsedId) || parsedId <= 0) {
        this.toast.error('Invalid proposal id.');
        void this.router.navigate(['/student/proposals']);
        return;
      }

      this.editing.set(true);
      this.proposalId = parsedId;
    }

    this.api.getResearchAreas().subscribe({
      next: (areas) => {
        this.areas.set(areas);
        if (this.proposalId !== null) {
          this.loadProposalForEdit(this.proposalId);
        }
      },
      error: (error) => this.toast.error(error?.error?.message ?? 'Unable to load research areas.')
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const payload: ProposalUpsertRequest = {
      title: raw.title,
      abstract: raw.abstract,
      technicalStack: raw.technicalStack.trim() ? raw.technicalStack : undefined,
      researchAreaId: raw.researchAreaId
    };

    if (this.proposalId !== null) {
      this.api.updateProposal(this.proposalId, payload).subscribe({
        next: () => {
          this.toast.success('Proposal updated successfully.');
          void this.router.navigate(['/student/proposals', this.proposalId]);
        },
        error: (error) => this.toast.error(error?.error?.message ?? 'Unable to update proposal.')
      });
      return;
    }

    this.api.createProposal(payload).subscribe({
      next: () => {
        this.toast.success('Proposal created successfully.');
        void this.router.navigate(['/student/proposals']);
      },
      error: (error) => this.toast.error(error?.error?.message ?? 'Unable to create proposal.')
    });
  }

  private loadProposalForEdit(id: number): void {
    this.loading.set(true);
    this.api.getProposal(id).subscribe({
      next: (proposal) => {
        // DECISION: backend details currently return research area name, so edit prefill resolves id by name.
        const researchAreaId = this.resolveResearchAreaId(proposal.researchAreaName);

        this.form.patchValue({
          title: proposal.title,
          abstract: proposal.abstract,
          technicalStack: proposal.technicalStack ?? '',
          researchAreaId
        });

        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load proposal for editing.');
      }
    });
  }

  private resolveResearchAreaId(researchAreaName: string): number {
    const area = this.areas().find((item) => item.name.toLowerCase() === researchAreaName.toLowerCase());
    return area?.id ?? 0;
  }
}

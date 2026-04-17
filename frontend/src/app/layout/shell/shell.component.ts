import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = this.auth.currentUser;
  readonly navLinks = computed(() => {
    const role = this.user()?.role;
    if (role === 'Student') {
      return [{ label: 'Dashboard', link: '/student' }, { label: 'Proposals', link: '/student/proposals' }, { label: 'Create', link: '/student/create' }];
    }
    if (role === 'Supervisor') {
      return [
        { label: 'Dashboard', link: '/supervisor' },
        { label: 'Browse', link: '/supervisor/browse' },
        { label: 'Interests', link: '/supervisor/interests' },
        { label: 'Confirmed', link: '/supervisor/confirmed' },
        { label: 'Expertise', link: '/supervisor/expertise' }
      ];
    }
    if (role === 'ModuleLeader') {
      return [
        { label: 'Dashboard', link: '/module-leader' },
        { label: 'Matches', link: '/module-leader/matches' },
        { label: 'Research Areas', link: '/module-leader/research-areas' },
        { label: 'Users', link: '/module-leader/users' }
      ];
    }
    if (role === 'SysAdmin') {
      return [{ label: 'Dashboard', link: '/admin' }, { label: 'Users', link: '/admin/users' }, { label: 'Migrations', link: '/admin/migrations' }, { label: 'Audit Logs', link: '/admin/audit-logs' }];
    }
    return [];
  });

  logout(): void {
    this.auth.logout();
  }

  goHome(): void {
    void this.router.navigate(['/']);
  }
}

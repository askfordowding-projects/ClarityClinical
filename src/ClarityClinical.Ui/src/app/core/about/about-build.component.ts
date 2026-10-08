import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BuildInfo, BuildInfoService } from './build-info.service';

@Component({
  selector: 'app-about-build',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './about-build.component.html',
  styleUrl: './about-build.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AboutBuildComponent implements OnInit {
  private readonly service = inject(BuildInfoService);
  protected readonly build = signal<BuildInfo | null>(null);
  protected readonly error = signal<string | null>(null);
  ngOnInit(): void {
    this.service.getBuild().subscribe({
      next: value => this.build.set(value),
      error: () => this.error.set('Build information could not be loaded.')
    });
  }
}

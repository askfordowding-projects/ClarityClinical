import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AboutBuildComponent } from './about-build.component';
import { BuildInfoService } from './build-info.service';

describe('AboutBuildComponent', () => {
  it('shows reproducible build identity and safety context', async () => {
    await TestBed.configureTestingModule({
      imports: [AboutBuildComponent],
      providers: [
        provideRouter([]),
        { provide: BuildInfoService, useValue: { getBuild: () => of({
          version: '0.1.0', commit: 'abc1234', environment: 'PublicDemo',
          latestMigration: '20261008121157_AddGovernanceSourcesAndRules',
          ruleSetVersion: 'demo-illustrative-1.0',
          buildTimestamp: '2026-10-08T12:00:00Z', isDemo: true
        }) } }
      ]
    }).compileComponents();

    const fixture = TestBed.createComponent(AboutBuildComponent);
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('0.1.0');
    expect(text).toContain('abc1234');
    expect(text).toContain('PublicDemo');
    expect(text).toContain('AddGovernanceSourcesAndRules');
    expect(text).toContain('demo-illustrative-1.0');
    expect(text.toLowerCase()).toContain('synthetic data');
  });
});

import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { DemoSessionService } from '../../core/auth/demo-session.service';
import { ConsultationListComponent } from './consultation-list.component';

describe('ConsultationListComponent', () => {
  it('starts the Miguel scenario and opens the clinical workspace', async () => {
    const session = jasmine.createSpyObj<DemoSessionService>('DemoSessionService', ['startScenario']);
    session.startScenario.and.returnValue(of('11111111-1111-1111-1111-111111111111'));

    await TestBed.configureTestingModule({
      imports: [ConsultationListComponent],
      providers: [provideRouter([]), { provide: DemoSessionService, useValue: session }]
    }).compileComponents();

    const fixture = TestBed.createComponent(ConsultationListComponent);
    const router = TestBed.inject(Router);
    spyOn(router, 'navigateByUrl').and.resolveTo(true);
    fixture.detectChanges();

    const root = fixture.nativeElement as HTMLElement;
    expect(root.textContent).toContain('Miguel Santos');
    expect(root.textContent).toContain('SYNTHETIC DATA ONLY');
    const button = root.querySelector('[data-action="start-miguel"]') as HTMLButtonElement;
    expect(button).not.toBeNull();
    button.click();
    fixture.detectChanges();

    expect(session.startScenario).toHaveBeenCalledOnceWith('miguel-santos-leg-swelling');
    expect(router.navigateByUrl).toHaveBeenCalledOnceWith('/clinical/consultations/11111111-1111-1111-1111-111111111111');
  });
});

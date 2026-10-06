import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { DemoLoginComponent } from './demo-login.component';

describe('DemoLoginComponent', () => {
  let fixture: ComponentFixture<DemoLoginComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DemoLoginComponent],
      providers: [provideHttpClient(), provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(DemoLoginComponent);
    fixture.detectChanges();
  });

  it('offers public Clinician and Administrator demo entry points', () => {
    const element = fixture.nativeElement as HTMLElement;
    const text = element.textContent ?? '';

    expect(text).toContain('Enter as Clinician');
    expect(text).toContain('Enter as Administrator');
    expect(text).toContain('SYNTHETIC DATA ONLY');
    expect(text).toContain('organisation-controlled authentication');
  });
});

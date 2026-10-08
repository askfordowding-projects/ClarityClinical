import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DemoSessionService } from './demo-session.service';

describe('DemoSessionService', () => {
  it('creates and starts a scenario before returning the consultation id', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(DemoSessionService);
    const http = TestBed.inject(HttpTestingController);
    let consultationId: string | undefined;

    service.startScenario('miguel-santos-leg-swelling').subscribe(id => consultationId = id);

    const create = http.expectOne('/api/demo/sessions');
    expect(create.request.method).toBe('POST');
    expect(create.request.body).toEqual({ scenarioKey: 'miguel-santos-leg-swelling' });
    create.flush({ id: 'session', scenarioId: 'scenario', consultationId: 'consultation-123', expiresAt: '2026-10-08T15:00:00Z' });

    const start = http.expectOne('/api/consultations/consultation-123/start');
    expect(start.request.method).toBe('POST');
    start.flush({});

    expect(consultationId).toBe('consultation-123');
    http.verify();
  });
});

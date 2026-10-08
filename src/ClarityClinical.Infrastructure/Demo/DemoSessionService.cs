using ClarityClinical.Application.Demo;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Domain.Demo;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Demo;

public sealed class DemoSessionService(
    ClarityClinicalDbContext dbContext,
    IDemoScenarioRepository scenarioRepository) : IDemoSessionService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(2);

    public async Task<DemoSession> CreateAsync(
        string scenarioKey,
        string visitorId,
        CancellationToken cancellationToken)
    {
        var scenario = await scenarioRepository.GetScenarioForVisitorAsync(scenarioKey, visitorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Demo scenario '{scenarioKey}' was not found.");
        var now = DateTimeOffset.UtcNow;
        var consultation = await CreateConsultationAsync(scenario.PatientId, now, cancellationToken);
        var session = new DemoSession(
            Guid.NewGuid(),
            scenario.Id,
            consultation.Id,
            visitorId,
            now,
            now,
            now.Add(SessionLifetime));

        dbContext.Consultations.Add(consultation);
        dbContext.DemoSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);
        return session;
    }

    public Task<DemoSession?> GetAsync(
        Guid sessionId,
        string visitorId,
        CancellationToken cancellationToken) =>
        dbContext.DemoSessions.SingleOrDefaultAsync(
            session => session.Id == sessionId
                && session.VisitorId == visitorId
                && session.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken);

    public async Task<DemoSession> ResetAsync(
        Guid sessionId,
        string visitorId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var session = await dbContext.DemoSessions.SingleOrDefaultAsync(
            item => item.Id == sessionId && item.VisitorId == visitorId,
            cancellationToken)
            ?? throw new KeyNotFoundException($"Demo session '{sessionId}' was not found.");
        var scenario = await dbContext.DemoScenarios.SingleAsync(
            item => item.Id == session.ScenarioId,
            cancellationToken);
        var oldConsultationId = session.ConsultationId;
        var now = DateTimeOffset.UtcNow;
        var replacement = await CreateConsultationAsync(scenario.PatientId, now, cancellationToken);

        dbContext.Consultations.Add(replacement);
        session.ReplaceConsultation(replacement.Id, now, SessionLifetime);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.AuditEvents
            .Where(item => item.ConsultationId == oldConsultationId)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.Consultations
            .Where(item => item.Id == oldConsultationId)
            .ExecuteDeleteAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return session;
    }

    public Task<bool> OwnsConsultationAsync(
        Guid consultationId,
        string visitorId,
        CancellationToken cancellationToken) =>
        dbContext.DemoSessions.AnyAsync(
            session => session.ConsultationId == consultationId
                && session.VisitorId == visitorId
                && session.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken);

    private async Task<Consultation> CreateConsultationAsync(
        Guid patientId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var patient = await dbContext.Patients
            .Include(item => item.Conditions)
            .SingleAsync(item => item.Id == patientId, cancellationToken);
        var consultation = new Consultation(Guid.NewGuid(), patientId);

        foreach (var condition in patient.Conditions)
        {
            consultation.AddFact(new ClinicalFact(
                Guid.NewGuid(),
                condition.Code,
                condition.DisplayName,
                ClinicalFactSource.EstablishedRecord,
                occurredAt));
        }

        return consultation;
    }
    public async Task<int> DeleteExpiredAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var expired = await dbContext.DemoSessions
            .Where(session => session.ExpiresAt <= now)
            .Select(session => new { session.Id, session.ConsultationId })
            .ToListAsync(cancellationToken);
        if (expired.Count == 0)
        {
            return 0;
        }

        var sessionIds = expired.Select(item => item.Id).ToArray();
        var consultationIds = expired.Select(item => item.ConsultationId).ToArray();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.AuditEvents.Where(item => consultationIds.Contains(item.ConsultationId)).ExecuteDeleteAsync(cancellationToken);
        await dbContext.DemoSessions.Where(item => sessionIds.Contains(item.Id)).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Consultations.Where(item => consultationIds.Contains(item.Id)).ExecuteDeleteAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return expired.Count;
    }
}

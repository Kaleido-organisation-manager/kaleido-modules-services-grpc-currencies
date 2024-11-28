using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;

public class DenominationRevisionBuilder
{
    private readonly DenominationRevisionEntity _revision = new()
    {
        Key = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        Action = RevisionAction.Created,
        Status = RevisionStatus.Active,
        Revision = 1
    };

    public DenominationRevisionBuilder WithKey(Guid key)
    {
        _revision.Key = key;
        return this;
    }

    public DenominationRevisionBuilder WithCreatedAt(DateTime createdAt)
    {
        _revision.CreatedAt = createdAt;
        return this;
    }

    public DenominationRevisionBuilder WithAction(RevisionAction action)
    {
        _revision.Action = action;
        return this;
    }

    public DenominationRevisionBuilder WithStatus(RevisionStatus status)
    {
        _revision.Status = status;
        return this;
    }

    public DenominationRevisionBuilder WithRevision(int revision)
    {
        _revision.Revision = revision;
        return this;
    }

    public DenominationRevisionEntity Build() => _revision;
}
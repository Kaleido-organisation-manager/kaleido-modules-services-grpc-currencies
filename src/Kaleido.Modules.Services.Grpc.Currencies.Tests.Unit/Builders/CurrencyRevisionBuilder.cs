using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;

public class CurrencyRevisionBuilder
{
    private readonly CurrencyRevisionEntity _revision = new()
    {
        Key = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        Action = RevisionAction.Created,
        Status = RevisionStatus.Active,
        Revision = 1
    };

    public CurrencyRevisionBuilder WithKey(Guid key)
    {
        _revision.Key = key;
        return this;
    }

    public CurrencyRevisionBuilder WithCreatedAt(DateTime createdAt)
    {
        _revision.CreatedAt = createdAt;
        return this;
    }

    public CurrencyRevisionBuilder WithAction(RevisionAction action)
    {
        _revision.Action = action;
        return this;
    }

    public CurrencyRevisionBuilder WithStatus(RevisionStatus status)
    {
        _revision.Status = status;
        return this;
    }

    public CurrencyRevisionBuilder WithRevision(int revision)
    {
        _revision.Revision = revision;
        return this;
    }

    public CurrencyRevisionEntity Build() => _revision;
}
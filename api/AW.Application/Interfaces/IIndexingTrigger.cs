using AW.Domain.Common;

namespace AW.Application.Interfaces;

public interface IIndexingTrigger
{
    Result<bool> TriggerIndexing();
}

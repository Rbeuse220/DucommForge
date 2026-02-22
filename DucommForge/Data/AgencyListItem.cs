namespace DucommForge.Data;

public sealed class AgencyListItem
{
    public int AgencyId { get; init; }
    public int DispatchCenterId { get; init; }

    public required string Short { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }

    public bool Owned { get; init; }
    public bool Active { get; init; }

    public required string DispatchCenterCode { get; init; }
}
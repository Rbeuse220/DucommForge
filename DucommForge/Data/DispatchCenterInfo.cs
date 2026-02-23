namespace DucommForge.Data;

public sealed class DispatchCenterInfo
{
    public int DispatchCenterId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public bool Active { get; init; }
}
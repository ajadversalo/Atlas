namespace NetMinimal.Domain.Entities;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAtUtc { get; protected set; }
    public DateTimeOffset UpdatedAtUtc { get; protected set; }

    protected Entity()
    {
    }

    public override bool Equals(object? obj) =>
        obj is Entity other && (ReferenceEquals(this, other) || Id.Equals(other.Id));

    public override int GetHashCode() => Id.GetHashCode();
}

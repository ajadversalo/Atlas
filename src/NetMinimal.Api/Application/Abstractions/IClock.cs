namespace NetMinimal.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

using System.Runtime.CompilerServices;
using NetMinimal.Domain.Exceptions;

namespace NetMinimal.Domain.StaticServices;

/// <summary>
/// Static argument and invariant checks used by entities and services.
/// </summary>
public static class Guard
{
    public static T AgainstNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
        where T : class
    {
        return value ?? throw new BusinessRuleException($"{name} is required.");
    }

    public static string AgainstNullOrWhiteSpace(
        string? value,
        int maxLength = int.MaxValue,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleException($"{name} is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new BusinessRuleException($"{name} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static string? Optional(
        string? value,
        int maxLength = int.MaxValue,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new BusinessRuleException($"{name} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static Guid AgainstEmpty(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value == Guid.Empty)
        {
            throw new BusinessRuleException($"{name} is required.");
        }

        return value;
    }

    public static int AgainstOutOfRange(
        int value,
        int min,
        int max,
        [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value < min || value > max)
        {
            throw new BusinessRuleException($"{name} must be between {min} and {max}.");
        }

        return value;
    }
}

namespace NetMinimal.Domain.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string entityName, object key)
        : base("not_found", $"{entityName} '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    public string EntityName { get; }
    public object Key { get; }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base("conflict", message)
    {
    }
}

public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base("business_rule", message)
    {
    }
}

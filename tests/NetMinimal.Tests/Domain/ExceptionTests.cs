using FluentAssertions;
using NetMinimal.Domain.Exceptions;

namespace NetMinimal.Tests.Domain;

public sealed class ExceptionTests
{
    [Fact]
    public void NotFoundException_includes_entity_and_key()
    {
        var id = Guid.NewGuid();
        var ex = new NotFoundException("Product", id);

        ex.Code.Should().Be("not_found");
        ex.EntityName.Should().Be("Product");
        ex.Key.Should().Be(id);
        ex.Message.Should().Contain(id.ToString()).And.Contain("Product");
    }

    [Fact]
    public void Conflict_and_business_rule_codes_are_stable()
    {
        new ConflictException("dup").Code.Should().Be("conflict");
        new BusinessRuleException("no").Code.Should().Be("business_rule");
    }
}

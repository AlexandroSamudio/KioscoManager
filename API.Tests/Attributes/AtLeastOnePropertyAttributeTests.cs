using API.Attributes;
using FluentAssertions;

namespace API.Tests.Attributes;

public class AtLeastOnePropertyAttributeTests
{
    private sealed class Dummy
    {
        public string? A { get; set; }
        public int? B { get; set; }
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenAllPropertiesNull()
    {
        var attr = new AtLeastOnePropertyAttribute();
        var obj = new Dummy { A = null, B = null };
        attr.IsValid(obj).Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenOnePropertyHasValue()
    {
        var attr = new AtLeastOnePropertyAttribute();
        var obj = new Dummy { A = "x", B = null };
        attr.IsValid(obj).Should().BeTrue();
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValueIsNull_ByDesign()
    {
        var attr = new AtLeastOnePropertyAttribute();
        object? obj = null;
        attr.IsValid(obj).Should().BeTrue();
    }
}

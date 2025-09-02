using FluentAssertions;
using Mottu.Rentals.Application.Validation;

namespace Mottu.Rentals.UnitTests;

public class CnpjValidatorTests
{
    [Theory]
    [InlineData("11.222.333/0001-81")] // valid formatted
    [InlineData("11222333000181")]     // valid digits only
    public void IsValid_returns_true_for_valid_cnpjs(string cnpj)
    {
        CnpjValidator.IsValid(cnpj).Should().BeTrue();
    }

    [Theory]
    [InlineData("11.222.333/0001-80")] // invalid check digits
    [InlineData("00000000000000")]     // repeated digits
    [InlineData("123")]                // too short
    public void IsValid_returns_false_for_invalid_cnpjs(string cnpj)
    {
        CnpjValidator.IsValid(cnpj).Should().BeFalse();
    }

    [Fact]
    public void Normalize_strips_non_digits()
    {
        CnpjValidator.Normalize("11.222.333/0001-81").Should().Be("11222333000181");
    }
}



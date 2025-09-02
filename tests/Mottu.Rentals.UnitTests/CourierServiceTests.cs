using FluentAssertions;
using Mottu.Rentals.Application.Abstractions;
using Mottu.Rentals.Application.Couriers;
using Mottu.Rentals.Contracts.Couriers;
using Mottu.Rentals.Domain.Entities;
using Mottu.Rentals.Domain.Enums;
using NSubstitute;

namespace Mottu.Rentals.UnitTests;

public class CourierServiceTests
{
    private readonly ICourierRepository _repo = Substitute.For<ICourierRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();

    private CourierService CreateSut() => new(_repo, _storage);

    [Fact]
    public async Task CreateAsync_rejects_invalid_cnpj()
    {
        var sut = CreateSut();
        var req = new CourierCreateRequest("id-1", "John", "00000000000000", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)), "CNH1", "A");
        var act = async () => await sut.CreateAsync(req, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Invalid CNPJ*");
    }

    [Fact]
    public async Task CreateAsync_normalizes_and_checks_uniqueness()
    {
        _repo.ExistsCnpjAsync("11222333000181", Arg.Any<CancellationToken>()).Returns(true);

        var sut = CreateSut();
        var req = new CourierCreateRequest("id-2", "Jane", "11.222.333/0001-81", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)), "CNH2", "A");

        var act = async () => await sut.CreateAsync(req, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
        await _repo.Received(1).ExistsCnpjAsync("11222333000181", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_persists_normalized_cnpj_on_success()
    {
        _repo.ExistsCnpjAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repo.ExistsCnhNumberAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        Courier? added = null;
        await _repo.AddAsync(Arg.Do<Courier>(c => added = c), Arg.Any<CancellationToken>());

        var sut = CreateSut();
        var req = new CourierCreateRequest("id-3", "Mary", "11.222.333/0001-81", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)), "CNH3", "A");
        var resp = await sut.CreateAsync(req, CancellationToken.None);

        added!.Cnpj.Should().Be("11222333000181");
        resp.Cnpj.Should().Be("11222333000181");
    }
}



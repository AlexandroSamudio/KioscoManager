using API.DTOs;
using API.Entities;
using API.Helpers;
using AutoMapper;
using FluentAssertions;

namespace API.Tests.Helpers;

public class AutoMapperProfilesTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfiles>());
        return config.CreateMapper();
    }

    [Fact]
    public void Producto_To_ProductoDto_Maps_CategoriaNombre()
    {
        var mapper = CreateMapper();
        var prod = new Producto
        {
            Id = 1,
            Sku = "1234567890123",
            Nombre = "N",
            PrecioCompra = 1,
            PrecioVenta = 2,
            Stock = 3,
            CategoriaId = 5,
            Categoria = new Categoria { Id = 5, Nombre = "Cat" }
        };
        var dto = mapper.Map<ProductoDto>(prod);
        dto.CategoriaNombre.Should().Be("Cat");
        dto.Id.Should().Be(1);
    }

    [Fact]
    public void ProfileUpdateDto_To_AppUser_Ignores_Nulls()
    {
        var mapper = CreateMapper();
        var existing = new AppUser { UserName = "old", Email = "old@example.com" };
        var update = new ProfileUpdateDto { UserName = null, Email = "new@example.com" };

        mapper.Map(update, existing);

        existing.UserName.Should().Be("old");
        existing.Email.Should().Be("new@example.com");
    }
}

using AutoMapper;
using API.DTOs;
using API.Entities;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
                .ForSourceMember(src => src.Password, opt => opt.DoNotValidate());

            CreateMap<AppUser, UserDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            CreateMap<Producto, ProductoDto>()
                .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria!.Nombre));

            CreateMap<ProductoCreateDto, Producto>();

            CreateMap<ProductoUpdateDto, Producto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.KioscoId, opt => opt.Ignore());

            CreateMap<ProductoVentaDto, DetalleVenta>()
                .ForMember(dest => dest.ProductoId, opt => opt.MapFrom(src => src.ProductoId))
                .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.Cantidad))
                .ForMember(dest => dest.PrecioUnitario, opt => opt.Ignore());

            CreateMap<Venta, VentaDto>()
                .ForMember(dest => dest.CantidadProductos, opt => opt.MapFrom(src => src.Detalles.Sum(d => d.Cantidad)));

            CreateMap<VentaCreateDto, Venta>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Productos));

            CreateMap<Compra, CompraDto>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));

            CreateMap<CompraDetalle, CompraDetalleViewDto>()
                .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto!.Nombre))
                .ForMember(dest => dest.ProductoSku, opt => opt.MapFrom(src => src.Producto!.Sku))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Cantidad * src.CostoUnitario));

            CreateMap<CompraDetalleDto, CompraDetalle>();
            CreateMap<CompraCreateDto, Compra>();

            CreateMap<KioscoConfig, KioscoConfigDto>();
            CreateMap<KioscoConfigUpdateDto, KioscoConfig>()
                .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => DateTime.UtcNow));
            
            CreateMap<UserPreferences, UserPreferencesDto>();
            CreateMap<UserPreferencesUpdateDto, UserPreferences>()
                .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<AppUser, UserManagementDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.KioscoId, opt => opt.MapFrom(src => src.KioscoId))
                .ForMember(dest => dest.Role, opt => opt.Ignore()) 
                .ForMember(dest => dest.NombreKiosco, opt => opt.Ignore()); 

            CreateMap<Kiosco, KioscoBasicInfoDto>();
            CreateMap<KioscoBasicInfoUpdateDto, Kiosco>();

            CreateMap<Categoria, CategoriaDto>();
            CreateMap<CategoriaCreateDto, Categoria>();
            CreateMap<CategoriaUpdateDto, Categoria>();
        }
    }
}

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
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
                .ForSourceMember(src => src.Password, opt => opt.DoNotValidate());

            //AppUser mappings
            CreateMap<AppUser, UserDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            CreateMap<AppUser, UserManagementDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.KioscoId, opt => opt.MapFrom(src => src.KioscoId))
                .ForMember(dest => dest.Role, opt => opt.Ignore()) 
                .ForMember(dest => dest.NombreKiosco, opt => opt.Ignore()); 


            //Producto mappings
            CreateMap<Producto, ProductoDto>()
                .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria!.Nombre));

            CreateMap<ProductoCreateDto, Producto>();

            CreateMap<ProductoUpdateDto, Producto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.KioscoId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            //Venta mappings
            CreateMap<ProductoVentaDto, DetalleVenta>()
                .ForMember(dest => dest.ProductoId, opt => opt.MapFrom(src => src.ProductoId))
                .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.Cantidad))
                .ForMember(dest => dest.PrecioUnitario, opt => opt.Ignore());

            CreateMap<Venta, VentaDto>()
                .ForMember(dest => dest.CantidadProductos, opt => opt.MapFrom(src => src.Detalles.Sum(d => d.Cantidad)));

            CreateMap<VentaCreateDto, Venta>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Productos));

            //Compra mappings
            CreateMap<Compra, CompraDto>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));

            CreateMap<CompraDetalle, CompraDetalleViewDto>()
                .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto!.Nombre))
                .ForMember(dest => dest.ProductoSku, opt => opt.MapFrom(src => src.Producto!.Sku))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Cantidad * src.CostoUnitario));

            CreateMap<CompraDetalleDto, CompraDetalle>();
            CreateMap<CompraCreateDto, Compra>();

            //KioscoConfig mappings
            CreateMap<KioscoConfig, KioscoConfigDto>();
            CreateMap<KioscoConfigUpdateDto, KioscoConfig>()
                .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            
            //UserPreferences mappings
            CreateMap<UserPreferences, UserPreferencesDto>();
            CreateMap<UserPreferencesUpdateDto, UserPreferences>()
                .ForMember(dest => dest.FechaActualizacion, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            //KioscoInfo mappings
            CreateMap<Kiosco, KioscoBasicInfoDto>();
            CreateMap<KioscoBasicInfoUpdateDto, Kiosco>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            //Categoria mappings
            CreateMap<Categoria, CategoriaDto>();
            CreateMap<CategoriaCreateDto, Categoria>();
            CreateMap<CategoriaUpdateDto, Categoria>();
        }
    }
}

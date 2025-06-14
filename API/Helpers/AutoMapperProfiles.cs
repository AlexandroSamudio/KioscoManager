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

            CreateMap<Venta, VentaDto>()
                .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario!.UserName))
                .ForMember(dest => dest.CantidadProductos, opt => opt.MapFrom(src => src.Detalles.Sum(d => d.Cantidad)));
        }
    }
}

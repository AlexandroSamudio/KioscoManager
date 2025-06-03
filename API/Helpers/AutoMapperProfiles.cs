using AutoMapper;
using API.DTOs;
using API.Entities;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Producto, ProductoDto>()
                .ForMember(dest => dest.CategoriaNombre,
                           opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : null));

            CreateMap<RegisterDto, AppUser>();
        }
    }
}

using AutoMapper;

public class AuthMapperProfile : Profile
{
    public AuthMapperProfile()
    {
        CreateMap<RegisterRequest, ApplicationUser>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.Email));

        CreateMap<ApplicationUser, AuthResponse>()
           .ForMember(dest => dest.Email,
               opt => opt.MapFrom(src => src.Email))
           .ForMember(dest => dest.Token,
               opt => opt.Ignore())
           .ForMember(dest => dest.RefreshToken,
               opt => opt.Ignore())
           .ForMember(dest => dest.Roles,
               opt => opt.Ignore());
    }
}

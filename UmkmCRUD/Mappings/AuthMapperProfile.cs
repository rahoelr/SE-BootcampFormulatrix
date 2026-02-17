using AutoMapper;

public class AuthMapperProfile : Profile
{
    public AuthMapperProfile()
    {
        CreateMap<RegisterRequest, ApplicationUser>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.Email));
    }
}

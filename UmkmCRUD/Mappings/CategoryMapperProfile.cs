using AutoMapper;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryResponse>();

        CreateMap<CategoryRequest, Category>();
    }
}

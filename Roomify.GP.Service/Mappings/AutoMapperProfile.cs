using AutoMapper;
using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.DTOs.PortfolioPost;
using Roomify.GP.Core.DTOs.User;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Roomify.GP.Service.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ApplicationUser, UserResponseDto>().ReverseMap();
            CreateMap<UserCreateDto, ApplicationUser>();
            CreateMap<UserUpdateDto, ApplicationUser>();
            CreateMap<ApplicationUser, UserWithRolesDto>()
           .ForMember(dest => dest.Role, opt => opt.Ignore())
           .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed));

            CreateMap<PortfolioPostDto, PortfolioPost>()
            .ForMember(dest => dest.ImagePath, options => options.Ignore())
            .ForMember(dest => dest.Id, options => options.Ignore())
            .ForMember(dest => dest.CreatedAt, options => options.Ignore())
            .ForMember(dest => dest.ApplicationUser, options => options.Ignore());

            CreateMap<PortfolioPost, PortfolioPostResponseDto>();

        }
    }
}

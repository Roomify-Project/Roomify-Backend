using AutoMapper;
using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.DTOs.Comment;
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

            CreateMap<PortfolioPost, PortfolioPostResponseDto>()
                .ForMember(dest => dest.OwnerUserName, opt => opt.MapFrom(src => src.ApplicationUser.UserName))
                .ForMember(dest => dest.OwnerProfilePicture, opt => opt.MapFrom(src => src.ApplicationUser.ProfilePicture));

            // Comment mappings
            CreateMap<CommentCreateDto, Comment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationUser, opt => opt.Ignore())
                .ForMember(dest => dest.PortfolioPost, opt => opt.Ignore());

            CreateMap<Comment, CommentResponseDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.ApplicationUserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.ApplicationUser.UserName))
                .ForMember(dest => dest.UserProfilePicture, opt => opt.MapFrom(src => src.ApplicationUser.ProfilePicture));
        }
    }
}

using AutoMapper;
using Roomify.GP.Core.DTOs.AI;
using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.DTOs.Comment;
using Roomify.GP.Core.DTOs.Like;
using Roomify.GP.Core.DTOs.PortfolioPost;
using Roomify.GP.Core.DTOs.User;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.AI;
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
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed));

            CreateMap<PortfolioPostDto, PortfolioPost>();
            CreateMap<PortfolioPost, PortfolioPostResponseDto>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(src => src.ApplicationUserId))
                .ForMember(d => d.UserName, opt => opt.MapFrom(src => src.ApplicationUser.UserName))
                .ForMember(d => d.UserProfilePicture, opt => opt.MapFrom(src => src.ApplicationUser.ProfilePicture))
                .ForMember(d => d.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(d => d.LikesCount, opt => opt.MapFrom(src => src.Likes.Count));

            CreateMap<SavedDesign, SavedDesignResponseDto>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(src => src.ApplicationUserId))
                .ForMember(d => d.UserName, opt => opt.MapFrom(src => src.ApplicationUser.FullName))
                .ForMember(d => d.UserProfilePicture, opt => opt.MapFrom(src => src.ApplicationUser.ProfilePicture))
                .ForMember(d => d.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(d => d.LikesCount, opt => opt.MapFrom(src => src.Likes.Count));

            CreateMap<CommentCreateDto, Comment>().ForMember(dest => dest.ApplicationUserId,opt => opt.MapFrom(src => src.UserId)); ;
            CreateMap<Comment, CommentResponseDto>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(src => src.ApplicationUserId))
                .ForMember(d => d.UserName, opt => opt.MapFrom(src => src.ApplicationUser.UserName))
                .ForMember(d => d.UserProfilePicture, opt => opt.MapFrom(src => src.ApplicationUser.ProfilePicture));

            CreateMap<Like, LikeDto>();
        }
    }
}

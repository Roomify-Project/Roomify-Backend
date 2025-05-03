using Microsoft.AspNetCore.Http;
using Roomify.GP.Core.DTOs.ApplicationUser;


public class UserUpdateDto
{
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string Bio { get; set; }
    public string Email { get; set; }
    //public string ProfilePicture { get; set; }
    public IFormFile? ProfileImage { get; set; }
}

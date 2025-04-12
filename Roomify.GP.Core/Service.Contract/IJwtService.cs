using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.Identity;


namespace Roomify.GP.Core.Service.Contract
{
    public interface IJwtService
    {
       Task<string>  GenerateToken(ApplicationUser user);
    }
}

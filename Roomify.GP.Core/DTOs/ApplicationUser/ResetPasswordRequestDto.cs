using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.ApplicationUser
{
    public class ResetPasswordRequestDto
    {
        public string Email { get; set; }
        public string OtpCode { get; set; } 
        public string NewPassword { get; set; }
    }
}

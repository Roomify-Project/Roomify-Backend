using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities.Identity
{
    public enum Roles
    {
        [EnumMember(Value = "NormalUser")]
        NormalUser,

        [EnumMember(Value = "InteriorDesigner")]
        InteriorDesigner
    }
}
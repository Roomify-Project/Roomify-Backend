using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface INotificationSender
    {
        Task NotifyUserAsync(Guid userId, string message, object data);
    }
}

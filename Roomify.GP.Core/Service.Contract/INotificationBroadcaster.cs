using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface INotificationBroadcaster
    {
        Task BroadcastNotificationAsync(Guid recipientUserId, string type, string message, Guid? relatedEntityId = null);
    }
}

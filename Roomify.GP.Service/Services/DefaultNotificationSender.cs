using Roomify.GP.Core.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{
    public class DefaultNotificationSender : INotificationSender
    {
        public Task NotifyUserAsync(Guid userId, string message, object data)
        {
            // تنفيذ فارغ مؤقت - سيتم استبداله لاحقًا بتنفيذ حقيقي
            return Task.CompletedTask;
        }
    }
}

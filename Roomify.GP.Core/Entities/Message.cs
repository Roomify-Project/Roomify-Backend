using Roomify.GP.Core.Entities.Identity;

public class Message
{
    public Guid Id { get; set; }          // معرف الرسالة
    public Guid SenderId { get; set; } // ID المرسل (تأكد من أنه من نوع string)
    public Guid ReceiverId { get; set; } // ID المستقبل (تأكد من أنه من نوع string)
    public string Content { get; set; }   // محتوى الرسالة
    public DateTime SentAt { get; set; }  // وقت إرسال الرسالة
    public bool IsDeleted { get; set; } = false;


}

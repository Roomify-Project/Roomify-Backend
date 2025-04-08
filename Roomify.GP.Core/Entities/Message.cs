using Roomify.GP.Core.Entities.Identity;

public class Message
{
    public int Id { get; set; }          // معرف الرسالة
    public string SenderId { get; set; } // ID المرسل (تأكد من أنه من نوع string)
    public string ReceiverId { get; set; } // ID المستقبل (تأكد من أنه من نوع string)
    public string Content { get; set; }   // محتوى الرسالة
    public DateTime SentAt { get; set; }  // وقت إرسال الرسالة

  
   
}

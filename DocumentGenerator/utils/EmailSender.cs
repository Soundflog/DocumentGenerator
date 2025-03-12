using System.IO;
using System.Net;
using System.Net.Mail;
using DocumentGenerator.Models;

namespace DocumentGenerator.utils;

// Класс для отправки email с вложением
public static class EmailSender
{
    public static bool SendEmail(string recipient, string subject, string body, byte[] attachmentContent,
        string fileExtension)
    {
        try
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient("smtp.example.com");
            mail.From = new MailAddress("your_email@example.com");
            mail.To.Add(recipient);
            mail.Subject = subject;
            mail.Body = body;

            using var stream = new MemoryStream(attachmentContent);
            var fileName = "document" + fileExtension;
            var attachment = new Attachment(stream, fileName);
            mail.Attachments.Add(attachment);

            smtpServer.Port = 587;
            smtpServer.Credentials = new NetworkCredential("your_email@example.com", "your_password");
            smtpServer.EnableSsl = true;

            smtpServer.Send(mail);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Log("Ошибка отправки email", ex.Message);
            return false;
        }
    }
}
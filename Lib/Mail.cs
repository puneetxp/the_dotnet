using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace The.DotNet.Lib
{
    public class Mail
    {
        public string Subject { get; set; } = "Birthday Reminders for August";
        public string Message { get; set; }
        public List<string> To { get; set; } = new List<string>();
        public List<string> Cc { get; set; } = new List<string>();
        public List<string> Bcc { get; set; } = new List<string>();

        public Mail SetSubject(string subject)
        {
            this.Subject = subject;
            return this;
        }

        public Mail SetMessage(string message)
        {
            this.Message = message;
            return this;
        }

        public Mail AddTo(string email)
        {
            this.To.Add(email);
            return this;
        }

        public void Send()
        {
            var from = Environment.GetEnvironmentVariable("host") != null ? "sender@" + Environment.GetEnvironmentVariable("host") : "sender@example.com";
            
            using (var smtp = new SmtpClient())
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.Subject = this.Subject;
                mail.Body = this.Message;
                mail.IsBodyHtml = true;

                foreach (var email in To) mail.To.Add(email);
                foreach (var email in Cc) mail.CC.Add(email);
                foreach (var email in Bcc) mail.Bcc.Add(email);

                // Configure SMTP based on env vars
                // smtp.Host = ... 
                // smtp.Send(mail);
                // Placeholder
                Console.WriteLine("Mock Email Sent: " + this.Subject);
            }
        }
    }
}

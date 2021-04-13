using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace RegistryInvoiceGenerator
{
    class SmtpSender
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _from;

        public SmtpSender(string host, int port, string from)
        {
            _host = host;
            _port = port;
            _from = from;
        }

        public bool SendMail(string to, string subject, string body, string attachmentFileName)
        {
            try
            {
                using (var smtp = new SmtpClient(_host, _port))
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_from),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    foreach (var address in to.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mailMessage.To.Add(new MailAddress(address));
                    }

                    System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType
                    {
                        MediaType = System.Net.Mime.MediaTypeNames.Application.Octet,
                        Name = "Счет-извещение.pdf"
                    };
                    mailMessage.Attachments.Add(new Attachment(attachmentFileName, contentType));
                    smtp.Send(mailMessage);
                    mailMessage.Attachments.Dispose();
                    smtp.Dispose();
                }
                return true;
            } catch
            {
                return false;
            }
        }
    }
}

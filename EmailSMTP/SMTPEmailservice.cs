using DocumentFormat.OpenXml.Wordprocessing;
using System.Net;
using System.Net.Mail;

namespace AutoMailer.EmailSMTP
{
    public class SMTPEmailservice
    {
        public async Task<string> SendEmailAsyncForRanjan(List<string> emailList)
        {

         string fromEmail = "TestEmail@gmail.com";
         string appPassword = "test key of mygmail";
         string fromName = "Test User";

        var results = new List<string>();

            try
            {

                if (emailList == null || emailList.Count == 0)
                    return "⚠️ No recipient email addresses found.";

                string subject = "Application for Java Developer Position – Rakesh Ranjan";


                //  use Chat gpt to change that html Content
                string htmlContent = @"
                            <p>Dear Hiring Manager,</p>

                            <p>I hope you're doing well.</p>

                            <p>
                            I am a <b>Java Developer</b> with <b>5.5 years of experience</b> in
                            <b>Java, Spring Boot, Microservices, MySQL, PostgreSQL, and basic AWS</b>.
                            I'm currently exploring new opportunities and would like to know if there are any relevant openings in your team.
                            </p>

                            <p>
                            I've attached my resume for your consideration. I would appreciate the opportunity to connect.
                            </p>

                            <p>
                            Thank you for your time.
                            </p>

                            <p>
                            Best regards,<br/>
                            <b>Rakesh Ranjan</b><br/>
                            Contact No : 9155104987
                            </p>";

                string resumePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", "MyResume.pdf");

                if (!File.Exists(resumePath))
                    return $"❌ Resume file not found at path: {resumePath}";

                foreach (var email in emailList)
                {
                    string result = await SendEmailUsingGmailSMTP(email, subject, htmlContent, resumePath, fromEmail, appPassword, fromName);
                    results.Add(result);
                    await Task.Delay(1500); // small delay to avoid Gmail rate limit
                }

                return string.Join(Environment.NewLine, results);
            }
            catch (Exception ex)
            {
                return $"❌ Fatal error while sending emails: {ex.Message}" +
                       (ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "");
            }
        }


       


        private async Task<string> SendEmailUsingGmailSMTP(string toEmail, string subject, string htmlBody, string attachmentPath,string fromEmail, string appPassword, string fromName)
        {

            try
            {
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, fromName);
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = htmlBody;
                    mail.IsBodyHtml = true;

                    // ✅ Attach resume
                    if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                    {
                        mail.Attachments.Add(new Attachment(attachmentPath));
                    }
                    else
                    {
                        return $"⚠️ Attachment not found for {toEmail}: {attachmentPath}";
                    }

                    // ✅ Gmail SMTP setup
                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Timeout = 20000;

                        try
                        {
                            await smtp.SendMailAsync(mail);
                            return $"✅ Email successfully sent to: {toEmail}";
                        }
                        catch (SmtpException smtpEx)
                        {
                            return $"❌ SMTP error sending to {toEmail}: {smtpEx.Message} (Status: {smtpEx.StatusCode})" +
                                   (smtpEx.InnerException != null ? $" | Inner: {smtpEx.InnerException.Message}" : "");
                        }
                        catch (InvalidOperationException invEx)
                        {
                            return $"❌ Invalid operation while sending to {toEmail}: {invEx.Message}";
                        }
                        catch (Exception ex)
                        {
                            return $"❌ General error while sending to {toEmail}: {ex.Message}" +
                                   (ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "");
                        }
                    }
                }
            }
            catch (FormatException fex)
            {
                return $"❌ Invalid email format for recipient {toEmail}: {fex.Message}";
            }
            catch (Exception ex)
            {
                return $"❌ Unexpected error while preparing email for {toEmail}: {ex.Message}" +
                       (ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "");
            }
        }

    }
}

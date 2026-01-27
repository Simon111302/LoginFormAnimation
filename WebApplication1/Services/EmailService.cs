using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace WebApplication1.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task SendVerificationCodeAsync(string toEmail, string code);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                emailSettings["SenderName"],
                emailSettings["SenderEmail"]
            ));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Password Reset Request";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 40px; border-radius: 10px;'>
                            <h2 style='color: #333;'>Password Reset Request</h2>
                            <p style='color: #666; line-height: 1.6;'>
                                We received a request to reset your password. Click the button below to reset your password:
                            </p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' 
                                   style='background: linear-gradient(135deg, #00c6ff, #0072ff); 
                                          color: white; 
                                          padding: 15px 40px; 
                                          text-decoration: none; 
                                          border-radius: 25px; 
                                          font-weight: bold;
                                          display: inline-block;'>
                                    Reset Password
                                </a>
                            </div>
                            <p style='color: #666; font-size: 14px;'>
                                If you didn't request this, please ignore this email.
                            </p>
                            <p style='color: #666; font-size: 14px;'>
                                This link will expire in 1 hour.
                            </p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                            <p style='color: #999; font-size: 12px;'>
                                © 2026 Student Portal. All rights reserved.
                            </p>
                        </div>
                    </div>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

        public async Task SendVerificationCodeAsync(string toEmail, string code)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                emailSettings["SenderName"],
                emailSettings["SenderEmail"]
            ));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Password Reset Verification Code";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 40px; border-radius: 10px;'>
                            <h2 style='color: #333;'>Password Reset Verification Code</h2>
                            <p style='color: #666; line-height: 1.6;'>
                                Your password reset verification code is:
                            </p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                                            color: white;
                                            font-size: 36px;
                                            font-weight: bold;
                                            padding: 20px 40px;
                                            border-radius: 15px;
                                            letter-spacing: 8px;
                                            display: inline-block;'>
                                    {code}
                                </div>
                            </div>
                            <p style='color: #666; font-size: 14px; text-align: center;'>
                                Enter this code on the password reset page to continue.
                            </p>
                            <p style='color: #666; font-size: 14px; text-align: center;'>
                                If you didn't request this, please ignore this email.
                            </p>
                            <p style='color: #999; font-size: 14px; text-align: center;'>
                                This code will expire in 15 minutes.
                            </p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                            <p style='color: #999; font-size: 12px; text-align: center;'>
                                © 2026 Student Portal. All rights reserved.
                            </p>
                        </div>
                    </div>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            using var client = new SmtpClient();
            await client.ConnectAsync(
                emailSettings["SmtpServer"],
                int.Parse(emailSettings["SmtpPort"]!),
                SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                emailSettings["SmtpUsername"],
                emailSettings["SmtpPassword"]
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

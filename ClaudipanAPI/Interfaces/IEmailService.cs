using ClaudipanAPI.Models.Entities;

namespace ClaudipanAPI.Interfaces;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string temporaryPassword, string resetToken);
    Task<bool> SendPreRegisterEmailAsync(PreRegistro preregistro);
}

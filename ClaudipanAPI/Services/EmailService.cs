using System.Net;
using System.Net.Mail;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.Entities;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ClaudipanAPI.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string temporaryPassword, string resetToken)
    {
        try
        {
            var host = Environment.GetEnvironmentVariable("EmailSettings__Host") 
                ?? _configuration["EmailSettings:Host"] 
                ?? "smtp.gmail.com";

            var portStr = Environment.GetEnvironmentVariable("EmailSettings__Port") 
                ?? _configuration["EmailSettings:Port"] 
                ?? "587";
            int.TryParse(portStr, out var port);
            if (port == 0) port = 587;

            var fromAddress = Environment.GetEnvironmentVariable("EmailSettings__From") 
                ?? _configuration["EmailSettings:From"] 
                ?? "appvoto@gmail.com";

            var password = Environment.GetEnvironmentVariable("EmailSettings__Password") 
                ?? _configuration["EmailSettings:Password"] 
                ?? "oldfwfvxznqqvczw";

            var displayName = Environment.GetEnvironmentVariable("EmailSettings__DisplayName") 
                ?? _configuration["EmailSettings:DisplayName"] 
                ?? "Claudipan 2026";

            var frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") 
                ?? _configuration["FrontendUrl"] 
                ?? "https://claudipan.pedroleyvasenador26.org";

            var activationUrl = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";

            var mail = new MailMessage
            {
                From = new MailAddress(fromAddress, displayName),
                Subject = "🥖 Claudipan - Recuperación de Contraseña y Activación",
                IsBodyHtml = true
            };

            mail.To.Add(new MailAddress(toEmail, userName));

            mail.Body = $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Recuperación de Contraseña - Claudipan</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f7f3ed; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #2d241e;"">
  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f7f3ed; padding: 30px 10px;"">
    <tr>
      <td align=""center"">
        <!-- Main Card -->
        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 580px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 25px rgba(180, 83, 9, 0.1); border: 1px solid #fde68a;"">
          <!-- Header Banner -->
          <tr>
            <td style=""background: linear-gradient(135deg, #78350f 0%, #b45309 50%, #d97706 100%); padding: 36px 30px; text-align: center; color: #ffffff;"">
              <div style=""display: inline-block; background-color: rgba(255,255,255,0.15); padding: 10px 18px; border-radius: 9999px; margin-bottom: 12px; border: 1px solid rgba(255,255,255,0.25);"">
                <span style=""font-size: 13px; font-weight: 700; letter-spacing: 1.5px; text-transform: uppercase;"">🥐 Claudipan Artesanal 🥖</span>
              </div>
              <h1 style=""margin: 0; font-size: 26px; font-weight: 800; letter-spacing: -0.5px;"">Recuperación de Contraseña</h1>
              <p style=""margin: 8px 0 0; font-size: 14px; opacity: 0.9;"">Panadería & Pastelería SENA ADSO</p>
            </td>
          </tr>

          <!-- Body Content -->
          <tr>
            <td style=""padding: 36px 32px;"">
              <p style=""margin: 0 0 16px; font-size: 17px; font-weight: 700; color: #1c1917;"">
                Hola, <span style=""color: #b45309;"">{WebUtility.HtmlEncode(userName)}</span> 👋
              </p>
              <p style=""margin: 0 0 20px; font-size: 14px; line-height: 1.6; color: #57534e;"">
                Hemos recibido una solicitud para restablecer la contraseña de acceso a tu cuenta de Claudipan asociada al correo <strong>{WebUtility.HtmlEncode(toEmail)}</strong>.
              </p>

              <!-- Temporary Password Box -->
              <div style=""background: #fffbeb; border: 2px dashed #f59e0b; border-radius: 14px; padding: 20px; text-align: center; margin: 24px 0;"">
                <span style=""display: block; font-size: 11px; font-weight: 700; color: #92400e; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 6px;"">
                  Tu Nueva Contraseña Asignada
                </span>
                <span style=""display: inline-block; font-size: 28px; font-weight: 900; letter-spacing: 4px; color: #78350f; font-family: 'Consolas', 'Courier New', monospace; background: #ffffff; padding: 8px 24px; border-radius: 8px; border: 1px solid #fde68a;"">
                  {WebUtility.HtmlEncode(temporaryPassword)}
                </span>
                <span style=""display: block; font-size: 12px; color: #b45309; margin-top: 8px; font-style: italic;"">
                  Guarda esta contraseña temporal para iniciar sesión una vez activada.
                </span>
              </div>

              <!-- Instructions -->
              <p style=""margin: 0 0 24px; font-size: 14px; line-height: 1.6; color: #57534e;"">
                Para que esta nueva contraseña surta efecto y reemplace tu clave anterior, debes confirmarla presionando el siguiente botón de activación seguro:
              </p>

              <!-- CTA Button -->
              <div style=""text-align: center; margin: 30px 0;"">
                <a href=""{activationUrl}"" target=""_blank"" style=""display: inline-block; background: linear-gradient(135deg, #d97706 0%, #b45309 100%); color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 800; padding: 16px 36px; border-radius: 14px; box-shadow: 0 6px 16px rgba(180, 83, 9, 0.35); text-transform: uppercase; letter-spacing: 0.5px;"">
                  🚀 Activar mi nueva contraseña
                </a>
              </div>

              <!-- Security Notice -->
              <div style=""background-color: #f5f5f4; border-radius: 10px; padding: 14px 18px; margin-top: 24px;"">
                <p style=""margin: 0; font-size: 12px; color: #78716c; line-height: 1.5;"">
                  🔒 <strong>Seguridad:</strong> Este enlace de activación y token tienen una validez de <strong>24 horas</strong>. Si tú no realizaste esta solicitud, puedes ignorar este mensaje; tu contraseña actual continuará protegida y no cambiará hasta que se presione el botón de activación.
                </p>
              </div>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style=""background-color: #fafaf9; border-top: 1px solid #e7e5e4; padding: 20px 30px; text-align: center;"">
              <p style=""margin: 0; font-size: 12px; color: #a8a29e;"">
                © 2026 Panadería Claudipan. Proyecto Formativo SENA ADSO.<br>
                Este es un mensaje automático, por favor no responda directamente a este correo.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
";

            using var smtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(fromAddress, password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
            Log.Information("Password reset email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send password reset email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendPreRegisterEmailAsync(PreRegistro preregistro)
    {
        try
        {
            var host = Environment.GetEnvironmentVariable("EmailSettings__Host") 
                ?? _configuration["EmailSettings:Host"] 
                ?? "smtp.gmail.com";

            var portStr = Environment.GetEnvironmentVariable("EmailSettings__Port") 
                ?? _configuration["EmailSettings:Port"] 
                ?? "587";
            int.TryParse(portStr, out var port);
            if (port == 0) port = 587;

            var fromAddress = Environment.GetEnvironmentVariable("EmailSettings__From") 
                ?? _configuration["EmailSettings:From"] 
                ?? "appvoto@gmail.com";

            var password = Environment.GetEnvironmentVariable("EmailSettings__Password") 
                ?? _configuration["EmailSettings:Password"] 
                ?? "oldfwfvxznqqvczw";

            var displayName = Environment.GetEnvironmentVariable("EmailSettings__DisplayName") 
                ?? _configuration["EmailSettings:DisplayName"] 
                ?? "Claudipan 2026";

            var frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") 
                ?? _configuration["FrontendUrl"] 
                ?? "https://claudipan.pedroleyvasenador26.org";

            var confirmUrl = $"{frontendBaseUrl.TrimEnd('/')}/confirmar-registro?token={Uri.EscapeDataString(preregistro.TokenValidacion)}&email={Uri.EscapeDataString(preregistro.Email)}";
            var cancelUrl = $"{frontendBaseUrl.TrimEnd('/')}/cancelar-registro?token={Uri.EscapeDataString(preregistro.TokenCancelacion)}&email={Uri.EscapeDataString(preregistro.Email)}";

            var mail = new MailMessage
            {
                From = new MailAddress(fromAddress, displayName),
                Subject = "🥐 Claudipan - Valida o Cancela tu Registro de Usuario",
                IsBodyHtml = true
            };

            mail.To.Add(new MailAddress(preregistro.Email, preregistro.Nombre));

            var nombresDetalle = string.Join(" ", new[] { preregistro.PrimerNombre, preregistro.SegundoNombre, preregistro.PrimerApellido, preregistro.SegundoApellido }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (string.IsNullOrWhiteSpace(nombresDetalle)) nombresDetalle = preregistro.Nombre;

            mail.Body = $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Prerregistro de Usuario - Claudipan</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f7f3ed; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #2d241e;"">
  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f7f3ed; padding: 30px 10px;"">
    <tr>
      <td align=""center"">
        <!-- Main Card -->
        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 620px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 30px rgba(180, 83, 9, 0.12); border: 1px solid #fde68a;"">
          
          <!-- Header Banner -->
          <tr>
            <td style=""background: linear-gradient(135deg, #78350f 0%, #b45309 50%, #d97706 100%); padding: 36px 30px; text-align: center; color: #ffffff;"">
              <div style=""display: inline-block; background-color: rgba(255,255,255,0.18); padding: 8px 18px; border-radius: 9999px; margin-bottom: 12px; border: 1px solid rgba(255,255,255,0.3);"">
                <span style=""font-size: 13px; font-weight: 800; letter-spacing: 1.5px; text-transform: uppercase;"">🥖 Panadería & Pastelería Claudipan 🥐</span>
              </div>
              <h1 style=""margin: 0; font-size: 26px; font-weight: 800; letter-spacing: -0.5px;"">¡Confirma tu Registro de Usuario!</h1>
              <p style=""margin: 8px 0 0; font-size: 14px; opacity: 0.9;"">SENA ADSO • Pan Fresco y Calidad Artesanal</p>
            </td>
          </tr>

          <!-- Body Content -->
          <tr>
            <td style=""padding: 36px 32px;"">
              <p style=""margin: 0 0 16px; font-size: 18px; font-weight: 700; color: #1c1917;"">
                ¡Hola, <span style=""color: #b45309;"">{WebUtility.HtmlEncode(preregistro.Nombre)}</span>! 👋
              </p>
              <p style=""margin: 0 0 20px; font-size: 14px; line-height: 1.6; color: #57534e;"">
                Has completado el formulario de prerregistro en nuestra plataforma. A continuación encontrarás el resumen detallado de todos los datos que registraste, incluida tu contraseña de acceso:
              </p>

              <!-- Data Summary Table -->
              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #fffdfa; border: 1px solid #fde68a; border-radius: 12px; overflow: hidden; margin-bottom: 24px; font-size: 13px;"">
                <tr style=""border-bottom: 1px solid #fef3c7;"">
                  <td style=""padding: 10px 16px; font-weight: 700; color: #92400e; width: 38%; background-color: #fef3c7;"">Nombre Completo:</td>
                  <td style=""padding: 10px 16px; color: #1c1917; font-weight: 600;"">{WebUtility.HtmlEncode(preregistro.Nombre)}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #fef3c7;"">
                  <td style=""padding: 10px 16px; font-weight: 700; color: #92400e; background-color: #fef3c7;"">Nombres y Apellidos:</td>
                  <td style=""padding: 10px 16px; color: #44403c;"">{WebUtility.HtmlEncode(nombresDetalle)}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #fef3c7;"">
                  <td style=""padding: 10px 16px; font-weight: 700; color: #92400e; background-color: #fef3c7;"">Cédula / Documento:</td>
                  <td style=""padding: 10px 16px; color: #44403c;"">{(string.IsNullOrWhiteSpace(preregistro.Cedula) ? "No especificada" : WebUtility.HtmlEncode(preregistro.Cedula))}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #fef3c7;"">
                  <td style=""padding: 10px 16px; font-weight: 700; color: #92400e; background-color: #fef3c7;"">Correo Electrónico:</td>
                  <td style=""padding: 10px 16px; color: #44403c; font-weight: 600;"">{WebUtility.HtmlEncode(preregistro.Email)}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #fef3c7;"">
                  <td style=""padding: 10px 16px; font-weight: 700; color: #92400e; background-color: #fef3c7;"">Celular / Teléfono:</td>
                  <td style=""padding: 10px 16px; color: #44403c;"">{(string.IsNullOrWhiteSpace(preregistro.Telefono) ? "No especificado" : WebUtility.HtmlEncode(preregistro.Telefono))}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #fef3c7;"">
                  <td style=""padding: 10px 16px; font-weight: 700; color: #92400e; background-color: #fef3c7;"">Dirección:</td>
                  <td style=""padding: 10px 16px; color: #44403c;"">{(string.IsNullOrWhiteSpace(preregistro.Direccion) ? "No especificada" : WebUtility.HtmlEncode(preregistro.Direccion))}</td>
                </tr>
                <tr style=""border-bottom: 1px solid #fef3c7;"">
                  <td style=""padding: 10px 16px; font-weight: 700; color: #92400e; background-color: #fef3c7;"">Redes Sociales:</td>
                  <td style=""padding: 10px 16px; color: #44403c;"">{(string.IsNullOrWhiteSpace(preregistro.RedesSociales) ? "No especificadas" : WebUtility.HtmlEncode(preregistro.RedesSociales))}</td>
                </tr>
                <tr>
                  <td style=""padding: 10px 16px; font-weight: 700; color: #92400e; background-color: #fef3c7;"">Cupo de Crédito:</td>
                  <td style=""padding: 10px 16px; color: #15803d; font-weight: 700;"">$50.000 COP (Asignado)</td>
                </tr>
              </table>

              <!-- Highlighted Password Box -->
              <div style=""background: #fffbeb; border: 2px dashed #f59e0b; border-radius: 14px; padding: 20px; text-align: center; margin: 24px 0;"">
                <span style=""display: block; font-size: 11px; font-weight: 800; color: #92400e; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 6px;"">
                  🔑 Contraseña Creada por Ti
                </span>
                <span style=""display: inline-block; font-size: 24px; font-weight: 900; letter-spacing: 3px; color: #78350f; font-family: 'Consolas', 'Courier New', monospace; background: #ffffff; padding: 8px 24px; border-radius: 8px; border: 1px solid #fde68a;"">
                  {WebUtility.HtmlEncode(preregistro.PasswordPlana)}
                </span>
                <p style=""margin: 10px 0 0; font-size: 11px; color: #78350f; line-height: 1.4;"">
                  ⚠️ Por tu seguridad, guarda esta contraseña. Al confirmar tu cuenta, se transformará de manera segura e irreversible a hash MD5 en nuestra base de datos.
                </p>
              </div>

              <!-- Action Instructions -->
              <p style=""margin: 24px 0 16px; font-size: 14px; line-height: 1.5; color: #44403c; text-align: center; font-weight: 600;"">
                Para completar la activación o cancelar si no reconoces este registro, usa las siguientes opciones:
              </p>

              <!-- Buttons Container -->
              <div style=""text-align: center; margin: 24px 0 28px;"">
                <!-- Validation Button -->
                <div style=""margin-bottom: 14px;"">
                  <a href=""{confirmUrl}"" target=""_blank"" style=""display: inline-block; background: linear-gradient(135deg, #15803d 0%, #16a34a 100%); color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 800; padding: 15px 36px; border-radius: 12px; box-shadow: 0 4px 15px rgba(22, 163, 74, 0.35); letter-spacing: 0.3px;"">
                    🥖 Validar y Confirmar Registro
                  </a>
                </div>

                <!-- Cancel Button -->
                <div>
                  <a href=""{cancelUrl}"" target=""_blank"" style=""display: inline-block; background: #ffffff; color: #b91c1c; border: 1.5px solid #fca5a5; text-decoration: none; font-size: 13px; font-weight: 700; padding: 10px 24px; border-radius: 10px;"">
                    ❌ Cancelar Registro
                  </a>
                </div>
              </div>

              <!-- Notice Box -->
              <div style=""background: #f5f5f4; border-left: 4px solid #d97706; padding: 12px 16px; border-radius: 0 8px 8px 0; margin-top: 24px;"">
                <p style=""margin: 0; font-size: 12px; color: #57534e; line-height: 1.5;"">
                  ⏳ <strong>Vigencia:</strong> Este enlace estará disponible por las próximas <strong>48 horas</strong>. Si no confirmas tu registro en este plazo, tus datos temporales se eliminarán automáticamente.
                </p>
              </div>

            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style=""background-color: #faf6ed; padding: 24px 30px; text-align: center; border-top: 1px solid #fde68a;"">
              <p style=""margin: 0 0 6px; font-size: 12px; color: #78350f; font-weight: 700;"">
                Claudipan • El sabor tradicional en cada bocado
              </p>
              <p style=""margin: 0; font-size: 11px; color: #a8a29e;"">
                Este es un mensaje automático generado por la plataforma web Claudipan. Por favor no respondas a este correo.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>
";

            using var smtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(fromAddress, password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
            Log.Information("Pre-registration confirmation email sent successfully to {Email}", preregistro.Email);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send pre-registration email to {Email}", preregistro.Email);
            return false;
        }
    }
}

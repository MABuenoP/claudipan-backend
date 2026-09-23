using System.Net;
using System.Net.Mail;
using ClaudipanAPI.Interfaces;
using ClaudipanAPI.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ClaudipanAPI.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public EmailService(IConfiguration configuration, IHttpContextAccessor? httpContextAccessor = null)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Detecta dinámicamente la URL base del Frontend (localhost en desarrollo o dominio en producción)
    /// </summary>
    private string GetFrontendBaseUrl()
    {
        try
        {
            var req = _httpContextAccessor?.HttpContext?.Request;
            if (req != null)
            {
                var origin = req.Headers["Origin"].ToString();
                if (!string.IsNullOrWhiteSpace(origin))
                {
                    return origin.TrimEnd('/');
                }

                var referer = req.Headers["Referer"].ToString();
                if (!string.IsNullOrWhiteSpace(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refUri))
                {
                    return $"{refUri.Scheme}://{refUri.Authority}";
                }
            }
        }
        catch
        {
            // Fallback en caso de excepciones al leer contexto HTTP
        }

        var configured = Environment.GetEnvironmentVariable("FRONTEND_URL") 
            ?? _configuration["FrontendUrl"] 
            ?? "https://claudipan.pedroleyvasenador26.org";

        return configured.TrimEnd('/');
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

            var frontendBaseUrl = GetFrontendBaseUrl();
            var activationUrl = $"{frontendBaseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";

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
        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 580px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(120, 53, 15, 0.12); border: 1px solid #e7e5e4;"">
          
          <!-- Header Banner (Solid Dark Brown Background) -->
          <tr>
            <td align=""center"" bgcolor=""#78350f"" style=""background-color: #78350f; padding: 36px 24px; text-align: center;"">
              <div style=""display: inline-block; background-color: #92400e; padding: 8px 18px; border-radius: 9999px; margin-bottom: 12px; border: 1px solid #b45309;"">
                <span style=""font-size: 13px; font-weight: 800; letter-spacing: 1.5px; text-transform: uppercase; color: #fef3c7;"">🥐 Claudipan Artesanal 🥖</span>
              </div>
              <h1 style=""margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.5px; color: #ffffff !important;"">Recuperación de Contraseña</h1>
              <p style=""margin: 8px 0 0; font-size: 14px; color: #fde68a !important; font-weight: 500;"">Panadería & Pastelería SENA ADSO</p>
            </td>
          </tr>

          <!-- Body Content -->
          <tr>
            <td style=""padding: 32px 28px; background-color: #ffffff;"">
              <p style=""margin: 0 0 16px; font-size: 17px; font-weight: 700; color: #1c1917;"">
                Hola, <span style=""color: #b45309;"">{WebUtility.HtmlEncode(userName)}</span> 👋
              </p>
              <p style=""margin: 0 0 20px; font-size: 14px; line-height: 1.6; color: #44403c;"">
                Hemos recibido una solicitud para restablecer la contraseña de acceso a tu cuenta de Claudipan asociada al correo <strong>{WebUtility.HtmlEncode(toEmail)}</strong>.
              </p>

              <!-- Temporary Password Box -->
              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #fffbeb; border: 2px dashed #d97706; border-radius: 12px; margin: 20px 0;"">
                <tr>
                  <td align=""center"" style=""padding: 20px 16px;"">
                    <span style=""display: block; font-size: 11px; font-weight: 800; color: #92400e; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 8px;"">
                      Tu Nueva Contraseña Asignada
                    </span>
                    <div style=""display: inline-block; background-color: #451a03; padding: 10px 24px; border-radius: 8px; border: 2px solid #b45309;"">
                      <span style=""font-size: 24px; font-weight: 900; letter-spacing: 3px; color: #ffffff !important; font-family: 'Consolas', 'Courier New', monospace;"">
                        {WebUtility.HtmlEncode(temporaryPassword)}
                      </span>
                    </div>
                    <span style=""display: block; font-size: 12px; color: #78350f; margin-top: 10px; font-weight: 600;"">
                      Guarda esta contraseña temporal para iniciar sesión una vez activada.
                    </span>
                  </td>
                </tr>
              </table>

              <!-- Instructions -->
              <p style=""margin: 0 0 24px; font-size: 14px; line-height: 1.6; color: #44403c;"">
                Para que esta nueva contraseña surta efecto y reemplace tu clave anterior, debes confirmarla presionando el siguiente botón:
              </p>

              <!-- Bulletproof Button -->
              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" align=""center"" style=""margin: 0 auto; border-collapse: separate;"">
                <tr>
                  <td align=""center"" bgcolor=""#b45309"" style=""background-color: #b45309; border-radius: 12px; padding: 0;"">
                    <a href=""{activationUrl}"" target=""_blank"" style=""display: block; background-color: #b45309; color: #ffffff !important; font-size: 16px; font-weight: 800; font-family: 'Segoe UI', Arial, sans-serif; text-decoration: none; padding: 16px 36px; border-radius: 12px; border: 1px solid #d97706; letter-spacing: 0.5px;"">
                      <span style=""color: #ffffff !important; text-decoration: none;"">🥖 Activar Nueva Contraseña</span>
                    </a>
                  </td>
                </tr>
              </table>

              <!-- Direct fallback link -->
              <p style=""margin: 14px 0 0; font-size: 12px; color: #78716c; text-align: center; line-height: 1.4;"">
                Si el botón no funciona, copia y pega este enlace en tu navegador:<br/>
                <a href=""{activationUrl}"" target=""_blank"" style=""color: #b45309; font-weight: 700; text-decoration: underline; word-break: break-all;"">
                  {activationUrl}
                </a>
              </p>

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

            var frontendBaseUrl = GetFrontendBaseUrl();
            var confirmUrl = $"{frontendBaseUrl}/confirmar-registro?token={Uri.EscapeDataString(preregistro.TokenValidacion)}&email={Uri.EscapeDataString(preregistro.Email)}";
            var cancelUrl = $"{frontendBaseUrl}/cancelar-registro?token={Uri.EscapeDataString(preregistro.TokenCancelacion)}&email={Uri.EscapeDataString(preregistro.Email)}";

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
        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 620px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 30px rgba(120, 53, 15, 0.12); border: 1px solid #e7e5e4;"">
          
          <!-- Header Banner (Solid Dark Brown #78350f - No washed out colors) -->
          <tr>
            <td align=""center"" bgcolor=""#78350f"" style=""background-color: #78350f; padding: 36px 24px; text-align: center;"">
              <div style=""display: inline-block; background-color: #92400e; padding: 8px 20px; border-radius: 9999px; margin-bottom: 12px; border: 1px solid #b45309;"">
                <span style=""font-size: 13px; font-weight: 800; letter-spacing: 1.5px; text-transform: uppercase; color: #fef3c7;"">🥖 Panadería & Pastelería Claudipan 🥐</span>
              </div>
              <h1 style=""margin: 0; font-size: 26px; font-weight: 800; letter-spacing: -0.5px; color: #ffffff !important;"">¡Confirma tu Registro de Usuario!</h1>
              <p style=""margin: 8px 0 0; font-size: 14px; color: #fde68a !important; font-weight: 500;"">SENA ADSO • Pan Fresco y Calidad Artesanal</p>
            </td>
          </tr>

          <!-- Body Content -->
          <tr>
            <td style=""padding: 32px 28px; background-color: #ffffff;"">
              <p style=""margin: 0 0 16px; font-size: 18px; font-weight: 700; color: #1c1917;"">
                ¡Hola, <span style=""color: #b45309;"">{WebUtility.HtmlEncode(preregistro.Nombre)}</span>! 👋
              </p>
              <p style=""margin: 0 0 20px; font-size: 14px; line-height: 1.6; color: #44403c;"">
                Has completado el formulario de prerregistro en nuestra plataforma. A continuación encontrarás el resumen detallado de todos los datos que registraste, incluida tu contraseña de acceso:
              </p>

              <!-- Data Summary Table with High Contrast Colors -->
              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #ffffff; border: 1px solid #fde68a; border-radius: 10px; overflow: hidden; margin-bottom: 24px; font-size: 13px;"">
                <tr>
                  <td bgcolor=""#fef3c7"" style=""padding: 10px 16px; font-weight: 700; color: #78350f; width: 38%; background-color: #fef3c7; border-bottom: 1px solid #fde68a;"">Nombre Completo:</td>
                  <td bgcolor=""#ffffff"" style=""padding: 10px 16px; color: #1c1917; font-weight: 600; background-color: #ffffff; border-bottom: 1px solid #fde68a;"">{WebUtility.HtmlEncode(preregistro.Nombre)}</td>
                </tr>
                <tr>
                  <td bgcolor=""#fef3c7"" style=""padding: 10px 16px; font-weight: 700; color: #78350f; background-color: #fef3c7; border-bottom: 1px solid #fde68a;"">Nombres y Apellidos:</td>
                  <td bgcolor=""#ffffff"" style=""padding: 10px 16px; color: #44403c; background-color: #ffffff; border-bottom: 1px solid #fde68a;"">{WebUtility.HtmlEncode(nombresDetalle)}</td>
                </tr>
                <tr>
                  <td bgcolor=""#fef3c7"" style=""padding: 10px 16px; font-weight: 700; color: #78350f; background-color: #fef3c7; border-bottom: 1px solid #fde68a;"">Cédula / Documento:</td>
                  <td bgcolor=""#ffffff"" style=""padding: 10px 16px; color: #44403c; background-color: #ffffff; border-bottom: 1px solid #fde68a;"">{(string.IsNullOrWhiteSpace(preregistro.Cedula) ? "No especificada" : WebUtility.HtmlEncode(preregistro.Cedula))}</td>
                </tr>
                <tr>
                  <td bgcolor=""#fef3c7"" style=""padding: 10px 16px; font-weight: 700; color: #78350f; background-color: #fef3c7; border-bottom: 1px solid #fde68a;"">Correo Electrónico:</td>
                  <td bgcolor=""#ffffff"" style=""padding: 10px 16px; color: #1c1917; font-weight: 700; background-color: #ffffff; border-bottom: 1px solid #fde68a;"">{WebUtility.HtmlEncode(preregistro.Email)}</td>
                </tr>
                <tr>
                  <td bgcolor=""#fef3c7"" style=""padding: 10px 16px; font-weight: 700; color: #78350f; background-color: #fef3c7; border-bottom: 1px solid #fde68a;"">Celular / Teléfono:</td>
                  <td bgcolor=""#ffffff"" style=""padding: 10px 16px; color: #44403c; background-color: #ffffff; border-bottom: 1px solid #fde68a;"">{(string.IsNullOrWhiteSpace(preregistro.Telefono) ? "No especificado" : WebUtility.HtmlEncode(preregistro.Telefono))}</td>
                </tr>
                <tr>
                  <td bgcolor=""#fef3c7"" style=""padding: 10px 16px; font-weight: 700; color: #78350f; background-color: #fef3c7; border-bottom: 1px solid #fde68a;"">Dirección:</td>
                  <td bgcolor=""#ffffff"" style=""padding: 10px 16px; color: #44403c; background-color: #ffffff; border-bottom: 1px solid #fde68a;"">{(string.IsNullOrWhiteSpace(preregistro.Direccion) ? "No especificada" : WebUtility.HtmlEncode(preregistro.Direccion))}</td>
                </tr>
                <tr>
                  <td bgcolor=""#fef3c7"" style=""padding: 10px 16px; font-weight: 700; color: #78350f; background-color: #fef3c7; border-bottom: 1px solid #fde68a;"">Redes Sociales:</td>
                  <td bgcolor=""#ffffff"" style=""padding: 10px 16px; color: #44403c; background-color: #ffffff; border-bottom: 1px solid #fde68a;"">{(string.IsNullOrWhiteSpace(preregistro.RedesSociales) ? "No especificadas" : WebUtility.HtmlEncode(preregistro.RedesSociales))}</td>
                </tr>
                <tr>
                  <td bgcolor=""#fef3c7"" style=""padding: 10px 16px; font-weight: 700; color: #78350f; background-color: #fef3c7;"">Cupo de Crédito:</td>
                  <td bgcolor=""#ffffff"" style=""padding: 10px 16px; color: #15803d; font-weight: 800; background-color: #ffffff;"">${NumberToCurrency(preregistro.LimiteCredito)} COP (Asignado)</td>
                </tr>
              </table>

              <!-- High Contrast Password Box -->
              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #fffbeb; border: 2px dashed #d97706; border-radius: 12px; margin: 24px 0;"">
                <tr>
                  <td align=""center"" style=""padding: 20px 16px;"">
                    <span style=""display: block; font-size: 11px; font-weight: 800; color: #92400e; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 8px;"">
                      🔑 Contraseña Registrada por Ti
                    </span>
                    <div style=""display: inline-block; background-color: #451a03; padding: 10px 24px; border-radius: 8px; border: 2px solid #b45309;"">
                      <span style=""font-size: 22px; font-weight: 900; letter-spacing: 3px; color: #ffffff !important; font-family: 'Consolas', 'Courier New', monospace;"">
                        {WebUtility.HtmlEncode(preregistro.PasswordPlana)}
                      </span>
                    </div>
                    <p style=""margin: 10px 0 0; font-size: 11px; color: #78350f; font-weight: 600; line-height: 1.4;"">
                      ⚠️ Guarda esta clave. Al confirmar tu cuenta, se transformará de manera segura e irreversible a hash MD5 en nuestra base de datos.
                    </p>
                  </td>
                </tr>
              </table>

              <!-- Action Instructions -->
              <p style=""margin: 24px 0 16px; font-size: 15px; line-height: 1.5; color: #1c1917; text-align: center; font-weight: 700;"">
                Para completar la activación o cancelar el registro, selecciona una de las siguientes opciones:
              </p>

              <!-- Bulletproof Validation Button (Solid Green #15803d) -->
              <div style=""text-align: center; margin: 20px 0 10px;"">
                <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" align=""center"" style=""margin: 0 auto; border-collapse: separate;"">
                  <tr>
                    <td align=""center"" bgcolor=""#15803d"" style=""background-color: #15803d; border-radius: 12px; padding: 0;"">
                      <a href=""{confirmUrl}"" target=""_blank"" style=""display: block; background-color: #15803d; color: #ffffff !important; font-size: 16px; font-weight: 800; font-family: 'Segoe UI', Arial, sans-serif; text-decoration: none; padding: 16px 36px; border-radius: 12px; border: 1px solid #16a34a; letter-spacing: 0.5px;"">
                        <span style=""color: #ffffff !important; text-decoration: none;"">🥖 Validar y Confirmar Registro</span>
                      </a>
                    </td>
                  </tr>
                </table>
              </div>

              <!-- Direct link fallback for validation -->
              <p style=""margin: 8px 0 24px; font-size: 12px; color: #57534e; text-align: center; line-height: 1.4;"">
                Si el botón no abre la página, haz clic directamente en este enlace:<br/>
                <a href=""{confirmUrl}"" target=""_blank"" style=""color: #15803d; font-weight: 700; text-decoration: underline; word-break: break-all;"">
                  {confirmUrl}
                </a>
              </p>

              <!-- Divider -->
              <div style=""border-top: 1px solid #e7e5e4; margin: 20px 0;""></div>

              <!-- Bulletproof Cancel Button (Soft Red Background + Bold Red Border & Text) -->
              <div style=""text-align: center; margin: 16px 0 10px;"">
                <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" align=""center"" style=""margin: 0 auto; border-collapse: separate;"">
                  <tr>
                    <td align=""center"" bgcolor=""#fee2e2"" style=""background-color: #fee2e2; border-radius: 10px; border: 2px solid #ef4444; padding: 0;"">
                      <a href=""{cancelUrl}"" target=""_blank"" style=""display: block; background-color: #fee2e2; color: #b91c1c !important; font-size: 14px; font-weight: 800; font-family: 'Segoe UI', Arial, sans-serif; text-decoration: none; padding: 12px 28px; border-radius: 10px;"">
                        <span style=""color: #b91c1c !important; text-decoration: none;"">❌ Cancelar Registro</span>
                      </a>
                    </td>
                  </tr>
                </table>
              </div>

              <!-- Direct link fallback for cancel -->
              <p style=""margin: 6px 0 20px; font-size: 11px; color: #78716c; text-align: center; line-height: 1.4;"">
                Para anular la solicitud: <a href=""{cancelUrl}"" target=""_blank"" style=""color: #b91c1c; font-weight: 600; text-decoration: underline; word-break: break-all;"">{cancelUrl}</a>
              </p>

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

    private static string NumberToCurrency(decimal amount)
    {
        return amount.ToString("N0", new System.Globalization.CultureInfo("es-CO"));
    }
}

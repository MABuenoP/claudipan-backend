namespace ClaudipanAPI.Models.DTOs;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestDto
{
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RedesSociales { get; set; }
    public decimal? LimiteCredito { get; set; }
    public string? FotoBase64 { get; set; }
}

public class AuthResponseDto
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Cedula { get; set; }
    public string Rol { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RedesSociales { get; set; }
    public decimal LimiteCredito { get; set; }
    public decimal DeudaActual { get; set; }
    public string? FotoBase64 { get; set; }
}

public class RefreshTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Cedula { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RedesSociales { get; set; }
    public decimal LimiteCredito { get; set; }
    public decimal DeudaActual { get; set; }
    public string? FotoBase64 { get; set; }
}

public class UpdateProfileDto
{
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Nombre { get; set; }
    public string? Cedula { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RedesSociales { get; set; }
    public string? FotoBase64 { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UsuarioAdminDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Cedula { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RedesSociales { get; set; }
    public decimal LimiteCredito { get; set; }
    public decimal DeudaActual { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? FotoBase64 { get; set; }
}

public class UpdateUsuarioAdminDto
{
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Nombre { get; set; }
    public string? Cedula { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RedesSociales { get; set; }
    public decimal? LimiteCredito { get; set; }
    public bool Activo { get; set; } = true;
    public string? Password { get; set; }
    public string? FotoBase64 { get; set; }
}

public class CheckFieldDto
{
    public string Field { get; set; } = string.Empty; // "email" | "cedula" | "telefono"
    public string Value { get; set; } = string.Empty;
}

public class CheckFieldResponseDto
{
    public bool Exists { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ConfirmPreRegisterDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CancelPreRegisterDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class PreRegisterResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

public class PreRegistroDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Cedula { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordPlana { get; set; } = string.Empty;
    public string Rol { get; set; } = "Cliente";
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RedesSociales { get; set; }
    public decimal LimiteCredito { get; set; }
    public string? FotoBase64 { get; set; }
    public string TokenValidacion { get; set; } = string.Empty;
    public string TokenCancelacion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class UpdatePreRegistroDto
{
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Nombre { get; set; }
    public string? Cedula { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordPlana { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RedesSociales { get; set; }
    public decimal? LimiteCredito { get; set; }
    public string? FotoBase64 { get; set; }
}
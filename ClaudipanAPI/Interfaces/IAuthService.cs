using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Responses;

namespace ClaudipanAPI.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId);
    Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileDto request);
    Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto request);
    Task<ApiResponse<List<UsuarioAdminDto>>> GetAllUsersAsync();
    Task<ApiResponse<UsuarioAdminDto>> CreateUserAdminAsync(UpdateUsuarioAdminDto request);
    Task<ApiResponse<UsuarioAdminDto>> UpdateUserAdminAsync(int id, UpdateUsuarioAdminDto request);
    Task<ApiResponse<bool>> DeleteUserAdminAsync(int id);
}
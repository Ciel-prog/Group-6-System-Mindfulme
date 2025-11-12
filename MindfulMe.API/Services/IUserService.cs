using MindfulMe.API.DTOs;
using MindfulMe.API.Models;

namespace MindfulMe.API.Services
{
    public interface IUserService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<User?> GetUserByIdAsync(int id);
    }
}
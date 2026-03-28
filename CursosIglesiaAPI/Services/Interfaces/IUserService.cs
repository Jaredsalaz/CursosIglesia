using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;

namespace CursosIglesia.Services.Interfaces;

public interface IUserService
{
    Task<UserProfile> GetProfileAsync();
    Task<ProfileResponse> UpdateProfileAsync(UpdateProfileRequest request);
    Task<ProfileResponse> UpdatePasswordAsync(ChangePasswordRequest request);
    Task<List<PaymentMethod>> GetPaymentMethodsAsync();
    Task AddPaymentMethodAsync(PaymentMethod method);
    Task RemovePaymentMethodAsync(Guid methodId);
    Task UpdateNotificationsAsync(NotificationPreferences preferences);
}

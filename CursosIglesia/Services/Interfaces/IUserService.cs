using CursosIglesia.Models;

namespace CursosIglesia.Services.Interfaces;

public interface IUserService
{
    Task<UserProfile> GetProfileAsync();
    Task UpdateProfileAsync(UserProfile profile);
    Task UpdatePasswordAsync(string currentPassword, string newPassword);
    Task<List<PaymentMethod>> GetPaymentMethodsAsync();
    Task AddPaymentMethodAsync(PaymentMethod method);
    Task RemovePaymentMethodAsync(int methodId);
    Task UpdateNotificationsAsync(NotificationPreferences preferences);
}

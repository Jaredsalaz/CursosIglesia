using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;

namespace CursosIglesia.Services.Implementations;

public class UserService : IUserService
{
    private static UserProfile _profile = new()
    {
        FirstName = "Gerardo",
        LastName = "Lopez",
        Email = "gerardo.lopez@email.com",
        Phone = "+52 (55) 9876-5432",
        AvatarUrl = "https://ui-avatars.com/api/?name=Gerardo+Lopez&background=7b2d26&color=fff&size=150&font-size=0.4",
        Bio = "Católico comprometido con la formación continua en la fe. Catequista parroquial desde 2018.",
        BirthDate = new DateTime(1990, 3, 15),
        JoinedDate = new DateTime(2025, 6, 1),
        Parish = "Parroquia San José",
        City = "Ciudad de México",
        Country = "México",
        PaymentMethods = new List<PaymentMethod>
        {
            new() { Id = 1, Type = "visa", LastFourDigits = "4532", ExpiryDate = "12/27", IsDefault = true },
            new() { Id = 2, Type = "mastercard", LastFourDigits = "8901", ExpiryDate = "06/28", IsDefault = false }
        },
        Notifications = new NotificationPreferences
        {
            EmailNotifications = true,
            CourseUpdates = true,
            PromotionalEmails = false,
            NewCourseAlerts = true
        }
    };

    public Task<UserProfile> GetProfileAsync()
        => Task.FromResult(_profile);

    public Task UpdateProfileAsync(UserProfile profile)
    {
        _profile = profile;
        return Task.CompletedTask;
    }

    public Task UpdatePasswordAsync(string currentPassword, string newPassword)
    {
        // Mock: just return success
        return Task.CompletedTask;
    }

    public Task<List<PaymentMethod>> GetPaymentMethodsAsync()
        => Task.FromResult(_profile.PaymentMethods);

    public Task AddPaymentMethodAsync(PaymentMethod method)
    {
        method.Id = _profile.PaymentMethods.Any() ? _profile.PaymentMethods.Max(p => p.Id) + 1 : 1;
        _profile.PaymentMethods.Add(method);
        return Task.CompletedTask;
    }

    public Task RemovePaymentMethodAsync(int methodId)
    {
        var method = _profile.PaymentMethods.FirstOrDefault(p => p.Id == methodId);
        if (method != null) _profile.PaymentMethods.Remove(method);
        return Task.CompletedTask;
    }

    public Task UpdateNotificationsAsync(NotificationPreferences preferences)
    {
        _profile.Notifications = preferences;
        return Task.CompletedTask;
    }
}

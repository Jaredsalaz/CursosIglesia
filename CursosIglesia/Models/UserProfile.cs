namespace CursosIglesia.Models;

public class UserProfile
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public DateTime JoinedDate { get; set; }
    public string Parish { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<PaymentMethod> PaymentMethods { get; set; } = new();
    public NotificationPreferences Notifications { get; set; } = new();

    public string FullName => $"{FirstName} {LastName}";
    public string Initials => $"{(FirstName.Length > 0 ? FirstName[0] : ' ')}{(LastName.Length > 0 ? LastName[0] : ' ')}";
}

public class PaymentMethod
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // "visa", "mastercard", "paypal"
    public string LastFourDigits { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class NotificationPreferences
{
    public bool EmailNotifications { get; set; } = true;
    public bool CourseUpdates { get; set; } = true;
    public bool PromotionalEmails { get; set; } = false;
    public bool NewCourseAlerts { get; set; } = true;
}

using CursosIglesia.Models;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class UserProfileViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICourseService _courseService;

    private UserProfile _profile = new();
    public UserProfile Profile
    {
        get => _profile;
        set => SetProperty(ref _profile, value);
    }

    private string _activeTab = "personal";
    public string ActiveTab
    {
        get => _activeTab;
        set => SetProperty(ref _activeTab, value);
    }

    // Editable fields
    private string _editFirstName = string.Empty;
    public string EditFirstName
    {
        get => _editFirstName;
        set => SetProperty(ref _editFirstName, value);
    }

    private string _editLastName = string.Empty;
    public string EditLastName
    {
        get => _editLastName;
        set => SetProperty(ref _editLastName, value);
    }

    private string _editEmail = string.Empty;
    public string EditEmail
    {
        get => _editEmail;
        set => SetProperty(ref _editEmail, value);
    }

    private string _editPhone = string.Empty;
    public string EditPhone
    {
        get => _editPhone;
        set => SetProperty(ref _editPhone, value);
    }

    private string _editBio = string.Empty;
    public string EditBio
    {
        get => _editBio;
        set => SetProperty(ref _editBio, value);
    }

    private string _editParish = string.Empty;
    public string EditParish
    {
        get => _editParish;
        set => SetProperty(ref _editParish, value);
    }

    private string _editCity = string.Empty;
    public string EditCity
    {
        get => _editCity;
        set => SetProperty(ref _editCity, value);
    }

    // Password
    private string _currentPassword = string.Empty;
    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }

    private string _newPassword = string.Empty;
    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    // Status messages
    private string? _successMessage;
    public string? SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    // Stats
    private int _enrolledCount;
    public int EnrolledCount
    {
        get => _enrolledCount;
        set => SetProperty(ref _enrolledCount, value);
    }

    private int _completedCount;
    public int CompletedCount
    {
        get => _completedCount;
        set => SetProperty(ref _completedCount, value);
    }

    public UserProfileViewModel(IUserService userService, IEnrollmentService enrollmentService, ICourseService courseService)
    {
        _userService = userService;
        _enrollmentService = enrollmentService;
        _courseService = courseService;
    }

    public override async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            Profile = await _userService.GetProfileAsync();
            LoadEditFields();

            var enrollments = await _enrollmentService.GetEnrollmentsAsync();
            EnrolledCount = enrollments.Count;
            CompletedCount = enrollments.Count(e => e.IsCompleted);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar el perfil: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadEditFields()
    {
        EditFirstName = Profile.FirstName;
        EditLastName = Profile.LastName;
        EditEmail = Profile.Email;
        EditPhone = Profile.Phone;
        EditBio = Profile.Bio;
        EditParish = Profile.Parish;
        EditCity = Profile.City;
    }

    public async Task SaveProfileAsync()
    {
        Profile.FirstName = EditFirstName;
        Profile.LastName = EditLastName;
        Profile.Email = EditEmail;
        Profile.Phone = EditPhone;
        Profile.Bio = EditBio;
        Profile.Parish = EditParish;
        Profile.City = EditCity;
        Profile.AvatarUrl = $"https://ui-avatars.com/api/?name={EditFirstName}+{EditLastName}&background=7b2d26&color=fff&size=150&font-size=0.4";

        await _userService.UpdateProfileAsync(Profile);
        SuccessMessage = "Datos personales actualizados correctamente.";
        OnPropertyChanged(nameof(Profile));
    }

    public async Task ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "Todos los campos son requeridos.";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Las contraseñas no coinciden.";
            return;
        }

        if (NewPassword.Length < 8)
        {
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres.";
            return;
        }

        await _userService.UpdatePasswordAsync(CurrentPassword, NewPassword);
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        ErrorMessage = null;
        SuccessMessage = "Contraseña actualizada correctamente.";
    }

    public async Task RemovePaymentMethodAsync(int id)
    {
        await _userService.RemovePaymentMethodAsync(id);
        Profile = await _userService.GetProfileAsync();
        SuccessMessage = "Método de pago eliminado.";
    }

    public async Task SaveNotificationsAsync()
    {
        await _userService.UpdateNotificationsAsync(Profile.Notifications);
        SuccessMessage = "Preferencias de notificación actualizadas.";
    }

    public void SwitchTab(string tab)
    {
        ActiveTab = tab;
        SuccessMessage = null;
        ErrorMessage = null;
    }
}

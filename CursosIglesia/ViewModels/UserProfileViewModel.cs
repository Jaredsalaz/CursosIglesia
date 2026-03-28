using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class UserProfileViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICourseService _courseService;

    private bool _isSavingProfile;
    public bool IsSavingProfile
    {
        get => _isSavingProfile;
        set => SetProperty(ref _isSavingProfile, value);
    }

    private bool _isChangingPassword;
    public bool IsChangingPassword
    {
        get => _isChangingPassword;
        set => SetProperty(ref _isChangingPassword, value);
    }

    private bool _isSavingNotifications;
    public bool IsSavingNotifications
    {
        get => _isSavingNotifications;
        set => SetProperty(ref _isSavingNotifications, value);
    }

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
        Console.WriteLine("[UserProfileViewModel] InitializeAsync called");
        try
        {
            Console.WriteLine("[UserProfileViewModel] Calling _userService.GetProfileAsync()...");
            Profile = await _userService.GetProfileAsync();
            Console.WriteLine($"[UserProfileViewModel] ✅ Profile loaded: {Profile.Email}");
            LoadEditFields();

            Console.WriteLine("[UserProfileViewModel] Loading enrollments...");
            var enrollments = await _enrollmentService.GetEnrollmentsAsync();
            EnrolledCount = enrollments.Count;
            CompletedCount = enrollments.Count(e => e.IsCompleted);
            Console.WriteLine($"[UserProfileViewModel] ✅ Enrollments loaded: {EnrolledCount} total, {CompletedCount} completed");
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"[UserProfileViewModel] ❌ HTTP Error: {httpEx.StatusCode} - {httpEx.Message}");
            ErrorMessage = $"Error al cargar el perfil: {httpEx.StatusCode} - {httpEx.Message}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UserProfileViewModel] ❌ Exception: {ex.GetType().Name} - {ex.Message}");
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
        if (IsSavingProfile || IsChangingPassword || IsSavingNotifications)
            return;

        ErrorMessage = null;
        SuccessMessage = null;
        IsSavingProfile = true;

        var request = new UpdateProfileRequest
        {
            FirstName = EditFirstName,
            LastName = EditLastName,
            Email = EditEmail,
            Phone = EditPhone,
            Bio = EditBio,
            Parish = EditParish,
            City = EditCity
        };

        try
        {
            var result = await _userService.UpdateProfileAsync(request);
            if (result.Success)
            {
                Profile = result.Profile ?? Profile;
                LoadEditFields();
                SuccessMessage = string.IsNullOrWhiteSpace(result.Message) ? "Perfil actualizado con éxito." : result.Message;
                OnPropertyChanged(nameof(Profile));
            }
            else
            {
                ErrorMessage = string.IsNullOrWhiteSpace(result.Message) ? "No se pudo actualizar el perfil." : result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al actualizar el perfil: {ex.Message}";
        }
        finally
        {
            IsSavingProfile = false;
        }
    }

    public async Task ChangePasswordAsync()
    {
        if (IsChangingPassword || IsSavingProfile || IsSavingNotifications)
            return;

        ErrorMessage = null;
        SuccessMessage = null;

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

        IsChangingPassword = true;

        var request = new ChangePasswordRequest
        {
            CurrentPassword = CurrentPassword,
            NewPassword = NewPassword
        };

        try
        {
            var result = await _userService.UpdatePasswordAsync(request);
            if (result.Success)
            {
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
                SuccessMessage = string.IsNullOrWhiteSpace(result.Message) ? "Contraseña actualizada con éxito." : result.Message;
            }
            else
            {
                ErrorMessage = string.IsNullOrWhiteSpace(result.Message) ? "No se pudo actualizar la contraseña." : result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al actualizar contraseña: {ex.Message}";
        }
        finally
        {
            IsChangingPassword = false;
        }
    }

    public async Task RemovePaymentMethodAsync(Guid id)
    {
        await _userService.RemovePaymentMethodAsync(id);
        Profile = await _userService.GetProfileAsync();
        SuccessMessage = "Método de pago eliminado.";
    }

    public async Task SaveNotificationsAsync()
    {
        if (IsSavingNotifications || IsSavingProfile || IsChangingPassword)
            return;

        ErrorMessage = null;
        SuccessMessage = null;
        IsSavingNotifications = true;

        try
        {
            await _userService.UpdateNotificationsAsync(Profile.Notifications);
            SuccessMessage = "Preferencias de notificación actualizadas.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar notificaciones: {ex.Message}";
        }
        finally
        {
            IsSavingNotifications = false;
        }
    }

    public void SwitchTab(string tab)
    {
        ActiveTab = tab;
        SuccessMessage = null;
        ErrorMessage = null;
    }
}

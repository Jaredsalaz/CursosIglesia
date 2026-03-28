using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private readonly IAuthService _authService;

    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    public RegisterModel RegisterModel { get; set; } = new();

    private bool _isSubmitting;
    public bool IsSubmitting
    {
        get => _isSubmitting;
        set => SetProperty(ref _isSubmitting, value);
    }

    private string? _successMessage;
    public string? SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    private bool _showPassword;
    public bool ShowPassword
    {
        get => _showPassword;
        set => SetProperty(ref _showPassword, value);
    }

    private bool _showConfirmPassword;
    public bool ShowConfirmPassword
    {
        get => _showConfirmPassword;
        set => SetProperty(ref _showConfirmPassword, value);
    }

    private int _currentStep = 1;
    public int CurrentStep
    {
        get => _currentStep;
        set => SetProperty(ref _currentStep, value);
    }

    public void TogglePassword() => ShowPassword = !ShowPassword;
    public void ToggleConfirmPassword() => ShowConfirmPassword = !ShowConfirmPassword;

    public bool ValidateStep1()
    {
        if (string.IsNullOrWhiteSpace(RegisterModel.FirstName) || string.IsNullOrWhiteSpace(RegisterModel.LastName))
        {
            ErrorMessage = "Por favor ingresa tu nombre completo.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(RegisterModel.Email) || !RegisterModel.Email.Contains("@"))
        {
            ErrorMessage = "Por favor ingresa un correo electrónico válido.";
            return false;
        }
        ErrorMessage = null;
        return true;
    }

    public void NextStep()
    {
        if (CurrentStep == 1 && ValidateStep1())
        {
            CurrentStep = 2;
        }
    }

    public void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            ErrorMessage = null;
        }
    }

    public async Task<bool> RegisterAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsSubmitting = true;

        try
        {
            var request = new RegisterRequest
            {
                FirstName = RegisterModel.FirstName,
                LastName = RegisterModel.LastName,
                Email = RegisterModel.Email,
                Password = RegisterModel.Password,
                Parish = RegisterModel.Parish,
                Country = RegisterModel.Country,
                AvatarUrl = string.IsNullOrEmpty(RegisterModel.AvatarUrl) 
                    ? $"https://ui-avatars.com/api/?name={RegisterModel.FirstName}+{RegisterModel.LastName}&background=7b2d26&color=fff"
                    : RegisterModel.AvatarUrl
            };

            var result = await _authService.RegisterAsync(request);

            if (result.Success)
            {
                SuccessMessage = $"¡Cuenta creada! Bienvenido, {result.Profile?.FirstName}. Redirigiendo...";
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception)
        {
            ErrorMessage = "Ocurrió un error inesperado. Inténtalo de nuevo.";
            return false;
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}

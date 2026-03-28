using CursosIglesia.Models;
using CursosIglesia.Models.DTOs;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.ViewModels.Base;

namespace CursosIglesia.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    public LoginModel LoginModel { get; set; } = new();

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

    public void TogglePassword() => ShowPassword = !ShowPassword;

    public async Task<bool> LoginAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsSubmitting = true;

        try
        {
            var request = new LoginRequest
            {
                Email = LoginModel.Email,
                Password = LoginModel.Password
            };

            var result = await _authService.LoginAsync(request);

            if (result.Success)
            {
                SuccessMessage = $"¡Bienvenido, {result.Profile?.FirstName}! Redirigiendo...";
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

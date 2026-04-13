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

    private string _otpCode = string.Empty;
    public string OtpCode
    {
        get => _otpCode;
        set => SetProperty(ref _otpCode, value);
    }

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
                SuccessMessage = "Revisa tu correo para obtener el código de seguridad.";
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch (Exception)
        {
            ErrorMessage = "Ocurrió un error inesperado al enviar los datos. Inténtalo de nuevo.";
            return false;
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    public async Task<bool> VerifyOtpAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsSubmitting = true;

        try
        {
            if (string.IsNullOrWhiteSpace(OtpCode) || OtpCode.Length < 4)
            {
                ErrorMessage = "El código ingresado no es válido.";
                return false;
            }

            var request = new VerifyOtpRequest
            {
                Email = RegisterModel.Email,
                Otp = OtpCode
            };

            var result = await _authService.VerifyRegistrationOtpAsync(request);

            if (result.Success)
            {
                // ✅ ¡Iniciar sesión automáticamente tras verificar!
                var loginRequest = new LoginRequest 
                { 
                    Email = RegisterModel.Email, 
                    Password = RegisterModel.Password 
                };
                await _authService.LoginAsync(loginRequest);

                SuccessMessage = "¡Cuenta verificada exitosamente! Redirigiendo...";
                return true;
            }

            ErrorMessage = result.Message;
            return false;
        }
        catch
        {
            ErrorMessage = "Error verificando el código. Revisa tu conexión.";
            return false;
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}

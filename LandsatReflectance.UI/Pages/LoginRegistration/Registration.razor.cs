using FluentValidation;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Severity = MudBlazor.Severity;

namespace LandsatReflectance.UI.Pages.LoginRegistration;



public class UserModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordReEnter { get; set; } = string.Empty;
}

public class UserModelValidator : AbstractValidator<UserModel>
{
    public UserModelValidator()
    {
        RuleFor(model => model.FirstName)
            .NotEmpty().WithMessage("A first name is required.")
            .Length(1, 100);
        
        RuleFor(model => model.LastName)
            .NotEmpty().WithMessage("A last name is required.")
            .Length(1, 100);

        RuleFor(model => model.Email)
            .NotEmpty().WithMessage("An email is required.")
            .EmailAddress()
            .Must((value, cancellationToken) => true);  // add logic to check if email is unique here

        RuleFor(model => model.Password)
            .NotEmpty().WithMessage("A password is required.")
            .MinimumLength(8).WithMessage("The password must have a minimum length of 8 characters.");

        RuleFor(model => model.PasswordReEnter)
            .NotEmpty().WithMessage("Please re-type your password.")
            .Equal(model => model.Password).WithMessage("Passwords do not match.");
    }
    
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UserModel>.CreateWithOptions((UserModel)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}


public partial class Registration : ComponentBase
{
    [Parameter]
    public string Email { get; set; } = string.Empty;

    private MudForm m_mudForm = new();
    private UserModelValidator m_userModelValidator = new();
    private UserModel m_userModel = new();
    
    
    protected override void OnInitialized()
    {
        var uri = new Uri(NavigationManager.Uri);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (query.TryGetValue("email", out var email))
        {
            Email = email.ToString();
        }

        m_userModel.Email = Email;
    }
    
#region Password Text Field
    private InputType m_passwordFieldInputType = InputType.Password;

    private string PasswordFieldInputIcon =>
        m_passwordFieldInputType is InputType.Password
            ? Icons.Material.Filled.Visibility
            : Icons.Material.Filled.VisibilityOff;

    private void OnPasswordVisibilityButtonClicked()
    {
        m_passwordFieldInputType = m_passwordFieldInputType is InputType.Password 
            ? InputType.Text 
            : InputType.Password;
    }
#endregion


    private async Task SubmitForm()
    {
        await m_mudForm.Validate();

        Snackbar.Add($"Form status: {m_mudForm.IsValid}", m_mudForm.IsValid ? Severity.Info : Severity.Error);
    }
}
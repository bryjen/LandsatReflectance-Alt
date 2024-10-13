using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LandsatReflectance.UI.Pages.LoginRegistration;

public partial class Login : ComponentBase
{
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

#region RegistrationPanel
    private string m_registrationEmail = string.Empty;

    private void GoToRegistrationPage()
    {
        string emailQueryString = string.IsNullOrWhiteSpace(m_registrationEmail)
            ? string.Empty
            : "?email=" + Uri.EscapeDataString(m_registrationEmail);
        
        NavigationManager.NavigateTo($"/Register{emailQueryString}");
    }

#endregion
}
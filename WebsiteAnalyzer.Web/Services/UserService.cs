using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using WebsiteAnalyzer.Infrastructure;

namespace WebsiteAnalyzer.Web.Services;

public interface IUserService
{
    Task<ApplicationUser?> GetCurrentUserAsync();
}

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public UserService(UserManager<ApplicationUser> userManager, AuthenticationStateProvider authenticationStateProvider)
    {
        _userManager = userManager;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        AuthenticationState authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        ApplicationUser? user = await _userManager.GetUserAsync(authenticationState.User);

        return user;
    }
}
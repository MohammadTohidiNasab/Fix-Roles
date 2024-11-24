using Microsoft.AspNetCore.Mvc.Filters;

public class AccessLevelAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    private readonly AccessLevel _requiredAccessLevel;

    public AccessLevelAuthorizeAttribute(AccessLevel requiredAccessLevel)
    {
        _requiredAccessLevel = requiredAccessLevel;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userManager = (UserManager<CustomUser>)context.HttpContext.RequestServices.GetService(typeof(UserManager<CustomUser>));
        var user = await userManager.GetUserAsync(context.HttpContext.User);

        if (user == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var roles = await userManager.GetRolesAsync(user);
        var roleManager = (RoleManager<Role>)context.HttpContext.RequestServices.GetService(typeof(RoleManager<Role>));

        // Fetch the user roles and check for permissions
        var userRoles = await Task.WhenAll(roles.Select(role => roleManager.FindByNameAsync(role)));

        // Extract the permissions for the user's roles
        var userPermissions = userRoles.SelectMany(r => ((Role)r).Permissions).ToList();

        if (!userPermissions.Contains(_requiredAccessLevel))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}

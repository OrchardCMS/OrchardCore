using System.Security.Claims;
using System.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;
using OrchardCore.ResourceManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;
using OrchardCore.Setup.Events;
using OrchardCore.Sms;
using OrchardCore.Users.Commands;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.DataMigrations;
using OrchardCore.Users.Deployment;
using OrchardCore.Users.Drivers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Liquid;
using OrchardCore.Users.Models;
using OrchardCore.Users.Recipes;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using YesSql.Filters.Query;

namespace OrchardCore.Users;

public sealed class Startup : StartupBase
{
    private static readonly string _accountControllerName = typeof(AccountController).ControllerName();
    private static readonly string _emailConfirmationControllerName = typeof(EmailConfirmationController).ControllerName();

    private readonly string _tenantName;

    private UserOptions _userOptions;

    public Startup(ShellSettings shellSettings)
    {
        _tenantName = shellSettings.Name;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<ExternalAuthenticationMigrations>();

        services.Configure<UserOptions>(userOptions =>
        {
            var configuration = ShellScope.Services.GetRequiredService<IShellConfiguration>();
            configuration.GetSection("OrchardCore_Users").Bind(userOptions);
        });

        // Add ILookupNormalizer as Singleton because it is needed by UserIndexProvider
        services.TryAddSingleton<ILookupNormalizer, UpperInvariantLookupNormalizer>();

        // Add the default token providers used to generate tokens for an admin to change user's password.
        services.AddIdentity<IUser, IRole>()
            .AddTokenProvider<DataProtectorTokenProvider<IUser>>(TokenOptions.DefaultProvider);

        // Configure token provider for email confirmation.
        services.AddTransient<IConfigureOptions<IdentityOptions>, EmailConfirmationIdentityOptionsConfigurations>()
            .AddTransient<EmailConfirmationTokenProvider>()
            .AddOptions<EmailConfirmationTokenProviderOptions>();

        services.AddTransient<IConfigureOptions<IdentityOptions>, IdentityOptionsConfigurations>();
        services.AddPhoneFormatValidator();
        // Configure the authentication options to use the application cookie scheme as the default sign-out handler.
        // This is required for security modules like the OpenID module (that uses SignOutAsync()) to work correctly.
        services.AddAuthentication(options => options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme);

        services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter, ExternalAuthenticationsStartupFilter>();
        services.AddUsers();

        services.ConfigureApplicationCookie(options =>
        {
            var userOptions = ShellScope.Services.GetRequiredService<IOptions<UserOptions>>();

            options.Cookie.Name = "orchauth_" + HttpUtility.UrlEncode(_tenantName);

            // Don't set the cookie builder 'Path' so that it uses the 'IAuthenticationFeature' value
            // set by the pipeline and coming from the request 'PathBase' which already ends with the
            // tenant prefix but may also start by a path related e.g to a virtual folder.

            options.LoginPath = "/" + userOptions.Value.LoginPath;
            options.LogoutPath = "/" + userOptions.Value.LogoffPath;
            options.AccessDeniedPath = "/Error/403";
        });

        services.AddTransient<IPostConfigureOptions<SecurityStampValidatorOptions>, ConfigureSecurityStampOptions>();
        services.AddDataMigration<Migrations>();

        services.AddScoped<IUserClaimsProvider, EmailClaimsProvider>();
        services.AddSingleton<IUserIdGenerator, DefaultUserIdGenerator>();

        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<ISetupEventHandler, SetupEventHandler>();
        services.AddScoped<ICommandHandler, UserCommands>();
        services.AddScoped<IExternalLoginEventHandler, ScriptExternalLoginEventHandler>();

        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();

        services.AddSiteDisplayDriver<LoginSettingsDisplayDriver>();

        services.AddDisplayDriver<User, UserDisplayDriver>();
        services.AddDisplayDriver<User, UserInformationDisplayDriver>();
        services.AddDisplayDriver<User, UserButtonsDisplayDriver>();

        services.AddScoped<IThemeSelector, UsersThemeSelector>();

        services.AddScoped<IRecipeEnvironmentProvider, RecipeEnvironmentSuperUserProvider>();

        services.AddScoped<IUsersAdminListQueryService, DefaultUsersAdminListQueryService>();

        services.AddDisplayDriver<UserIndexOptions, UserOptionsDisplayDriver>();

        services.AddSingleton<IUsersAdminListFilterParser>(sp =>
        {
            var filterProviders = sp.GetServices<IUsersAdminListFilterProvider>();
            var builder = new QueryEngineBuilder<User>();
            foreach (var provider in filterProviders)
            {
                provider.Build(builder);
            }

            var parser = builder.Build();

            return new DefaultUsersAdminListFilterParser(parser);
        });

        services.AddTransient<IUsersAdminListFilterProvider, DefaultUsersAdminListFilterProvider>();
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, UserOptionsConfiguration>();
        services.AddDisplayDriver<Navbar, UserMenuNavbarDisplayDriver>();
        services.AddDisplayDriver<UserMenu, UserMenuDisplayDriver>();
        services.AddShapeTableProvider<UserMenuShapeTableProvider>();

        services.AddRecipeExecutionStep<UsersStep>();

        services.AddScoped<CustomUserSettingsService>();
        services.AddRecipeExecutionStep<CustomUserSettingsStep>();
        services.AddDisplayDriver<LoginForm, LoginFormDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        _userOptions ??= serviceProvider.GetRequiredService<IOptions<UserOptions>>().Value;

        routes.MapAreaControllerRoute(
            name: "Login",
            areaName: UserConstants.Features.Users,
            pattern: _userOptions.LoginPath,
            defaults: new
            {
                controller = _accountControllerName,
                action = nameof(AccountController.Login),
            }
        );

        routes.MapAreaControllerRoute(
            name: "ChangePassword",
            areaName: UserConstants.Features.Users,
            pattern: _userOptions.ChangePasswordUrl,
            defaults: new
            {
                controller = _accountControllerName,
                action = nameof(AccountController.ChangePassword),
            }
        );

        routes.MapAreaControllerRoute(
            name: "ChangePasswordConfirmation",
            areaName: UserConstants.Features.Users,
            pattern: _userOptions.ChangePasswordConfirmationUrl,
            defaults: new
            {
                controller = _accountControllerName,
                action = nameof(AccountController.ChangePasswordConfirmation),
            }
        );

        routes.MapAreaControllerRoute(
            name: "UsersLogOff",
            areaName: UserConstants.Features.Users,
            pattern: _userOptions.LogoffPath,
            defaults: new
            {
                controller = _accountControllerName,
                action = nameof(AccountController.LogOff),
            }
        );

        routes.MapAreaControllerRoute(
            name: "ConfirmEmail",
            areaName: UserConstants.Features.Users,
            pattern: "ConfirmEmail",
            defaults: new
            {
                controller = _emailConfirmationControllerName,
                action = nameof(EmailConfirmationController.ConfirmEmail),
            }
        );

        routes.MapAreaControllerRoute(
            name: "ConfirmEmailSent",
            areaName: UserConstants.Features.Users,
            pattern: "ConfirmEmailSent",
            defaults: new
            {
                controller = _emailConfirmationControllerName,
                action = nameof(EmailConfirmationController.ConfirmEmailSent),
            }
        );

        builder.UseAuthorization();
    }
}

[Feature(UserConstants.Features.ExternalAuthentication)]
public sealed class ExternalAuthenticationStartup : StartupBase
{
    private static readonly string _accountControllerName = typeof(AccountController).ControllerName();

    private UserOptions _userOptions;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ILoginFormEvent, ExternalLoginFormEvents>();
        services.AddNavigationProvider<RegistrationAdminMenu>();
        services.AddDisplayDriver<UserMenu, ExternalAuthenticationUserMenuDisplayDriver>();
        services.AddSiteDisplayDriver<ExternalRegistrationSettingsDisplayDriver>();
        services.AddSiteDisplayDriver<ExternalLoginSettingsDisplayDriver>();
        services.AddTransient<IConfigureOptions<ExternalLoginOptions>, ExternalLoginOptionsConfigurations>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        _userOptions ??= serviceProvider.GetRequiredService<IOptions<UserOptions>>().Value;

        routes.MapAreaControllerRoute(
            name: "ExternalLogins",
            areaName: UserConstants.Features.Users,
            pattern: _userOptions.ExternalLoginsUrl,
            defaults: new
            {
                controller = _accountControllerName,
                action = nameof(ExternalAuthenticationsController.ExternalLogins),
            }
        );
    }
}

[RequireFeatures("OrchardCore.Roles")]
public sealed class RolesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IRoleRemovedEventHandler, UserRoleRemovedEventHandler>();
        services.AddIndexProvider<UserByRoleNameIndexProvider>();
        services.AddDisplayDriver<User, UserRoleDisplayDriver>();
        services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
        services.AddPermissionProvider<UserRolePermissions>();
        services.AddSingleton<IUsersAdminListFilterProvider, RolesAdminListFilterProvider>();
    }
}

[RequireFeatures("OrchardCore.Liquid")]
public sealed class LiquidStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.Filters.AddFilter("has_claim", UserFilters.HasClaim);
            o.Filters.AddFilter("user_id", UserFilters.UserId);

            o.MemberAccessStrategy.Register<ClaimsPrincipal>();
            o.MemberAccessStrategy.Register<ClaimsIdentity>();

            o.Scope.SetValue("User", new ObjectValue(new LiquidUserAccessor()));
            o.MemberAccessStrategy.Register<LiquidUserAccessor, FluidValue>((obj, name, ctx) =>
            {
                var user = ((LiquidTemplateContext)ctx).Services.GetRequiredService<IHttpContextAccessor>().HttpContext?.User;
                if (user != null)
                {
                    return name switch
                    {
                        nameof(ClaimsPrincipal.Identity) => new ObjectValue(user.Identity),
                        _ => NilValue.Instance
                    };
                }

                return NilValue.Instance;
            });

            o.MemberAccessStrategy.Register<User, FluidValue>((user, name, context) =>
            {
                return name switch
                {
                    nameof(User.UserId) => new StringValue(user.UserId),
                    nameof(User.UserName) => new StringValue(user.UserName),
                    nameof(User.NormalizedUserName) => new StringValue(user.NormalizedUserName),
                    nameof(User.Email) => new StringValue(user.Email),
                    nameof(User.NormalizedEmail) => new StringValue(user.NormalizedEmail),
                    nameof(User.EmailConfirmed) => user.EmailConfirmed ? BooleanValue.True : BooleanValue.False,
                    nameof(User.IsEnabled) => user.IsEnabled ? BooleanValue.True : BooleanValue.False,
                    nameof(User.RoleNames) => new ArrayValue(user.RoleNames.Select(x => new StringValue(x)).ToArray()),
                    nameof(User.Properties) => new ObjectValue(user.Properties),
                    _ => NilValue.Instance
                };
            });
        })
       .AddLiquidFilter<UsersByIdFilter>("users_by_id")
       .AddLiquidFilter<HasPermissionFilter>("has_permission")
       .AddLiquidFilter<IsInRoleFilter>("is_in_role")
       .AddLiquidFilter<UserEmailFilter>("user_email");
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class LoginDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<LoginSettings, LoginDeploymentStartup>(S => S["Login settings"], S => S["Exports the Login settings."]);
    }
}

[Feature("OrchardCore.Users.ChangeEmail")]
public sealed class ChangeEmailStartup : StartupBase
{
    private const string ChangeEmailPath = "ChangeEmail";
    private const string ChangeEmailConfirmationPath = "ChangeEmailConfirmation";
    private const string ChangeEmailControllerName = "ChangeEmail";

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "ChangeEmail",
            areaName: UserConstants.Features.Users,
            pattern: ChangeEmailPath,
            defaults: new
            {
                controller = ChangeEmailControllerName,
                action = nameof(ChangeEmailController.Index),
            }
        );

        routes.MapAreaControllerRoute(
            name: "ChangeEmailConfirmation",
            areaName: UserConstants.Features.Users,
            pattern: ChangeEmailConfirmationPath,
            defaults: new
            {
                controller = ChangeEmailControllerName,
                action = nameof(ChangeEmailController.ChangeEmailConfirmation),
            }
        );
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<IdentityOptions>, ChangeEmailIdentityOptionsConfigurations>()
            .AddTransient<ChangeEmailTokenProvider>()
            .AddOptions<ChangeEmailTokenProviderOptions>();

        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<ChangeEmailViewModel>();
        });

        services.AddSiteDisplayDriver<ChangeEmailSettingsDisplayDriver>();
        services.AddNavigationProvider<ChangeEmailAdminMenu>();
        services.AddDisplayDriver<UserMenu, ChangeEmailUserMenuDisplayDriver>();
    }
}

[Feature("OrchardCore.Users.ChangeEmail")]
[RequireFeatures("OrchardCore.Deployment")]
public sealed class ChangeEmailDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<ChangeEmailDeploymentStartup, ChangeEmailDeploymentStartup>(S => S["Change Email settings"], S => S["Exports the Change Email settings."]);
    }
}

[Feature(UserConstants.Features.UserRegistration)]
public sealed class RegistrationStartup : StartupBase
{
    private const string RegisterPath = nameof(RegistrationController.Register);
    private const string ConfirmEmailSent = nameof(EmailConfirmationController.ConfirmEmailSent);
    private const string RegistrationPending = nameof(RegistrationController.RegistrationPending);
    private const string RegistrationControllerName = "Registration";

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<ConfirmEmailViewModel>();
        });

        services.AddDisplayDriver<User, UserRegistrationAdminDisplayDriver>();
        services.AddSiteDisplayDriver<RegistrationSettingsDisplayDriver>();
        services.AddNavigationProvider<RegistrationAdminMenu>();

        services.AddDisplayDriver<LoginForm, RegisterUserLoginFormDisplayDriver>();
        services.AddDisplayDriver<RegisterUserForm, RegisterUserFormDisplayDriver>();
        services.AddTransient<IConfigureOptions<RegistrationOptions>, RegistrationOptionsConfigurations>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: RegisterPath,
            areaName: UserConstants.Features.Users,
            pattern: RegisterPath,
            defaults: new
            {
                controller = RegistrationControllerName,
                action = RegisterPath,
            }
        );

        routes.MapAreaControllerRoute(
            name: ConfirmEmailSent,
            areaName: UserConstants.Features.Users,
            pattern: ConfirmEmailSent,
            defaults: new
            {
                controller = RegistrationControllerName,
                action = ConfirmEmailSent,
            }
        );

        routes.MapAreaControllerRoute(
            name: RegistrationPending,
            areaName: UserConstants.Features.Users,
            pattern: RegistrationPending,
            defaults: new
            {
                controller = RegistrationControllerName,
                action = RegistrationPending,
            }
        );
    }
}

[Feature(UserConstants.Features.UserRegistration)]
[RequireFeatures("OrchardCore.Deployment")]
public sealed class RegistrationDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<RegistrationSettings, RegistrationDeploymentStartup>(S => S["Registration settings"], S => S["Exports the Registration settings."]);
    }
}

[Feature(UserConstants.Features.ResetPassword)]
public sealed class ResetPasswordStartup : StartupBase
{
    private const string ForgotPasswordPath = "ForgotPassword";
    private const string ForgotPasswordConfirmationPath = "ForgotPasswordConfirmation";
    private const string ResetPasswordPath = "ResetPassword";
    private const string ResetPasswordConfirmationPath = "ResetPasswordConfirmation";
    private const string ResetPasswordControllerName = "ResetPassword";

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<IdentityOptions>, PasswordResetIdentityOptionsConfigurations>()
            .AddTransient<PasswordResetTokenProvider>()
            .AddOptions<PasswordResetTokenProviderOptions>();

        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<LostPasswordViewModel>();
        });

        services.AddSiteDisplayDriver<ResetPasswordSettingsDisplayDriver>();
        services.AddNavigationProvider<ResetPasswordAdminMenu>();

        services.AddDisplayDriver<ResetPasswordForm, ResetPasswordFormDisplayDriver>();
        services.AddDisplayDriver<LoginForm, ForgotPasswordLoginFormDisplayDriver>();
        services.AddDisplayDriver<ForgotPasswordForm, ForgotPasswordFormDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "ForgotPassword",
            areaName: UserConstants.Features.Users,
            pattern: ForgotPasswordPath,
            defaults: new
            {
                controller = ResetPasswordControllerName,
                action = nameof(ResetPasswordController.ForgotPassword),
            }
        );
        routes.MapAreaControllerRoute(
            name: "ForgotPasswordConfirmation",
            areaName: UserConstants.Features.Users,
            pattern: ForgotPasswordConfirmationPath,
            defaults: new
            {
                controller = ResetPasswordControllerName,
                action = nameof(ResetPasswordController.ForgotPasswordConfirmation),
            }
        );
        routes.MapAreaControllerRoute(
            name: "ResetPassword",
            areaName: UserConstants.Features.Users,
            pattern: ResetPasswordPath,
            defaults: new
            {
                controller = ResetPasswordControllerName,
                action = nameof(ResetPasswordController.ResetPassword),
            }
        );
        routes.MapAreaControllerRoute(
            name: "ResetPasswordConfirmation",
            areaName: UserConstants.Features.Users,
            pattern: ResetPasswordConfirmationPath,
            defaults: new
            {
                controller = ResetPasswordControllerName,
                action = nameof(ResetPasswordController.ResetPasswordConfirmation),
            }
        );
    }
}

[Feature(UserConstants.Features.ResetPassword)]
[RequireFeatures("OrchardCore.Deployment")]
public sealed class ResetPasswordDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<ResetPasswordSettings, ResetPasswordDeploymentStartup>(S => S["Reset Password settings"], S => S["Exports the Reset Password settings."]);
    }
}

[Feature("OrchardCore.Users.CustomUserSettings")]
public sealed class CustomUserSettingsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<User, CustomUserSettingsDisplayDriver>();
        services.AddPermissionProvider<CustomUserSettingsPermissions>();
        services.AddDeployment<CustomUserSettingsDeploymentSource, CustomUserSettingsDeploymentStep, CustomUserSettingsDeploymentStepDriver>();
        services.AddScoped<IStereotypesProvider, CustomUserSettingsStereotypesProvider>();

        services.Configure<ContentTypeDefinitionOptions>(options =>
        {
            options.Stereotypes.TryAdd("CustomUserSettings", new ContentTypeDefinitionDriverOptions
            {
                ShowCreatable = false,
                ShowListable = false,
                ShowDraftable = false,
                ShowVersionable = false,
            });
        });
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class UserDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AllUsersDeploymentSource, AllUsersDeploymentStep, AllUsersDeploymentStepDriver>();
    }
}

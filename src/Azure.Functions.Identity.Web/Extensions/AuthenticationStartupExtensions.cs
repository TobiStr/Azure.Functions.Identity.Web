using Microsoft.AspNetCore.Authentication;
using System;
using System.Linq;

namespace Azure.Functions.Identity.Web.Extensions;

//Code from https://github.com/AzureAD/microsoft-identity-web/issues/916
public static class AuthenticationStartupExtensions
{
#if DEBUG
    private const string CliAuthenticationHandler = "Azure.Functions.Cli.Actions.HostActions.WebHost.Security.CliAuthenticationHandler`1";
    private static Type _cliAuthenticationHandlerType = null;

    private static Type CliAuthenticationHandlerType
    {
        get
        {
            return _cliAuthenticationHandlerType ??= AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.IsClass && x.FullName == CliAuthenticationHandler)!;
        }
    }

#endif
    // ARM Authentication
#if DEBUG
    private const string ArmAuthenticationOptions = "Microsoft.Azure.WebJobs.Script.WebHost.Security.Authentication.ArmAuthenticationOptions";
    private static Type _armAuthenticationOptionsType = null;

    private static Type ArmAuthenticationOptionsType
    {
        get
        {
            return _armAuthenticationOptionsType ??= AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.IsClass && x.FullName == ArmAuthenticationOptions)!;
        }
    }

    public static AuthenticationBuilder AddArmToken(this AuthenticationBuilder builder)
    {
        try
        {
            var method = builder.GetType().GetMethods()
                .Where(x => x.Name == nameof(AuthenticationBuilder.AddScheme))
                .FirstOrDefault(x => x.GetParameters().Length == 2);
            var genericMethod = method!.MakeGenericMethod(ArmAuthenticationOptionsType, CliAuthenticationHandlerType.MakeGenericType(ArmAuthenticationOptionsType));

            return (genericMethod.Invoke(builder, new object[] { "ArmToken", new Action<dynamic>((_) => { }) }) as AuthenticationBuilder)!;
        }
        catch (Exception)
        {
            return builder;
        }

    }

#else
    private const string ArmAuthenticationExtensions = "Microsoft.Extensions.DependencyInjection.ArmAuthenticationExtensions";
    private static Type _armAuthenticationExtensionsTypes = null;
    private static Type ArmAuthenticationExtensionsType
    {
        get
        {
            if (_armAuthenticationExtensionsTypes is null)
            {
                _armAuthenticationExtensionsTypes = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .FirstOrDefault(x => x.IsClass && x.FullName == ArmAuthenticationExtensions);
            }
            return _armAuthenticationExtensionsTypes;
        }
    }

    public static AuthenticationBuilder AddArmToken(this AuthenticationBuilder builder)
    {
        var method = ArmAuthenticationExtensionsType.GetMethods()
            .Where(x => x.Name == nameof(AddArmToken))
            .FirstOrDefault(x => x.GetParameters().Length == 1);
        return method.Invoke(null, new object[] { builder }) as AuthenticationBuilder;
    }
#endif
    // Script Authentication Level
#if DEBUG
    private const string AuthenticationLevelOptions = "Microsoft.Azure.WebJobs.Script.WebHost.Authentication.AuthenticationLevelOptions";
    private static Type _authenticationLevelOptionsType = null;

    private static Type AuthenticationLevelOptionsType
    {
        get
        {
            return _authenticationLevelOptionsType ??= AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.IsClass && x.FullName == AuthenticationLevelOptions)!;
        }
    }

    public static AuthenticationBuilder AddScriptAuthLevel(this AuthenticationBuilder builder)
    {
        try
        {
            var method = builder.GetType().GetMethods()
                .Where(x => x.Name == nameof(AuthenticationBuilder.AddScheme))
                .FirstOrDefault(x => x.GetParameters().Length == 2);
            var genericMethod = method!.MakeGenericMethod(AuthenticationLevelOptionsType, CliAuthenticationHandlerType.MakeGenericType(AuthenticationLevelOptionsType));
            return (genericMethod.Invoke(builder, new object[] { "WebJobsAuthLevel", new Action<dynamic>((_) => { }) }) as AuthenticationBuilder)!;
        }
        catch (Exception)
        {
            return builder;
        }

    }

#else
    private const string AuthLevelExtensionsExtensions = "Microsoft.Extensions.DependencyInjection.AuthLevelExtensions";
    private static Type _authLevelExtensionsExtensionsTypes = null;
    private static Type AuthLevelExtensionsExtensionsType
    {
        get
        {
            if (_authLevelExtensionsExtensionsTypes is null)
            {
                _authLevelExtensionsExtensionsTypes = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .FirstOrDefault(x => x.IsClass && x.FullName == AuthLevelExtensionsExtensions);
            }
            return _authLevelExtensionsExtensionsTypes;
        }
    }

    public static AuthenticationBuilder AddScriptAuthLevel(this AuthenticationBuilder builder)
    {
        var method = AuthLevelExtensionsExtensionsType.GetMethods()
            .Where(x => x.Name == nameof(AddScriptAuthLevel))
            .FirstOrDefault(x => x.GetParameters().Length == 1);
        return method.Invoke(null, new object[] { builder }) as AuthenticationBuilder;
    }
#endif
    // JWT Bearer

    private const string JwtBearerExtensions = "Microsoft.Extensions.DependencyInjection.ScriptJwtBearerExtensions";
    private static Type _jwtBearerExtensionsTypes = null;

    private static Type JwtBearerExtensionsType
    {
        get
        {
            return _jwtBearerExtensionsTypes ??= AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.IsClass && x.FullName == JwtBearerExtensions)!;
        }
    }

    public static AuthenticationBuilder AddScriptJwtBearer(this AuthenticationBuilder builder)
    {
        var method = JwtBearerExtensionsType.GetMethods()
            .Where(x => x.Name == nameof(AddScriptJwtBearer))
            .FirstOrDefault(x => x.GetParameters().Length == 1);
        return (method!.Invoke(null, new object[] { builder }) as AuthenticationBuilder)!;
    }
}

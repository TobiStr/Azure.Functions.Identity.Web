using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Azure.Functions.Identity.Web.Extensions;

//code from https://github.com/AzureAD/microsoft-identity-web/issues/916
public static class AuthorizationStartupExtensions
{
    private const string AuthorizationOptionsExtensions = "Microsoft.Azure.WebJobs.Script.WebHost.Security.Authorization.Policies.AuthorizationOptionsExtensions";
    private static Type _authorizationOptionsExtensionsType = null;

    private static Type AuthorizationOptionsExtensionsType
    {
        get
        {
            return _authorizationOptionsExtensionsType ??= AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .FirstOrDefault(t => t.IsClass && t.FullName == AuthorizationOptionsExtensions)!;
        }
    }

    public static void AddScriptPolicies(this AuthorizationOptions options)
    {
        var method = AuthorizationOptionsExtensionsType.GetMethods()
            .Where(x => x.Name == nameof(AddScriptPolicies))
            .FirstOrDefault(x => x.GetParameters().Length == 1);

        method!.Invoke(null, new object[] { options });
    }

    private const string AuthLevelAuthorizationHandler = "Microsoft.Azure.WebJobs.Script.WebHost.Security.Authorization.AuthLevelAuthorizationHandler";
    private static Type _authLevelAuthorizationHandlerType = null;

    private static Type AuthLevelAuthorizationHandlerType
    {
        get
        {
            return _authLevelAuthorizationHandlerType ??= AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .FirstOrDefault(t => t.IsClass && t.FullName == AuthLevelAuthorizationHandler)!;
        }
    }

    public static IServiceCollection AddAuthLevelAuthorizationHandler(this IServiceCollection services)
    {
        try
        {
            var method = typeof(ServiceCollectionServiceExtensions).GetMethods()
                .Where(x => x.Name == nameof(ServiceCollectionServiceExtensions.AddSingleton))
                .FirstOrDefault(x => x.GetParameters().Length == 1);
            var genericMethod = method!.MakeGenericMethod(typeof(IAuthorizationHandler), AuthLevelAuthorizationHandlerType);
            return (genericMethod.Invoke(null, new object[] { services }) as IServiceCollection)!;
        }
        catch (Exception)
        {
            return services;
        }

    }

    private const string NamedAuthLevelAuthorizationHandler = "Microsoft.Azure.WebJobs.Script.WebHost.Security.Authorization.NamedAuthLevelAuthorizationHandler";
    private static Type _namedAuthLevelAuthorizationHandlerType = null;

    private static Type NamedAuthLevelAuthorizationHandlerType
    {
        get
        {
            return _namedAuthLevelAuthorizationHandlerType ??= AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .FirstOrDefault(t => t.IsClass && t.FullName == NamedAuthLevelAuthorizationHandler)!;
        }
    }

    public static IServiceCollection AddNamedAuthLevelAuthorizationHandler(this IServiceCollection services)
    {
        var method = typeof(ServiceCollectionServiceExtensions).GetMethods()
            .Where(x => x.Name == nameof(ServiceCollectionServiceExtensions.AddSingleton))
            .FirstOrDefault(x => x.GetParameters().Length == 1);
        var genericMethod = method!.MakeGenericMethod(typeof(IAuthorizationHandler), NamedAuthLevelAuthorizationHandlerType);
        return (genericMethod.Invoke(null, new object[] { services }) as IServiceCollection)!;
    }

    private const string FunctionAuthorizationHandler = "Microsoft.Azure.WebJobs.Script.WebHost.Security.Authorization.FunctionAuthorizationHandler";
    private static Type _functionAuthorizationHandlerType = null;

    private static Type FunctionAuthorizationHandlerType
    {
        get
        {
            return _functionAuthorizationHandlerType ??= AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .FirstOrDefault(t => t.IsClass && t.FullName == FunctionAuthorizationHandler)!;
        }
    }

    public static IServiceCollection AddFunctionAuthorizationHandler(this IServiceCollection services)
    {
        var method = typeof(ServiceCollectionServiceExtensions).GetMethods()
            .Where(x => x.Name == nameof(ServiceCollectionServiceExtensions.AddSingleton))
            .FirstOrDefault(x => x.GetParameters().Length == 1);
        var genericMethod = method!.MakeGenericMethod(typeof(IAuthorizationHandler), FunctionAuthorizationHandlerType);
        return (genericMethod.Invoke(null, new object[] { services }) as IServiceCollection)!;
    }
}

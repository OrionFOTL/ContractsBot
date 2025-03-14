namespace ContractsBot.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddAndValidateOptions<TOptions>(this IServiceCollection services, string? configurationSection = null)
        where TOptions : class
    {
        return services
            .AddOptionsWithValidateOnStart<TOptions>()
            .BindConfiguration(configurationSection ?? typeof(TOptions).Name)
            .ValidateDataAnnotations()
            .Services;
    }
}

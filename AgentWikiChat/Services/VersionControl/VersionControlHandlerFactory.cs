using Microsoft.Extensions.Configuration;

namespace AgentWikiChat.Services.VersionControl;

/// <summary>
/// Factory para crear instancias de handlers de sistemas de control de versiones.
/// Similar a DatabaseHandlerFactory, permite agregar nuevos proveedores fácilmente.
/// </summary>
public static class VersionControlHandlerFactory
{
    /// <summary>
    /// Crea un handler de sistema de control de versiones según el proveedor configurado.
    /// </summary>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <returns>Instancia del handler apropiado</returns>
    public static IVersionControlHandler CreateHandler(IConfiguration configuration)
    {
        var repoConfig = configuration.GetSection("Repository");
        
        var providerType = repoConfig.GetValue<string>("Provider")?.ToLowerInvariant()
            ?? throw new InvalidOperationException("Repository:Provider no configurado en appsettings.json");

        var repositoryUrl = repoConfig.GetValue<string>("RepositoryUrl")
            ?? throw new InvalidOperationException("Repository:RepositoryUrl no configurada en appsettings.json");

        var commandTimeout = repoConfig.GetValue("CommandTimeout", 60);

        return providerType switch
        {
            "svn" or "subversion" => new SvnVersionControlHandler(configuration),
            "git" => new GitVersionControlHandler(configuration),
            "github" => new GitHubVersionControlHandler(configuration),
            // Futuro: "mercurial" or "hg" => new MercurialVersionControlHandler(configuration),
            // Futuro: "tfs" or "tfvc" => new TfsVersionControlHandler(configuration),
            // Futuro: "perforce" or "p4" => new PerforceVersionControlHandler(configuration),
            // Futuro: "gitlab" => new GitLabVersionControlHandler(configuration),
            // Futuro: "bitbucket" => new BitbucketVersionControlHandler(configuration),
            _ => throw new NotSupportedException(
                $"Proveedor de control de versiones '{providerType}' no soportado. " +
                $"Proveedores válidos: {string.Join(", ", GetSupportedProviders())}")
        };
    }

    /// <summary>
    /// Obtiene la lista de proveedores soportados
    /// </summary>
    public static IEnumerable<string> GetSupportedProviders()
    {
        return new[] { "SVN", "Git", "GitHub" };
        // Futuro: return new[] { "SVN", "Git", "GitHub", "GitLab", "Bitbucket", "Mercurial", "TFS", "Perforce" };
    }

    /// <summary>
    /// Verifica si un proveedor está soportado.
    /// </summary>
    public static bool IsProviderSupported(string provider)
    {
        return GetSupportedProviders()
            .Select(p => p.ToLowerInvariant())
            .Contains(provider.ToLowerInvariant());
    }
}

# ?? Sistema de Control de Versiones - Arquitectura Genérica

## ?? Descripción

El sistema de control de versiones ha sido refactorizado siguiendo el mismo patrón arquitectónico que el sistema de base de datos, permitiendo soportar múltiples proveedores de control de versiones (SVN, Git, Mercurial, TFS, etc.) de manera extensible y mantenible.

---

## ??? Arquitectura

### Componentes Principales

```
AgentWikiChat/Services/VersionControl/
??? IVersionControlHandler.cs              # Interfaz genérica
??? BaseVersionControlHandler.cs           # Clase base abstracta
??? SvnVersionControlHandler.cs            # Implementación SVN
??? GitVersionControlHandler.cs            # Implementación Git
??? GitHubVersionControlHandler.cs         # Implementación GitHub (API REST) ??
??? VersionControlHandlerFactory.cs        # Factory para crear handlers

AgentWikiChat/Services/Handlers/
??? RepositoryToolHandler.cs               # Handler genérico (renombrado)
```

### Diagrama de Arquitectura

```
???????????????????????????????????????
?   RepositoryToolHandler             ?  ? Handler del Tool (registrado en DI)
?   (IToolHandler)                    ?     Nombre genérico, soporte multi-proveedor
???????????????????????????????????????
               ? usa
               ?
???????????????????????????????????????
?  VersionControlHandlerFactory       ?  ? Factory para crear instancias
???????????????????????????????????????
               ? crea
               ?
???????????????????????????????????????
?   IVersionControlHandler            ?  ? Interfaz genérica
???????????????????????????????????????
? • IsClientInstalled()               ?
? • GetClientVersion()                ?
? • TestConnectionAsync()             ?
? • ExecuteReadOnlyOperationAsync()   ?
? • IsOperationAllowed()              ?
? • GetAllowedOperations()            ?
? • GetInstallationInstructions()     ?
? • GetErrorSuggestions()             ?
???????????????????????????????????????
               ? implementa
               ?
???????????????????????????????????????
?  BaseVersionControlHandler          ?  ? Clase base con funcionalidad común
?  (abstract)                         ?
???????????????????????????????????????
? • Logging helpers                   ?
? • Formatting helpers                ?
? • Configuration loading             ?
???????????????????????????????????????
               ? hereda
               ????????????????????????????????
               ?                              ?
??????????????????????????????????????? ???????????????????????????????????????
?  SvnVersionControlHandler           ? ?  GitVersionControlHandler           ?
?  (concrete)                         ? ?  (concrete)                         ?
??????????????????????????????????????? ???????????????????????????????????????
? • BuildSvnCommand()                 ? ? • BuildGitCommand()                 ?
? • ParseSvnVersion()                 ? ? • Process.Start("git")              ?
? • SVN-specific logic                ? ? • Git-specific logic                ?
??????????????????????????????????????? ???????????????????????????????????????
               ?
               ?
???????????????????????????????????????
?  GitHubVersionControlHandler ??     ?
?  (concrete)                         ?
???????????????????????????????????????
? • HttpClient for GitHub API         ?
? • REST API v3 calls                 ?
? • No local client required          ?
? • GitHub-specific logic             ?
???????????????????????????????????????
```

---

## ?? Beneficios de la Nueva Arquitectura

### 1. **Separación de Responsabilidades**
- **Handler genérico** (`SVNRepositoryToolHandler`): Solo se encarga de la integración con el sistema de tools
- **Handler específico** (`SvnVersionControlHandler`): Contiene toda la lógica específica de SVN
- **Factory**: Gestiona la creación de instancias según el proveedor

### 2. **Extensibilidad**
Para agregar soporte para **Git**, **Mercurial**, etc., solo necesitas:
1. Crear `GitVersionControlHandler : BaseVersionControlHandler`
2. Implementar los métodos abstractos
3. Agregar el caso en `VersionControlHandlerFactory`

### 3. **Reutilización de Código**
- Logging, formateo y manejo de configuración están centralizados en `BaseVersionControlHandler`
- No hay duplicación de código entre proveedores

### 4. **Facilidad de Testing**
- Cada componente puede ser testeado independientemente
- Se pueden crear mocks de `IVersionControlHandler` fácilmente

### 5. **Configuración Unificada**
Todos los proveedores usan la misma estructura de configuración:
```json
{
  "Provider": "SVN",
  "RepositoryUrl": "...",
  "Username": "...",
  "Password": "...",
  "WorkingCopyPath": "...",
  "CommandTimeout": 60,
  "EnableLogging": true
}
```

---

## ?? Configuración

### Ejemplo: SVN (Actual)

```json
{
  "SVN": {
    "Provider": "SVN",
    "RepositoryUrl": "https://192.168.100.15/svn/RepDeTesting",
    "Username": "ffontanini",
    "Password": "12345678",
    "WorkingCopyPath": "C:\\Users\\ffontanini\\Music\\RepDeTesting",
    "CommandTimeout": 60,
    "EnableLogging": true
  }
}
```

### Ejemplo: Git (Futuro)

```json
{
  "Git": {
    "Provider": "Git",
    "RepositoryUrl": "https://github.com/company/project.git",
    "Username": "myuser",
    "Password": "ghp_token_here",
    "WorkingCopyPath": "C:\\Projects\\MyProject",
    "CommandTimeout": 60,
    "EnableLogging": true
  }
}
```

---

## ?? Cómo Agregar un Nuevo Proveedor

### Paso 1: Crear la Implementación

Crea `GitVersionControlHandler.cs`:

```csharp
public class GitVersionControlHandler : BaseVersionControlHandler
{
    public override string ProviderName => "Git";

    public GitVersionControlHandler(IConfiguration configuration)
        : base(configuration, "Git")
    {
        // Inicialización específica de Git
    }

    public override bool IsClientInstalled()
    {
        // Verificar si git está instalado
        // Ejecutar: git --version
    }

    public override string GetClientVersion()
    {
        // Obtener versión de git
    }

    public override async Task<bool> TestConnectionAsync()
    {
        // Probar conexión con: git ls-remote
    }

    public override async Task<string> ExecuteReadOnlyOperationAsync(
        string operation, 
        Dictionary<string, string> parameters)
    {
        // Ejecutar comando git según la operación
        // Ej: git log, git show, git ls-tree, etc.
    }

    public override bool IsOperationAllowed(string operation)
    {
        // Validar operaciones permitidas (log, show, ls-tree, blame, etc.)
    }

    public override IEnumerable<string> GetAllowedOperations()
    {
        return new[] { "log", "show", "ls-tree", "blame", "diff" };
    }

    public override string GetInstallationInstructions()
    {
        // Instrucciones para instalar Git
    }

    public override string GetErrorSuggestions(string errorMessage)
    {
        // Sugerencias específicas de Git
    }
}
```

### Paso 2: Registrar en el Factory

Edita `VersionControlHandlerFactory.cs`:

```csharp
public static IVersionControlHandler CreateHandler(IConfiguration configuration, string configSection)
{
    var config = configuration.GetSection(configSection);
    var provider = config.GetValue<string>("Provider") ?? "SVN";

    return provider.ToUpperInvariant() switch
    {
        "SVN" or "SUBVERSION" => new SvnVersionControlHandler(configuration),
        "GIT" => new GitVersionControlHandler(configuration),  // ? AGREGAR AQUÍ
        _ => throw new NotSupportedException($"Proveedor '{provider}' no soportado.")
    };
}

public static IEnumerable<string> GetSupportedProviders()
{
    return new[] { "SVN", "Git" };  // ? ACTUALIZAR AQUÍ
}
```

### Paso 3: Configurar en appsettings.json

```json
{
  "Git": {
    "Provider": "Git",
    "RepositoryUrl": "https://github.com/company/project.git",
    "Username": "myuser",
    "Password": "ghp_token",
    "CommandTimeout": 60,
    "EnableLogging": true
  }
}
```

### Paso 4: (Opcional) Crear Tool Handler Específico

Si quieres un handler separado para Git:

```csharp
public class GitRepositoryToolHandler : IToolHandler
{
    private readonly IVersionControlHandler _versionControlHandler;

    public GitRepositoryToolHandler(IConfiguration configuration)
    {
        _versionControlHandler = VersionControlHandlerFactory.CreateHandler(configuration, "Git");
    }

    public string ToolName => "git_operation";

    // ... resto de la implementación similar a SVNRepositoryToolHandler
}
```

Y registrarlo en `Program.cs`:

```csharp
services.AddSingleton<IToolHandler>(sp => new GitRepositoryToolHandler(sp.GetRequiredService<IConfiguration>()));
```

---

## ?? Seguridad

### Validación de Operaciones

Cada proveedor implementa su propia lista de operaciones permitidas y prohibidas:

**SVN:**
- ? Permitido: `log`, `info`, `list`, `cat`, `diff`, `blame`, `status`
- ? Prohibido: `commit`, `delete`, `update`, `add`, etc.

**Git (Futuro):**
- ? Permitido: `log`, `show`, `ls-tree`, `blame`, `diff`
- ? Prohibido: `commit`, `push`, `pull`, `add`, `rm`, etc.

### Implementación

```csharp
public override bool IsOperationAllowed(string operation)
{
    var normalizedOp = operation.Trim().ToLowerInvariant();
    
    if (ProhibitedCommands.Contains(normalizedOp))
        return false;
    
    return AllowedCommands.Contains(normalizedOp);
}
```

---

## ?? Comparación con Arquitectura Anterior

### ? Antes (Monolítico)

```
SVNRepositoryToolHandler
??? Toda la lógica SVN específica
??? Manejo de herramientas
??? Validación
??? Formateo
??? Logging
```

**Problemas:**
- ? Difícil agregar nuevos proveedores (Git, Mercurial)
- ? Mucha duplicación de código si se agregan proveedores
- ? Acoplamiento fuerte
- ? Difícil de testear

### ? Ahora (Modular)

```
SVNRepositoryToolHandler (genérico)
    ?
    ??? IVersionControlHandler (interfaz)
            ?
            ??? BaseVersionControlHandler (común)
            ?       ?
            ?       ??? SvnVersionControlHandler (específico SVN)
            ?       ??? GitVersionControlHandler (específico Git)
            ?       ??? MercurialVersionControlHandler (específico Mercurial)
            ?
            ??? VersionControlHandlerFactory (creación)
```

**Beneficios:**
- ? Fácil agregar nuevos proveedores
- ? Código reutilizable
- ? Bajo acoplamiento
- ? Fácil de testear
- ? Configuración unificada

---

## ?? Testing

### Ejemplo de Test Unitario

```csharp
[Fact]
public void SvnHandler_ShouldRejectCommitOperation()
{
    // Arrange
    var configuration = CreateMockConfiguration();
    var handler = new SvnVersionControlHandler(configuration);

    // Act
    var isAllowed = handler.IsOperationAllowed("commit");

    // Assert
    Assert.False(isAllowed);
}

[Fact]
public void SvnHandler_ShouldAllowLogOperation()
{
    // Arrange
    var configuration = CreateMockConfiguration();
    var handler = new SvnVersionControlHandler(configuration);

    // Act
    var isAllowed = handler.IsOperationAllowed("log");

    // Assert
    Assert.True(isAllowed);
}
```

---

## ?? Interfaz `IVersionControlHandler`

### Métodos Principales

| Método | Descripción | Retorno |
|--------|-------------|---------|
| `ProviderName` | Nombre del proveedor | `string` |
| `IsClientInstalled()` | Verifica si el cliente está instalado | `bool` |
| `GetClientVersion()` | Obtiene versión del cliente | `string` |
| `TestConnectionAsync()` | Prueba conexión con repositorio | `Task<bool>` |
| `ExecuteReadOnlyOperationAsync()` | Ejecuta operación de lectura | `Task<string>` |
| `IsOperationAllowed()` | Valida si operación es permitida | `bool` |
| `GetAllowedOperations()` | Lista operaciones permitidas | `IEnumerable<string>` |
| `GetInstallationInstructions()` | Instrucciones de instalación | `string` |
| `GetErrorSuggestions()` | Sugerencias según error | `string` |

---

## ?? Roadmap

### Fase 1: ? Completado
- [x] Crear arquitectura genérica
- [x] Migrar lógica SVN a implementación específica
- [x] Crear factory y clase base
- [x] Documentación

### Fase 2: ?? Futuro
- [ ] Implementar `GitVersionControlHandler`
- [ ] Crear tests unitarios
- [ ] Agregar soporte para múltiples repositorios simultáneos
- [ ] Implementar `MercurialVersionControlHandler`
- [ ] Implementar `TfsVersionControlHandler`

### Fase 3: ?? Ideas
- [ ] Crear interfaz web para visualizar repositorios
- [ ] Integración con GitHub/GitLab/Bitbucket APIs
- [ ] Caché de operaciones frecuentes
- [ ] Soporte para autenticación con tokens/SSH

---

## ?? Referencias

- [Patrón Factory](https://refactoring.guru/design-patterns/factory-method)
- [Principio de Inversión de Dependencias (DIP)](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

---

## ?? Contribuciones

Para agregar soporte para un nuevo proveedor de control de versiones:

1. **Fork** el repositorio
2. **Crea** una rama: `feature/add-git-support`
3. **Implementa** `YourProviderVersionControlHandler : BaseVersionControlHandler`
4. **Actualiza** `VersionControlHandlerFactory`
5. **Agrega tests** unitarios
6. **Actualiza** documentación
7. **Crea** Pull Request

---

**Última actualización**: v3.5.0  
**Autor**: AgentWikiChat Team

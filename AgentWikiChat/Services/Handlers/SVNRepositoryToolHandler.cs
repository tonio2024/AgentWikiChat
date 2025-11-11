using AgentWikiChat.Models;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AgentWikiChat.Services.Handlers;

/// <summary>
/// Handler para operaciones con repositorios SVN.
/// SEGURIDAD: Solo permite operaciones de lectura. Bloqueado: commit, delete, update, etc.
/// Usa el cliente SVN de línea de comandos (svn.exe) que debe estar instalado en el sistema.
/// </summary>
public class SVNRepositoryToolHandler : IToolHandler
{
    private readonly string _repositoryUrl;
    private readonly string _username;
    private readonly string _password;
    private readonly string _workingCopyPath;
    private readonly int _commandTimeout;
    private readonly bool _enableLogging;
    private readonly bool _debugMode = true;
    private static bool? _svnInstalled = null;
    private static string? _svnVersion = null;
    private static int _svnMajorVersion = 0;

    // Comandos permitidos (solo lectura)
    private static readonly string[] AllowedCommands = new[]
    {
        "log", "info", "list", "cat", "diff", "blame", "status"
    };

    // Comandos prohibidos (escritura/modificación)
    private static readonly string[] ProhibitedCommands = new[]
    {
        "commit", "ci", "delete", "del", "remove", "rm", 
        "add", "checkout", "co", "update", "up", "switch", 
        "merge", "copy", "cp", "move", "mv", "mkdir",
        "import", "export", "propdel", "propset", "lock", "unlock"
    };

    public SVNRepositoryToolHandler(IConfiguration configuration)
    {
        var svnConfig = configuration.GetSection("SVN");
        
        _repositoryUrl = svnConfig.GetValue<string>("RepositoryUrl") 
            ?? throw new InvalidOperationException("SVN:RepositoryUrl no configurada en appsettings.json");
        
        _username = svnConfig.GetValue<string>("Username") ?? "";
        _password = svnConfig.GetValue<string>("Password") ?? "";
        _workingCopyPath = svnConfig.GetValue<string>("WorkingCopyPath") ?? "";
        _commandTimeout = svnConfig.GetValue("CommandTimeout", 60);
        _enableLogging = svnConfig.GetValue("EnableLogging", true);

        // Verificar si SVN está instalado (solo una vez)
        if (_svnInstalled == null)
        {
            _svnInstalled = CheckSvnInstalled();
            if (_svnInstalled == true)
            {
                _svnVersion = GetSvnVersion();
                if (!string.IsNullOrEmpty(_svnVersion))
                {
                    ParseSvnVersion(_svnVersion);
                    LogDebug($"[SVN] Cliente SVN detectado - v{_svnVersion} (Major: {_svnMajorVersion})");
                }
            }
            else
            {
                LogError($"[SVN] ?? Cliente SVN no encontrado en el sistema");
            }
        }

        LogDebug($"[SVN] Inicializado - URL: {_repositoryUrl}, Timeout: {_commandTimeout}s");
        
        // Diagnóstico inicial
        if (_svnInstalled == true)
        {
            TestRepositoryConnection();
        }
    }

    public string ToolName => "svn_operation";

    public ToolDefinition GetToolDefinition()
    {
        return new ToolDefinition
        {
            Type = "function",
            Function = new FunctionDefinition
            {
                Name = ToolName,
                Description = "Ejecuta operaciones de SOLO LECTURA en repositorios SVN (log, info, list, cat, diff, blame, status). NO permite modificaciones (commit, delete, add, update). Usa esta herramienta para consultar historial, ver archivos, obtener información del repositorio.",
                Parameters = new FunctionParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, PropertyDefinition>
                    {
                        ["operation"] = new PropertyDefinition
                        {
                            Type = "string",
                            Description = "Operación SVN a ejecutar: 'log' (historial), 'info' (informaci), 'list' (listar archivos), 'cat' (ver contenido), 'diff' (diferencias), 'blame' (autoría), 'status' (estado)",
                            Enum = new List<string> { "log", "info", "list", "cat", "diff", "blame", "status" }
                        },
                        ["path"] = new PropertyDefinition
                        {
                            Type = "string",
                            Description = "Ruta del archivo o directorio en el repositorio (opcional, por defecto raíz). Ejemplo: '/trunk/src/MyFile.cs'"
                        },
                        ["revision"] = new PropertyDefinition
                        {
                            Type = "string",
                            Description = "Número de revisión o rango (ej: '1234', 'HEAD', '1000:1100'). Por defecto HEAD."
                        },
                        ["limit"] = new PropertyDefinition
                        {
                            Type = "string",
                            Description = "Límite de resultados para comandos como log (ej: '10', '50'). Por defecto 10."
                        }
                    },
                    Required = new List<string> { "operation" }
                }
            }
        };
    }

    public async Task<string> HandleAsync(ToolParameters parameters, MemoryService memory)
    {
        var operation = parameters.GetString("operation");
        var path = parameters.GetString("path", "");
        var revision = parameters.GetString("revision", "HEAD");
        var limit = parameters.GetString("limit", "10");

        if (string.IsNullOrWhiteSpace(operation))
        {
            return "?? Error: La operación SVN no puede estar vacía.";
        }

        // Verificar si SVN está instalado
        if (_svnInstalled == false)
        {
            return GetSvnNotInstalledMessage();
        }

        LogDebug($"[SVN] Operación recibida: {operation}, Path: {path}, Revision: {revision}");

        // ?? VALIDACIÓN DE SEGURIDAD
        var validationResult = ValidateOperation(operation);
        if (!validationResult.IsValid)
        {
            LogError($"[SVN] Operación rechazada: {validationResult.ErrorMessage}");
            return $"?? **Operación Rechazada por Seguridad**\n\n{validationResult.ErrorMessage}\n\n" +
                   $"?? **Recuerda**: Solo se permiten operaciones de solo lectura (log, info, list, cat, diff, blame, status).";
        }

        try
        {
            // Ejecutar operación SVN
            var result = await ExecuteSvnOperationAsync(operation, path, revision, limit);
            
            // Guardar en memoria modular
            memory.AddToModule("svn", "system", $"SVN {operation} ejecutado: {path} @ {revision}");

            return FormatSvnResult(operation, result, path, revision);
        }
        catch (Exception ex)
        {
            LogError($"[SVN] Error: {ex.Message}");
            
            // Error específico si SVN no se encuentra
            if (ex.Message.Contains("cannot find the file") || ex.Message.Contains("no puede encontrar el archivo"))
            {
                return GetSvnNotInstalledMessage();
            }

            // Mensajes de error más descriptivos
            var errorMessage = ex.Message;
            var suggestions = GetErrorSuggestions(errorMessage);
            
            return $"? **Error en SVN**\n\n" +
                   $"**Mensaje**: {errorMessage}\n\n" +
                   suggestions;
        }
    }

    #region SVN Detection & Diagnostics

    private bool CheckSvnInstalled()
    {
        try
        {
            var version = GetSvnVersion();
            return !string.IsNullOrEmpty(version);
        }
        catch
        {
            return false;
        }
    }

    private string GetSvnVersion()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "svn",
                Arguments = "--version --quiet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null) return "";

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(5000);

            return output;
        }
        catch
        {
            return "";
        }
    }

    private void ParseSvnVersion(string versionString)
    {
        try
        {
            // Version format: "1.14.1" or similar
            var parts = versionString.Split('.');
            if (parts.Length > 0 && int.TryParse(parts[0], out var major))
            {
                _svnMajorVersion = major;
            }
        }
        catch
        {
            _svnMajorVersion = 1; // Default to 1.x
        }
    }

    private void TestRepositoryConnection()
    {
        try
        {
            LogDebug($"[SVN] Probando conexión con {_repositoryUrl}...");
            
            var args = new List<string> { "info", $"\"{_repositoryUrl}\"" };
            
            // Agregar credenciales
            if (!string.IsNullOrEmpty(_username))
            {
                args.Add($"--username \"{_username}\"");
                if (!string.IsNullOrEmpty(_password))
                    args.Add($"--password \"{_password}\"");
            }
            
            args.Add("--non-interactive");
            
            // Solo usar trust-server-cert en versiones 1.9+
            if (_svnMajorVersion >= 1)
            {
                args.Add("--trust-server-cert");
            }

            var command = string.Join(" ", args);
            
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "svn",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null) return;

            process.WaitForExit(10000);
            
            if (process.ExitCode == 0)
            {
                LogDebug($"[SVN] ? Conexión exitosa con el repositorio");
            }
            else
            {
                var error = process.StandardError.ReadToEnd();
                LogWarning($"[SVN] ?? Problema al conectar: {error.Substring(0, Math.Min(200, error.Length))}");
            }
        }
        catch (Exception ex)
        {
            LogWarning($"[SVN] ?? No se pudo verificar conexión: {ex.Message}");
        }
    }

    private string GetErrorSuggestions(string errorMessage)
    {
        var suggestions = new StringBuilder();
        suggestions.AppendLine("?? **Posibles soluciones:**\n");

        if (errorMessage.Contains("E170013") || errorMessage.Contains("Unable to connect"))
        {
            suggestions.AppendLine("**Problema de conexión detectado:**");
            suggestions.AppendLine("1. ? Verifica que la URL sea correcta: `" + _repositoryUrl + "`");
            suggestions.AppendLine("2. ?? Verifica conectividad de red al servidor (ping, firewall)");
            suggestions.AppendLine("3. ?? Verifica credenciales (usuario/password)");
            suggestions.AppendLine("4. ?? El servidor puede requerir certificado SSL válido");
            suggestions.AppendLine("5. ?? Intenta usar una working copy local si tienes una (configura `WorkingCopyPath`)");
            suggestions.AppendLine();
            suggestions.AppendLine("**Prueba manual:**");
            suggestions.AppendLine("```bash");
            suggestions.AppendLine($"svn info {_repositoryUrl} --username {_username} --password [tu-password]");
            suggestions.AppendLine("```");
        }
        else if (errorMessage.Contains("E120112") || errorMessage.Contains("APR does not understand"))
        {
            suggestions.AppendLine("**Error de APR (Apache Portable Runtime):**");
            suggestions.AppendLine("1. ?? Puede ser un problema temporal del servidor");
            suggestions.AppendLine("2. ?? Reinstala el cliente SVN (TortoiseSVN con command line tools)");
            suggestions.AppendLine("3. ?? Problema con autenticación guardada - limpia cache:");
            suggestions.AppendLine("   - Windows: Elimina `%APPDATA%\\Subversion\\auth`");
            suggestions.AppendLine("   - Linux/Mac: Elimina `~/.subversion/auth`");
        }
        else if (errorMessage.Contains("E170001") || errorMessage.Contains("authorization"))
        {
            suggestions.AppendLine("**Problema de autenticación:**");
            suggestions.AppendLine("1. ?? Verifica usuario/contraseña en appsettings.json");
            suggestions.AppendLine("2. ?? Verifica que el usuario tenga permisos de lectura");
            suggestions.AppendLine("3. ?? Intenta autenticarte manualmente primero con `svn info`");
        }
        
        suggestions.AppendLine("\n?? Si el problema persiste, considera usar una working copy local.");
        
        return suggestions.ToString();
    }

    private string GetSvnNotInstalledMessage()
    {
        var message = new StringBuilder();
        message.AppendLine("? **Cliente SVN No Encontrado**\n");
        message.AppendLine("El sistema no puede encontrar el ejecutable `svn` (Subversion client).\n");
        message.AppendLine("**?? Soluciones:**\n");
        message.AppendLine("**Windows:**");
        message.AppendLine("1. Instalar TortoiseSVN: https://tortoisesvn.net/");
        message.AppendLine("   - Durante la instalación, marcar: *\"command line client tools\"*");
        message.AppendLine("2. O instalar Apache Subversion: https://subversion.apache.org/packages.html");
        message.AppendLine("3. Agregar al PATH: `C:\\Program Files\\TortoiseSVN\\bin`");
        message.AppendLine("4. Reiniciar la aplicación\n");
        message.AppendLine("**Linux (Ubuntu/Debian):**");
        message.AppendLine("```bash");
        message.AppendLine("sudo apt-get update");
        message.AppendLine("sudo apt-get install subversion");
        message.AppendLine("```\n");
        message.AppendLine("**Linux (CentOS/RHEL):**");
        message.AppendLine("```bash");
        message.AppendLine("sudo yum install subversion");
        message.AppendLine("```\n");
        message.AppendLine("**macOS:**");
        message.AppendLine("```bash");
        message.AppendLine("brew install svn");
        message.AppendLine("```\n");
        message.AppendLine("**? Verificar instalación:**");
        message.AppendLine("```bash");
        message.AppendLine("svn --version");
        message.AppendLine("```\n");
        message.AppendLine("?? Después de instalar, reinicia esta aplicación.");
        
        return message.ToString();
    }

    #endregion

    #region Validation

    private ValidationResult ValidateOperation(string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
        {
            return ValidationResult.Fail("La operación está vacía.");
        }

        var normalizedOp = operation.Trim().ToLowerInvariant();

        if (!AllowedCommands.Contains(normalizedOp))
        {
            return ValidationResult.Fail($"Operación '{operation}' no está en la lista de comandos permitidos. " +
                $"Comandos válidos: {string.Join(", ", AllowedCommands)}");
        }

        if (ProhibitedCommands.Contains(normalizedOp))
        {
            return ValidationResult.Fail($"Operación '{operation}' está prohibida. Solo se permiten operaciones de solo lectura.");
        }

        return ValidationResult.Success();
    }

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        public static ValidationResult Fail(string message) => new ValidationResult { IsValid = false, ErrorMessage = message };
    }

    #endregion

    #region SVN Execution

    private async Task<string> ExecuteSvnOperationAsync(string operation, string path, string revision, string limit)
    {
        var svnCommand = BuildSvnCommand(operation, path, revision, limit);
        
        LogDebug($"[SVN] Ejecutando: svn {svnCommand}");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "svn",
            Arguments = svnCommand,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = processStartInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var completed = await Task.Run(() => process.WaitForExit(_commandTimeout * 1000));

        if (!completed)
        {
            process.Kill();
            throw new TimeoutException($"La operación SVN excedió el timeout de {_commandTimeout} segundos.");
        }

        if (process.ExitCode != 0)
        {
            var error = errorBuilder.ToString().Trim();
            throw new InvalidOperationException($"SVN retornó código de error {process.ExitCode}: {error}");
        }

        var output = outputBuilder.ToString().Trim();
        
        if (string.IsNullOrEmpty(output))
        {
            return "? La operación se completó exitosamente pero no retornó datos.";
        }

        return output;
    }

    private string BuildSvnCommand(string operation, string path, string revision, string limit)
    {
        var args = new List<string> { operation };

        // Si hay working copy configurada, úsala para algunas operaciones
        var useWorkingCopy = !string.IsNullOrEmpty(_workingCopyPath) && 
                           Directory.Exists(_workingCopyPath) &&
                           (operation.ToLowerInvariant() == "status" || operation.ToLowerInvariant() == "info");

        string targetUrl;
        
        if (useWorkingCopy)
        {
            targetUrl = string.IsNullOrEmpty(path)
                ? _workingCopyPath
                : Path.Combine(_workingCopyPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        }
        else
        {
            targetUrl = string.IsNullOrEmpty(path) 
                ? _repositoryUrl 
                : $"{_repositoryUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        }

        switch (operation.ToLowerInvariant())
        {
            case "log":
                args.Add($"\"{targetUrl}\"");
                args.Add($"-r {revision}");
                if (int.TryParse(limit, out var logLimit))
                    args.Add($"-l {logLimit}");
                args.Add("-v");
                break;

            case "info":
                args.Add($"\"{targetUrl}\"");
                if (revision != "HEAD")
                    args.Add($"-r {revision}");
                break;

            case "list":
            case "ls":
                args.Add($"\"{targetUrl}\"");
                if (revision != "HEAD")
                    args.Add($"-r {revision}");
                args.Add("-v");
                break;

            case "cat":
                args.Add($"\"{targetUrl}\"");
                if (revision != "HEAD")
                    args.Add($"-r {revision}");
                break;

            case "diff":
                args.Add($"\"{targetUrl}\"");
                if (revision.Contains(":"))
                    args.Add($"-r {revision}");
                else if (revision != "HEAD")
                    args.Add($"-r {revision}:HEAD");
                break;

            case "blame":
            case "praise":
            case "annotate":
                args.Add($"\"{targetUrl}\"");
                if (revision != "HEAD")
                    args.Add($"-r {revision}");
                break;

            case "status":
                // Ya manejado arriba con useWorkingCopy
                break;
        }

        // Agregar credenciales si no estamos usando working copy
        if (!useWorkingCopy && !string.IsNullOrEmpty(_username))
        {
            args.Add($"--username \"{_username}\"");
            if (!string.IsNullOrEmpty(_password))
                args.Add($"--password \"{_password}\"");
        }

        // Opciones de autenticación
        args.Add("--non-interactive");
        
        // Usar flag apropiado según versión de SVN
        if (_svnMajorVersion >= 1)
        {
            // Para SVN 1.9+ usar --trust-server-cert
            // Para SVN 1.6+ usar solo --trust-server-cert
            args.Add("--trust-server-cert");
        }

        return string.Join(" ", args);
    }

    #endregion

    #region Formatting

    private string FormatSvnResult(string operation, string result, string path, string revision)
    {
        var output = new StringBuilder();

        output.AppendLine($"?? **Resultado de SVN - {operation.ToUpper()}**\n");
        output.AppendLine($"**Repositorio**: `{_repositoryUrl}`");
        
        if (!string.IsNullOrEmpty(path))
            output.AppendLine($"**Path**: `{path}`");
        
        if (revision != "HEAD")
            output.AppendLine($"**Revisión**: `{revision}`");

        output.AppendLine();
        output.AppendLine("**Resultado:**");
        output.AppendLine("```");
        output.AppendLine(TruncateForDisplay(result, 5000));
        output.AppendLine("```");

        return output.ToString();
    }

    #endregion

    #region Logging Helpers

    private void LogDebug(string message)
    {
        if (_debugMode && _enableLogging)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] {message}");
            Console.ResetColor();
        }
    }

    private void LogWarning(string message)
    {
        if (_debugMode && _enableLogging)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARN] {message}");
            Console.ResetColor();
        }
    }

    private void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    private string TruncateForDisplay(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "\n\n... (truncado)";
    }

    #endregion
}

# ?? GitHub Repository Tool - Documentación

## Descripción

La herramienta `github_operation` permite al agente ejecutar operaciones de **solo lectura** en repositorios GitHub usando la **API REST v3** de manera segura y sin necesidad de cliente local.

**? Operaciones Soportadas**: log, show, list, cat, diff, blame, branches, tags, info

---

## ?? Ventajas de GitHub API

### vs Git Local
| Característica | GitHub API | Git Local |
|----------------|------------|-----------|
| **Cliente requerido** | ? No | ? Sí (git) |
| **Clonar repo** | ? No | ? Sí |
| **Espacio en disco** | Mínimo | Tamaño del repo |
| **Velocidad inicial** | Instantánea | Depende del tamaño |
| **Repos privados** | ? Con token | ? Con credenciales |
| **Límite de uso** | 5,000 req/h | Ilimitado |
| **Información adicional** | Stars, forks, issues | Solo commits |

### Casos de Uso Ideales
- ? Consultas rápidas sin clonar
- ? Múltiples repositorios diferentes
- ? CI/CD y automatización
- ? Análisis de metadatos (stars, issues)
- ? Repositorios de solo lectura

---

## ?? Seguridad

### Operaciones Permitidas (Solo Lectura)
- ? `log` - Ver historial de commits
- ? `show` - Detalles de un commit específico
- ? `list` - Listar archivos y directorios
- ? `cat` - Ver contenido de archivos
- ? `diff` - Ver diferencias en un commit
- ? `blame` - Información de autoría
- ? `branches` - Listar ramas
- ? `tags` - Listar tags
- ? `info` - Información completa del repositorio

### Operaciones Prohibidas (Escritura/Modificación)
- ? `commit` - Crear commits
- ? `push` - Subir cambios
- ? `pull` - Fusionar cambios
- ? `merge` - Fusionar ramas
- ? `create` - Crear recursos
- ? `delete` - Eliminar recursos
- ? `update` - Actualizar recursos
- ? `fork` - Crear fork

---

## ?? Configuración

### appsettings.json

```json
{
  "GitHub": {
    "Provider": "GitHub",
    "RepositoryUrl": "https://github.com/owner/repo",
    "Username": "your-github-username",
    "Password": "ghp_YourPersonalAccessToken",
    "Branch": "main",
    "CommandTimeout": 30,
    "EnableLogging": true
  }
}
```

### Parámetros

| Parámetro | Descripción | Obligatorio | Default |
|-----------|-------------|-------------|---------|
| `Provider` | Tipo de proveedor | **Sí** | - |
| `RepositoryUrl` | URL del repositorio GitHub | **Sí** | - |
| `Username` | Usuario de GitHub | No | "" |
| `Password` | Personal Access Token | **Sí (privados)** | "" |
| `Branch` | Rama predeterminada | No | "main" |
| `CommandTimeout` | Timeout en segundos | No | 30 |
| `EnableLogging` | Habilita logs | No | true |

---

## ?? Obtener Personal Access Token

### Paso 1: Crear Token

1. Ve a: **https://github.com/settings/tokens**
2. Click en **"Generate new token (classic)"**
3. Asigna un nombre descriptivo (ej: "AgentWikiChat")
4. Selecciona expiration (ej: 90 días, 1 año, sin expiración)
5. Selecciona **scopes**:
   - **`repo`** - Acceso completo a repos privados
   - **`public_repo`** - Solo repos públicos (más restrictivo)
6. Click en **"Generate token"**
7. **Copia el token** (comienza con `ghp_`)

**?? Importante**: El token solo se muestra una vez. Guárdalo en lugar seguro.

### Paso 2: Configurar Token

```json
{
  "GitHub": {
    "Password": "ghp_1234567890abcdefghijklmnopqrstuvwxyz"
  }
}
```

### Scopes Recomendados

| Scope | Descripción | Uso |
|-------|-------------|-----|
| `public_repo` | Repos públicos | Lectura de repos públicos |
| `repo` | Repos completos | Lectura de repos privados |
| `read:org` | Lectura de org | Repos de organizaciones |

**Recomendación**: Usa el scope mínimo necesario (principio de menor privilegio).

---

## ?? Ejemplos de Uso

### Ejemplo 1: Ver Últimos Commits

**Consulta:**
```
Usuario: "Muestra los últimos 10 commits del repositorio"
```

**Tool Call:**
```json
{
  "operation": "log",
  "limit": "10"
}
```

**Resultado:**
```
?? Últimos 10 commits:

?? a1b2c3d - John Doe
   ?? 2025-01-15T10:30:00Z
   ?? Fix authentication bug

?? e4f5g6h - Jane Smith
   ?? 2025-01-14T15:45:00Z
   ?? Add new feature
...
```

---

### Ejemplo 2: Listar Archivos

**Consulta:**
```
Usuario: "Lista los archivos en la carpeta src/"
```

**Tool Call:**
```json
{
  "operation": "list",
  "path": "src"
}
```

**Resultado:**
```
?? Contenido de src/:

?? Main.cs (2048 bytes)
?? Config.cs (1024 bytes)
?? Models
?? Services
?? README.md (512 bytes)
```

---

### Ejemplo 3: Ver Contenido de Archivo

**Consulta:**
```
Usuario: "Muestra el contenido del archivo README.md"
```

**Tool Call:**
```json
{
  "operation": "cat",
  "path": "README.md"
}
```

**Resultado:**
```
# My Project

This is a sample project...
[contenido completo del archivo]
```

---

### Ejemplo 4: Información del Repositorio

**Consulta:**
```
Usuario: "Dame información del repositorio"
```

**Tool Call:**
```json
{
  "operation": "info"
}
```

**Resultado:**
```
?? Información del repositorio owner/repo:

**Nombre**: owner/repo
**Descripción**: A sample repository for demonstration
**Visibilidad**: ?? Público
**Rama predeterminada**: main
**Lenguaje principal**: C#
**Tamaño**: 15360 KB
**Stars**: ? 42
**Forks**: ?? 8
**Issues abiertos**: ?? 3
**Creado**: 2023-01-01T00:00:00Z
**Última actualización**: 2025-01-15T12:00:00Z
**URL**: https://github.com/owner/repo
```

---

### Ejemplo 5: Ver Ramas

**Consulta:**
```
Usuario: "Lista todas las ramas del repositorio"
```

**Tool Call:**
```json
{
  "operation": "branches"
}
```

**Resultado:**
```
?? Ramas disponibles:

?? main
?? develop
?? feature/new-ui
?? hotfix/critical-bug
```

---

### Ejemplo 6: Detalles de un Commit

**Consulta:**
```
Usuario: "Muestra detalles del commit a1b2c3d"
```

**Tool Call:**
```json
{
  "operation": "show",
  "revision": "a1b2c3d"
}
```

**Resultado:**
```
?? Detalles del commit a1b2c3d:

**Autor**: John Doe
**Email**: john@example.com
**Fecha**: 2025-01-15T10:30:00Z
**Mensaje**: Fix authentication bug

**Archivos modificados**:
  modified: src/Auth.cs (+10/-5)
  added: tests/AuthTests.cs (+50/-0)
```

---

## ?? Límites de la API

### Sin Autenticación
- **60 requests/hora**
- Solo repos públicos
- IP-based rate limit

### Con Personal Access Token
- **5,000 requests/hora**
- Repos públicos y privados
- User-based rate limit

### Verificar Límites

```bash
curl -H "Authorization: Bearer YOUR_TOKEN" https://api.github.com/rate_limit
```

**Respuesta:**
```json
{
  "rate": {
    "limit": 5000,
    "remaining": 4999,
    "reset": 1234567890
  }
}
```

---

## ?? Troubleshooting

### Error 401: Unauthorized

**Causa**: Token inválido o expirado

**Solución**:
1. Verifica que el token sea correcto
2. Verifica que no haya expirado
3. Genera un nuevo token
4. Verifica los scopes del token

---

### Error 403: Forbidden / Rate Limit

**Causa**: Límite de API excedido

**Solución**:
1. Espera a que se resetee el límite
2. Usa un Personal Access Token (aumenta a 5,000/h)
3. Optimiza el número de llamadas

---

### Error 404: Not Found

**Causa**: Repositorio, rama o archivo no existe

**Solución**:
1. Verifica la URL del repositorio
2. Verifica que tengas acceso al repo privado
3. Verifica el path del archivo
4. Verifica el nombre de la rama

---

### Error: Timeout

**Causa**: Timeout de conexión

**Solución**:
1. Aumenta `CommandTimeout` en `appsettings.json`
2. Verifica conexión a internet
3. Verifica firewall/proxy

---

## ?? Comparación de Operaciones

| Operación | GitHub API | Git Local | SVN |
|-----------|------------|-----------|-----|
| **log** | ? commits | ? commits | ? commits |
| **show** | ? commit details | ? commit details | ? |
| **list** | ? tree | ? ls-tree | ? list |
| **cat** | ? contenido | ? show | ? cat |
| **diff** | ? patch | ? diff | ? diff |
| **blame** | ?? básico | ? línea por línea | ? blame |
| **branches** | ? todas | ? local+remoto | ? |
| **tags** | ? todos | ? todos | ? |
| **info** | ? metadatos | ? | ? info |

---

## ?? Mejores Prácticas

### Seguridad
1. ? Usa variables de entorno para el token
2. ? Rota tokens periódicamente
3. ? Usa scopes mínimos necesarios
4. ? No compartas tokens en código fuente
5. ? Revoca tokens que no uses

### Rendimiento
1. ? Limita el número de commits (`limit`)
2. ? Usa cache cuando sea posible
3. ? Monitorea rate limits
4. ? Agrupa operaciones relacionadas

### Configuración
1. ? Especifica `Branch` explícitamente
2. ? Ajusta `CommandTimeout` según necesidad
3. ? Habilita logging para debug
4. ? Documenta el propósito del token

---

## ?? Registro en Program.cs

### Opción 1: Solo GitHub

```csharp
services.AddSingleton<IToolHandler>(sp => 
    new RepositoryToolHandler(sp.GetRequiredService<IConfiguration>(), "GitHub"));
```

### Opción 2: GitHub + SVN

```csharp
// SVN
services.AddSingleton<IToolHandler>(sp => 
    new RepositoryToolHandler(sp.GetRequiredService<IConfiguration>(), "SVN"));

// GitHub
services.AddSingleton<IToolHandler>(sp => 
    new RepositoryToolHandler(sp.GetRequiredService<IConfiguration>(), "GitHub"));
```

**Resultado**: Tools registradas: `svn_operation` y `github_operation`

---

## ?? Recursos Adicionales

- [GitHub REST API Documentation](https://docs.github.com/en/rest)
- [Personal Access Tokens](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token)
- [Rate Limiting](https://docs.github.com/en/rest/overview/resources-in-the-rest-api#rate-limiting)
- [API Best Practices](https://docs.github.com/en/rest/guides/best-practices-for-integrators)

---

## ?? Conclusión

GitHub API ofrece una forma **poderosa y eficiente** de interactuar con repositorios sin necesidad de clonarlos localmente. Es ideal para:

- ? Consultas rápidas y ligeras
- ? Automatización y CI/CD
- ? Análisis de metadatos
- ? Múltiples repositorios

**Combinado con el patrón ReAct del agente**, permite realizar análisis complejos de código y repositorios de manera inteligente y conversacional.

---

**Última actualización**: v3.5.0  
**Autor**: AgentWikiChat Team

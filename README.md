# 🤖 AgentWikiChat

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-3.5.0-green.svg)](https://github.com/ffontanini/AgentWikiChat)

**AgentWikiChat** es un agente conversacional inteligente multi-provider basado en .NET 9 que implementa el patrón **ReAct (Reasoning + Acting)** con soporte completo para **Tool Calling**. Permite interactuar con múltiples proveedores de IA y ejecutar herramientas especializadas de forma autónoma.

**🎉 NUEVO en v3.5.0**: Arquitectura genérica de control de versiones con soporte para SVN y Git.

----

## ✨ Características Principales

- 🧠 **Patrón ReAct**: Razonamiento paso a paso con múltiples herramientas
- 🔄 **Multi-Provider AI**: Soporte para Ollama, OpenAI, LM Studio y Anthropic Claude
- 🛠️ **Tool Calling Unificado**: Formato estándar compatible con todos los proveedores
- 🗄️ **Multi-Database**: SQL Server y PostgreSQL con misma interfaz
- 📚 **Wikipedia Integration**: Búsqueda y obtención de artículos
- 📦 **SVN Repository**: Consultas de solo lectura a repositorios Subversion
- 🔒 **Seguridad**: Solo consultas SELECT de lectura en bases de datos y operaciones de lectura en SVN
- 💾 **Session Logging**: Guarda conversaciones automáticamente
- 🎯 **Memoria Modular**: Contexto global + contexto por módulo
- 🔍 **Debug Mode**: Visualización detallada del proceso de razonamiento
- ⚡ **Detección de Loops**: Previene invocaciones repetidas inútiles

---

## 🎯 Casos de Uso

- 💬 **Chatbot Inteligente** con acceso a datos estructurados
- 📊 **Análisis de Datos** mediante consultas SQL naturales
- 🔍 **Búsqueda de Información** enciclopédica (Wikipedia)
- 📦 **Consulta de Repositorios** SVN con búsqueda de historial y código
- 🧪 **Investigación Multi-Paso** usando varias herramientas en secuencia
- 📈 **Reportes Automáticos** desde bases de datos
- 🎓 **Asistente de Aprendizaje** con contexto conversacional
- 👨‍💻 **Auditoría de Código** y análisis de autoría en repositorios

---

## 🚀 Inicio Rápido

### Prerequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server o PostgreSQL (opcional, para la herramienta de base de datos)
- Cliente SVN (opcional, para la herramienta de repositorio)
- Uno de los siguientes proveedores de IA:
  - [Ollama](https://ollama.ai/) (local, gratis)
  - [LM Studio](https://lmstudio.ai/) (local, gratis)
  - OpenAI API Key
  - Anthropic API Key

### Instalación

1. **Clonar el repositorio**
```bash
git clone https://github.com/yourusername/AgentWikiChat.git
cd AgentWikiChat
```

2. **Restaurar dependencias**
```bash
cd AgentWikiChat
dotnet restore
```

3. **Configurar el proveedor de IA**

Editar `appsettings.json`:
```json
{
  "AI": {
    "ActiveProvider": "LMStudio-Local",
    "Providers": [
      {
        "Name": "LMStudio-Local",
        "Type": "LMStudio",
        "BaseUrl": "http://localhost:1234",
        "Model": "meta-llama-3-8b-instruct",
        "Temperature": 0.7,
        "MaxTokens": 2048
      }
    ]
  }
}
```

4. **Configurar base de datos** (opcional)

```json
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=localhost;Database=MyDB;Integrated Security=True;",
    "MaxRowsToReturn": 100
  }
}
```

5. **Configurar SVN** (opcional)

```json
{
  "SVN": {
    "Provider": "SVN",
    "RepositoryUrl": "https://svn.company.com/repos/project",
    "Username": "myuser",
    "Password": "mypassword",
    "WorkingCopyPath": "",
    "CommandTimeout": 60,
    "EnableLogging": true
  }
}
```

**Parámetros:**
- **`Provider`**: Tipo de control de versiones ("SVN", "Git", etc.)
- **`RepositoryUrl`**: URL del repositorio SVN (HTTP, HTTPS, SVN, FILE protocols)
- **`Username`**: Usuario para autenticación (opcional si el repo es público)
- **`Password`**: Contraseña para autenticación
- **`WorkingCopyPath`**: Ruta local de working copy para operación `status` (opcional)
- **`CommandTimeout`**: Timeout en segundos para operaciones SVN
- **`EnableLogging`**: Habilita logging detallado de operaciones

**Ejemplos de URL:**
- HTTP: `http://svn.company.com/repos/project`
- HTTPS: `https://svn.secure.com/repos/project`
- SVN: `svn://svn.company.com/repos/project`
- FILE: `file:///C:/SVNRepos/project`

6. **Ejecutar**
```bash
dotnet run
```

---

## 📖 Uso

### Comandos Disponibles

| Comando | Descripción |
|---------|-------------|
| `/salir` | Finalizar la aplicación |
| `/memoria` | Ver estado de la memoria conversacional |
| `/limpiar` | Limpiar memoria y reiniciar contexto |
| `/tools` | Listar herramientas disponibles |
| `/config` | Ver configuración del agente |
| `/debug` | Alternar modo debug |

### Ejemplos de Consultas

**Wikipedia:**
```
👤 Tú> busca información sobre inteligencia artificial
```

**Base de Datos (SQL Server):**
```
👤 Tú> ¿cuántos usuarios hay en la base de datos?
👤 Tú> muéstrame las últimas 10 ventas
```

**Base de Datos (PostgreSQL):**
```
👤 Tú> dame los 5 productos más caros
👤 Tú> lista todas las tablas disponibles
```

**SVN Repository:**
```
👤 Tú> muéstrame los últimos 5 commits del repositorio
👤 Tú> ¿quién modificó el archivo Main.cs?
👤 Tú> lista los archivos en /trunk/src
👤 Tú> muestra el contenido del archivo README.md
```

**Multi-Step (ReAct):**
```
👤 Tú> busca información sobre C# en Wikipedia y luego cuéntame cuántos proyectos en C# tenemos en la BD
👤 Tú> dame los últimos commits y busca información sobre el autor principal en Wikipedia
```

---

## 🛠️ Herramientas Disponibles

### 1. 📚 Wikipedia
- **`search_wikipedia_titles`**: Busca títulos de artículos
- **`get_wikipedia_article`**: Obtiene contenido completo de un artículo

### 2. 🗄️ Base de Datos
- **`query_database`**: Ejecuta consultas SELECT (solo lectura)
- Soporta: SQL Server, PostgreSQL
- Seguridad: Bloquea INSERT, UPDATE, DELETE, DROP, etc.

### 3. 📦 SVN Repository
- **`svn_operation`**: Ejecuta operaciones de solo lectura en repositorios SVN
- Operaciones soportadas:
  - **`log`**: Ver historial de commits
  - **`info`**: Información del repositorio/archivo
  - **`list`**: Listar archivos y directorios
  - **`cat`**: Ver contenido de archivos
  - **`diff`**: Ver diferencias entre revisiones
  - **`blame`**: Ver autoría línea por línea
  - **`status`**: Estado de working copy
- Seguridad: Bloquea commit, delete, update, merge, etc.
- Compatible con SVN 1.6+

### 🆕 4. Git Repository (v3.5.0)
- **`git_operation`**: Ejecuta operaciones de solo lectura en repositorios Git
- Operaciones soportadas:
  - **`log`**: Ver historial de commits
  - **`show`**: Detalles de un commit específico
  - **`ls-tree`**: Listar archivos en el árbol
  - **`blame`**: Ver autoría línea por línea
  - **`diff`**: Ver diferencias entre commits
  - **`status`**: Estado del working directory
  - **`branch`**: Listar ramas
  - **`tag`**: Listar tags
- Seguridad: Bloquea commit, push, pull, add, rm, etc.
- Compatible con Git 2.0+

### 🆕 5. GitHub Repository (v3.5.0)
- **`github_operation`**: Ejecuta operaciones de solo lectura en repositorios GitHub usando API REST
- Operaciones soportadas:
  - **`log`**: Ver historial de commits
  - **`show`**: Detalles de un commit específico
  - **`list`**: Listar archivos y directorios
  - **`cat`**: Ver contenido de archivos
  - **`diff`**: Ver diferencias en un commit
  - **`blame`**: Ver información de autoría
  - **`branches`**: Listar todas las ramas
  - **`tags`**: Listar todos los tags
  - **`info`**: Información completa del repositorio
- Seguridad: Solo lectura, no permite push, merge, delete, etc.
- **No requiere cliente local** - Usa GitHub API v3
- Requiere Personal Access Token para repos privados

### 🆕 Configuración de Git (v3.5.0)

```json
{
  "Git": {
    "Provider": "Git",
    "RepositoryUrl": "https://github.com/user/repo.git",
    "Username": "myuser",
    "Password": "ghp_token_or_password",
    "WorkingCopyPath": "C:\\Projects\\MyRepo",
    "CommandTimeout": 60,
    "EnableLogging": true
  }
}
```

**Parámetros:**
- **`Provider`**: "Git"
- **`RepositoryUrl`**: URL del repositorio Git (HTTPS, SSH, local)
- **`Username`**: Usuario para autenticación (para HTTPS)
- **`Password`**: Token de acceso personal o contraseña
- **`WorkingCopyPath`**: Ruta local del repositorio clonado (obligatorio para Git)
- **`CommandTimeout`**: Timeout en segundos para operaciones Git
- **`EnableLogging`**: Habilita logging detallado de operaciones

**Nota**: Para GitHub/GitLab, usa Personal Access Token en lugar de contraseña.

### 🆕 Configuración de GitHub (v3.5.0)

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

**Parámetros:**
- **`Provider`**: "GitHub"
- **`RepositoryUrl`**: URL del repositorio GitHub (sin .git)
- **`Username`**: Tu nombre de usuario de GitHub
- **`Password`**: Personal Access Token (obligatorio, obtenerlo en: https://github.com/settings/tokens)
- **`Branch`**: Rama predeterminada (default: "main")
- **`CommandTimeout`**: Timeout en segundos para llamadas API
- **`EnableLogging`**: Habilita logging detallado de operaciones

**Ventajas de GitHub API:**
- ✅ No requiere cliente local instalado
- ✅ No requiere clonar el repositorio
- ✅ Acceso instantáneo a cualquier repositorio
- ✅ Funciona con repos públicos y privados
- ✅ 5,000 requests/hora con token

**Obtener Personal Access Token:**
1. Ve a: https://github.com/settings/tokens
2. Click en "Generate new token (classic)"
3. Selecciona scopes: `repo` (o `public_repo` para solo públicos)
4. Copia el token generado (comienza con `ghp_`)
5. Úsalo en el campo `Password`

**⚠️ Importante:**
- El token es sensible, no lo compartas
- Para repos públicos, el token es opcional (pero con límite de 60 requests/hora)
- Guarda el token de forma segura (ej: Azure Key Vault, variables de entorno)

---

## 📚 Documentación

- 📐 **[Arquitectura](AgentWikiChat/Docs/ARCHITECTURE.md)** - Diseño y patrones del sistema
- 🗄️ **[Database Tool](AgentWikiChat/Docs/SqlServerTool-README.md)** - Uso de consultas SQL
- 📦 **[SVN Tool](AgentWikiChat/Docs/SVNTool-README.md)** - Operaciones en repositorios SVN
- 🔍 **[SVN Troubleshooting](AgentWikiChat/Docs/SVN-TroubleshootingGuide.md)** - Solución de problemas SVN
- 📝 **[Session Logging](AgentWikiChat/Docs/SessionLogging-README.md)** - Sistema de logging

### 🆕 v3.5.0 - Control de Versiones Genérico
- 🏗️ **[VersionControl Architecture](AgentWikiChat/Docs/VersionControl-Architecture.md)** - Arquitectura completa
- 📋 **[VersionControl Changelog](AgentWikiChat/Docs/VersionControl-Changelog.md)** - Changelog técnico
- 📊 **[VersionControl Summary](AgentWikiChat/Docs/VersionControl-Summary.md)** - Resumen ejecutivo

## 🏗️ Arquitectura v3.5.0 - Control de Versiones Genérico

```
RepositoryToolHandler
    │
    └── VersionControlHandlerFactory
            │
            ├── IVersionControlHandler (interfaz)
            │       │
            │       └── BaseVersionControlHandler (base común)
            │               │
            │               ├── SvnVersionControlHandler
            │               ├── GitVersionControlHandler
            │               └── GitHubVersionControlHandler  ← 🆕 v3.5.0
            │
            └── Fácil extensión: GitLab, Bitbucket, Mercurial, TFS, Perforce

```

**Beneficios:**
- ✅ Arquitectura modular y extensible
- ✅ Código reutilizable entre proveedores
- ✅ Fácil agregar nuevos sistemas de control de versiones
- ✅ 81% reducción en complejidad
- ✅ **GitHub sin cliente local** - usa API REST

**Documentación detallada**: [`Docs/VersionControl-Architecture.md`](AgentWikiChat/Docs/VersionControl-Architecture.md)

## 📁 Estructura del Proyecto

```
├── Configuration/     # Configuración del agente
├── Models/            # Modelos de datos
├── Services/
│   ├── AI/            # Servicios de proveedores de IA
│   ├── Database/      # Handlers de bases de datos
│   ├── VersionControl/ # 🆕 Handlers de control de versiones (v3.5.0)
│   │   ├── IVersionControlHandler.cs
│   │   ├── BaseVersionControlHandler.cs
│   │   ├── SvnVersionControlHandler.cs
│   │   ├── GitVersionControlHandler.cs
│   │   ├── GitHubVersionControlHandler.cs      # 🆕 v3.5.0
│   │   └── VersionControlHandlerFactory.cs
│   ├── Handlers/      # Handlers de herramientas
│   ├── AgentOrchestrator.cs
│   ├── ReActEngine.cs
│   ├── MemoryService.cs
│   └── ConsoleLogger.cs
├── Docs/              # Documentación
│   ├── ARCHITECTURE.md
│   ├── SqlServerTool-README.md
│   ├── SVNTool-README.md
│   ├── SVN-TroubleshootingGuide.md
│   ├── VersionControl-Architecture.md    # 🆕 v3.5.0
│   ├── VersionControl-Changelog.md       # 🆕 v3.5.0
│   ├── VersionControl-Summary.md         # 🆕 v3.5.0
│   └── SessionLogging-README.md
├── Scripts/           # Scripts de utilidad y diagnóstico
├── Logs/              # Logs de sesiones (no versionado)
├── Program.cs         # Punto de entrada
└── appsettings.json   # Configuración

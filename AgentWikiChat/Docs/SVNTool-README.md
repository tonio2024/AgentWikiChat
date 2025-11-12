# ?? SVN Repository Tool - Documentación

## Descripción

La herramienta `svn_operation` permite al agente ejecutar operaciones de **solo lectura** en repositorios Subversion (SVN) de manera segura y controlada.

**? Operaciones Soportadas**: log, info, list, cat, diff, blame, status

## ?? Seguridad

### Restricciones Implementadas

La herramienta está diseñada con **máxima seguridad** y solo permite operaciones de **solo lectura**:

? **PERMITIDO:**
- `log` - Ver historial de commits
- `info` - Información del repositorio/archivo
- `list` (o `ls`) - Listar archivos y directorios
- `cat` - Ver contenido de archivos
- `diff` - Ver diferencias entre revisiones
- `blame` (o `annotate`, `praise`) - Ver autoría línea por línea
- `status` - Estado de working copy (si está configurado)

? **PROHIBIDO:**
- `commit` (o `ci`) - Guardar cambios
- `delete` (o `del`, `remove`, `rm`) - Eliminar archivos
- `add` - Agregar archivos al repositorio
- `checkout` (o `co`) - Descargar working copy
- `update` (o `up`) - Actualizar working copy
- `switch` - Cambiar de rama
- `merge` - Fusionar cambios
- `copy` (o `cp`) - Copiar archivos/directorios
- `move` (o `mv`) - Mover/renombrar
- `mkdir` - Crear directorios
- `import`/`export` - Importar/exportar
- `propdel`/`propset` - Modificar propiedades
- `lock`/`unlock` - Bloquear/desbloquear archivos

### Validaciones Automáticas

1. **Lista blanca de comandos**: Solo comandos de lectura permitidos
2. **Lista negra de comandos**: Bloqueo explícito de operaciones de escritura
3. **Timeout configurable**: Previene operaciones que demoran demasiado
4. **Non-interactive mode**: Evita prompts de contraseña/certificados

---

## ?? Configuración

### Requisito Previo

**Instalar el cliente SVN:**
- **Windows**: [TortoiseSVN](https://tortoisesvn.net/) o [Apache Subversion](https://subversion.apache.org/)
- **Linux**: `sudo apt-get install subversion` (Ubuntu/Debian) o `sudo yum install subversion` (CentOS/RHEL)
- **macOS**: `brew install svn`

**Verificar instalación:**
```bash
svn --version
```

### appsettings.json

```json
{
  "SVN": {
    "RepositoryUrl": "https://svn.company.com/repos/myproject",
    "Username": "myuser",
    "Password": "mypassword",
    "WorkingCopyPath": "",
    "CommandTimeout": 60,
    "EnableLogging": true
  }
}
```

### Parámetros de Configuración

| Parámetro | Descripción | Obligatorio | Por defecto |
|-----------|-------------|-------------|-------------|
| `RepositoryUrl` | URL del repositorio SVN | **Sí** | - |
| `Username` | Usuario para autenticación | No (si el repo es público) | "" |
| `Password` | Contraseña para autenticación | No | "" |
| `WorkingCopyPath` | Ruta local de working copy (para `status`) | No | "" |
| `CommandTimeout` | Timeout en segundos | No | 60 |
| `EnableLogging` | Habilita logging de operaciones | No | true |

---

## ?? Solución de Problemas (v3.4.0+)

### Error Común: E170013 - Unable to connect

Si recibes el error:
```
svn: E170013: Unable to connect to a repository at URL
svn: E120112: Error running context: APR does not understand this error code
```

**Soluciones implementadas en v3.4.0:**

1. **Diagnóstico automático**: El handler ahora prueba la conexión al iniciar
2. **Compatibilidad mejorada**: Soporte para SVN 1.6+ (antes solo 1.10+)
3. **Mensajes de error detallados**: Con sugerencias específicas según el error

**Herramientas de diagnóstico incluidas:**

#### Windows:
```cmd
Scripts\SVN-Diagnostic.bat
```

#### Linux/Mac:
```bash
chmod +x Scripts/SVN-Diagnostic.sh
./Scripts/SVN-Diagnostic.sh
```

**Guía completa de troubleshooting:**
?? Ver [`Docs/SVN-TroubleshootingGuide.md`](./SVN-TroubleshootingGuide.md)

### Solución Rápida: Usar Working Copy Local

Si tienes problemas de conexión constantes, usa una copia local:

1. **Hacer checkout manual** (una sola vez):
```bash
svn checkout http://svn.server.com/repos/project C:\Projects\MyProject --username user
```

2. **Configurar en appsettings.json**:
```json
{
  "SVN": {
    "RepositoryUrl": "http://svn.server.com/repos/project",
    "Username": "myuser",
    "Password": "mypassword",
    "WorkingCopyPath": "C:\\Projects\\MyProject"
  }
}
```

3. Las operaciones `info` y `status` usarán la copia local automáticamente ?

---

## ?? Uso

### Ejemplos de Consultas al Agente

#### **Ejemplo 1: Ver últimos commits**
```
Usuario: "Muéstrame los últimos 5 commits del repositorio"

Agente: svn_operation({
  "operation": "log",
  "limit": "5"
})
```

**Resultado:**
```
?? Resultado de SVN - LOG

Repositorio: https://svn.company.com/repos/myproject

Resultado:
------------------------------------------------------------------------
r1234 | juan.perez | 2025-01-06 14:30:25 | 2 líneas

Agregado validación de formulario
M /trunk/src/Forms/LoginForm.cs
A /trunk/src/Validators/LoginValidator.cs
------------------------------------------------------------------------
r1233 | maria.gomez | 2025-01-06 12:15:10 | 1 línea

Corregido bug en el módulo de reportes
M /trunk/src/Reports/ReportGenerator.cs
------------------------------------------------------------------------
...
```

---

#### **Ejemplo 2: Ver información de un archivo**
```
Usuario: "Dame información del archivo Main.cs"

Agente: svn_operation({
  "operation": "info",
  "path": "/trunk/src/Main.cs"
})
```

**Resultado:**
```
?? Resultado de SVN - INFO

Repositorio: https://svn.company.com/repos/myproject
Path: /trunk/src/Main.cs

Resultado:
Path: Main.cs
Name: Main.cs
URL: https://svn.company.com/repos/myproject/trunk/src/Main.cs
Repository Root: https://svn.company.com/repos/myproject
Revision: 1234
Node Kind: file
Last Changed Author: juan.perez
Last Changed Rev: 1230
Last Changed Date: 2025-01-05 16:45:00
```

---

#### **Ejemplo 3: Listar archivos de un directorio**
```
Usuario: "Lista los archivos en /trunk/src"

Agente: svn_operation({
  "operation": "list",
  "path": "/trunk/src"
})
```

**Resultado:**
```
?? Resultado de SVN - LIST

Repositorio: https://svn.company.com/repos/myproject
Path: /trunk/src

Resultado:
   1230 juan.perez       15234 Jan 05 16:45 Main.cs
   1228 maria.gomez       8456 Jan 05 14:20 Program.cs
   1232 juan.perez        4567 Jan 06 10:30 Config.cs
   1225 carlos.lopez           Jan 04 09:15 Utils/
   1229 maria.gomez            Jan 05 15:00 Models/
```

---

#### **Ejemplo 4: Ver contenido de un archivo**
```
Usuario: "Muéstrame el contenido del archivo README.md"

Agente: svn_operation({
  "operation": "cat",
  "path": "/trunk/README.md"
})
```

**Resultado:**
```
?? Resultado de SVN - CAT

Repositorio: https://svn.company.com/repos/myproject
Path: /trunk/README.md

Resultado:
# Mi Proyecto

Este es el proyecto de ejemplo...
[contenido completo del archivo]
```

---

#### **Ejemplo 5: Ver diferencias entre revisiones**
```
Usuario: "Muestra los cambios entre las revisiones 1200 y 1210"

Agente: svn_operation({
  "operation": "diff",
  "revision": "1200:1210"
})
```

**Resultado:**
```
?? Resultado de SVN - DIFF

Repositorio: https://svn.company.com/repos/myproject
Revisión: 1200:1210

Resultado:
Index: trunk/src/Main.cs
===================================================================
--- trunk/src/Main.cs   (revision 1200)
+++ trunk/src/Main.cs   (revision 1210)
@@ -15,7 +15,10 @@
     public void Initialize()
     {
-        Console.WriteLine("Iniciando...");
+        Console.WriteLine("Iniciando aplicación...");
+        LoadConfiguration();
+        ConnectToDatabase();
     }
```

---

#### **Ejemplo 6: Ver autoría de un archivo (blame)**
```
Usuario: "¿Quién escribió cada línea del archivo Config.cs?"

Agente: svn_operation({
  "operation": "blame",
  "path": "/trunk/src/Config.cs"
})
```

**Resultado:**
```
?? Resultado de SVN - BLAME

Repositorio: https://svn.company.com/repos/myproject
Path: /trunk/src/Config.cs

Resultado:
  1200 juan.perez    using System;
  1200 juan.perez    using System.Configuration;
  1210 maria.gomez   using System.IO;
  1200 juan.perez    
  1200 juan.perez    public class Config
  1200 juan.perez    {
  1210 maria.gomez       public string DatabasePath { get; set; }
  1215 carlos.lopez      public int Timeout { get; set; }
  1200 juan.perez    }
```

---

## ?? Definición de la Tool

```json
{
  "name": "svn_operation",
  "description": "Ejecuta operaciones de SOLO LECTURA en repositorios SVN",
  "parameters": {
    "operation": {
      "type": "string",
      "required": true,
      "enum": ["log", "info", "list", "cat", "diff", "blame", "status"],
      "description": "Operación SVN a ejecutar"
    },
    "path": {
      "type": "string",
      "required": false,
      "description": "Ruta del archivo o directorio (opcional)"
    },
    "revision": {
      "type": "string",
      "required": false,
      "description": "Número de revisión o rango (por defecto HEAD)"
    },
    "limit": {
      "type": "string",
      "required": false,
      "description": "Límite de resultados para log (por defecto 10)"
    }
  }
}
```

---

## ?? Formato de Salida

El resultado se presenta en formato Markdown con:

- **Operación ejecutada** (LOG, INFO, LIST, etc.)
- **URL del repositorio**
- **Path** (si aplica)
- **Revisión** (si aplica)
- **Resultado** en bloque de código

---

## ?? Mensajes de Error

### Error: Operación rechazada
```
?? Operación Rechazada por Seguridad

Operación 'commit' está prohibida. Solo se permiten operaciones de solo lectura.

?? Recuerda: Solo se permiten operaciones de solo lectura (log, info, list, cat, diff, blame, status).
```

### Error: SVN no encontrado
```
? Error en SVN

Mensaje: The system cannot find the file specified

?? Verifica que el cliente SVN esté instalado y en el PATH del sistema.
```

### Error: Conexión fallida (NUEVO en v3.4.0)
```
? Error en SVN

Mensaje: svn: E170013: Unable to connect to a repository at URL

?? Posibles soluciones:

1. ? Verifica que la URL sea correcta
2. ?? Verifica conectividad de red
3. ?? Verifica credenciales
4. ?? Intenta usar una working copy local
5. ?? Ejecuta el script de diagnóstico: Scripts\SVN-Diagnostic.bat

Ver guía completa: Docs/SVN-TroubleshootingGuide.md
```

### Error: Autenticación fallida
```
? Error en SVN

Mensaje: Authentication failed

?? Verifica el usuario y contraseña en appsettings.json.
```

### Error: Timeout
```
? Error en SVN

Mensaje: La operación SVN excedió el timeout de 60 segundos.

?? Aumenta CommandTimeout en appsettings.json o verifica la conectividad al repositorio.
```

---

## ?? Testing

### Comandos de Prueba

#### 1. Verificar Conexión
```
Usuario: "Dame información del repositorio"
? svn_operation({ operation: "info" })
```

#### 2. Ver Historial
```
Usuario: "Muestra los últimos 10 commits"
? svn_operation({ operation: "log", limit: "10" })
```

#### 3. Explorar Estructura
```
Usuario: "Lista los archivos en /trunk"
? svn_operation({ operation: "list", path: "/trunk" })
```

#### 4. Ver Archivo Específico
```
Usuario: "Muestra el contenido de /trunk/README.txt"
? svn_operation({ operation: "cat", path: "/trunk/README.txt" })
```

### Operaciones que Serán Rechazadas

```
# ? COMMIT
Usuario: "Haz commit de los cambios"
? Rechazado: operación 'commit' prohibida

# ? DELETE
Usuario: "Borra el archivo old.txt"
? Rechazado: operación 'delete' prohibida

# ? UPDATE
Usuario: "Actualiza el working copy"
? Rechazado: operación 'update' prohibida
```

---

## ?? Configuraciones Avanzadas

### 1. Repositorio HTTP con Autenticación

```json
{
  "SVN": {
    "RepositoryUrl": "http://svn.company.com/repos/project",
    "Username": "myuser",
    "Password": "mypassword"
  }
}
```

### 2. Repositorio HTTPS con SSL

```json
{
  "SVN": {
    "RepositoryUrl": "https://svn.secure.com/repos/project",
    "Username": "myuser",
    "Password": "mypassword"
  }
}
```

### 3. Repositorio Local (file://)

```json
{
  "SVN": {
    "RepositoryUrl": "file:///C:/SVNRepos/project",
    "Username": "",
    "Password": ""
  }
}
```

### 4. Con Working Copy Local (RECOMENDADO para problemas de conexión)

```json
{
  "SVN": {
    "RepositoryUrl": "https://svn.company.com/repos/project",
    "Username": "myuser",
    "Password": "mypassword",
    "WorkingCopyPath": "C:/Projects/MyProject"
  }
}
```

---

## ?? Troubleshooting Avanzado

### Error: "svn: E170000: URL doesn't exist"
**Solución**: Verifica que la URL del repositorio sea correcta y que tengas permisos de lectura.

### Error: "svn: E215004: Authentication failed"
**Solución**: Verifica usuario y contraseña en `appsettings.json`.

### Error: "The system cannot find the file specified"
**Solución**: 
1. Instala el cliente SVN
2. Agrega `svn.exe` al PATH del sistema
3. Reinicia la aplicación

### Error: Timeout en operaciones grandes
**Solución**: Aumenta `CommandTimeout` en `appsettings.json`:
```json
{
  "SVN": {
    "CommandTimeout": 120
  }
}
```

### Working Copy no se reconoce
**Solución**: Verifica que `WorkingCopyPath` apunte a un directorio válido de SVN:
```json
{
  "SVN": {
    "WorkingCopyPath": "C:/Projects/MyProject"
  }
}
```

### Error: E170013 / E120112 (Conexión)
**Solución completa**: Ver [`Docs/SVN-TroubleshootingGuide.md`](./SVN-TroubleshootingGuide.md)

**Prueba rápida**:
```bash
# Windows
Scripts\SVN-Diagnostic.bat

# Linux/Mac
./Scripts/SVN-Diagnostic.sh
```

---

## ?? Notas Adicionales

- El handler guarda un registro de cada operación en la memoria modular (`svn`)
- Los logs detallados están disponibles cuando `Debug` y `EnableLogging` están habilitados
- La validación se ejecuta **antes** de invocar el comando SVN
- **Compatible con SVN 1.6+ (mejorado en v3.4.0)**
- Soporta HTTP, HTTPS, SVN y FILE protocols
- **Diagnóstico automático de conexión al iniciar**
- **Mensajes de error con sugerencias contextuales**

---

## ?? Casos de Uso Comunes

| Caso de Uso | Operación | Ejemplo |
|-------------|-----------|---------|
| **Ver últimos cambios** | `log` | `{ operation: "log", limit: "10" }` |
| **Información de archivo** | `info` | `{ operation: "info", path: "/trunk/Main.cs" }` |
| **Explorar directorio** | `list` | `{ operation: "list", path: "/trunk/src" }` |
| **Leer archivo** | `cat` | `{ operation: "cat", path: "/trunk/README.md" }` |
| **Ver diferencias** | `diff` | `{ operation: "diff", revision: "1200:1210" }` |
| **Autoría de código** | `blame` | `{ operation: "blame", path: "/trunk/Config.cs" }` |
| **Estado de cambios** | `status` | `{ operation: "status" }` (requiere WorkingCopyPath) |

---

## ?? Referencias

- [Apache Subversion Documentation](https://subversion.apache.org/docs/)
- [TortoiseSVN Manual](https://tortoisesvn.net/docs/)
- [SVN Command Reference](https://svnbook.red-bean.com/en/1.8/svn.ref.svn.html)
- [Troubleshooting Guide](./SVN-TroubleshootingGuide.md) **(NUEVO)**

---

## ?? Changelog

### v3.4.0 (2025-01-XX)
- ? Detección automática de versión SVN
- ? Compatibilidad mejorada con SVN 1.6+ (antes solo 1.10+)
- ? Diagnóstico automático de conexión al iniciar
- ? Mensajes de error con sugerencias contextuales
- ? Soporte mejorado para working copy local
- ? Scripts de diagnóstico incluidos (Windows

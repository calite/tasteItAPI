# TasteIt API

Backend ASP.NET Web API (.NET 6) con autenticación Firebase y base de datos Neo4j.

## Requisitos

- .NET SDK 6.0+
- Docker Desktop
- Neo4j (contenedor Docker)
- Archivo de credenciales Firebase (`firebase-config.json`)

## 1) Levantar Neo4j en Docker

```powershell
docker pull neo4j:5
docker run -d --name neo4j-tasteit `
  -p 7474:7474 -p 7687:7687 `
  -e NEO4J_AUTH=neo4j/password123 `
  -v neo4j_data:/data `
  neo4j:5
```

Verificar:

```powershell
docker ps
```

Acceso web:

- http://localhost:7474
- User: `neo4j`
- Password: `password123`

## 2) Configurar Firebase

Coloca el archivo `firebase-config.json` dentro de:

`tasteItAPI/TasteItApi/firebase-config.json`

Nota: este archivo está ignorado por git (no se versiona).

## 3) Configurar secretos de Neo4j (obligatorio)

El proyecto no usa contraseña en `appsettings.json` por seguridad.
Debes configurar `Neo4j:Password` con User Secrets o variable de entorno.

En la carpeta `tasteItAPI/TasteItApi`:

```powershell
dotnet user-secrets set "Neo4j:Password" "password123"
```

Alternativa por variable de entorno:

```powershell
$env:Neo4j__Password="password123"
```

## 4) Restaurar y compilar

Desde `tasteItAPI/TasteItApi`:

```powershell
dotnet restore
dotnet build
```

## 5) Ejecutar la API

```powershell
dotnet run
```

Swagger (Development):

- https://localhost:7076/swagger
- http://localhost:5269/swagger

## 6) Endpoints de testing (`/test`)

Los endpoints `/test/*` solo se registran cuando:

- el entorno es `Development`, o
- `Features:EnableTestGraphEndpoints = true`

Fuera de Development están bloqueados por middleware (`404`).

## Configuración principal (appsettings.json)

```json
"Neo4j": {
  "Uri": "bolt://localhost:7687",
  "Username": "neo4j",
  "Password": "",
  "ConnectionTimeoutSeconds": 15,
  "MaxConnectionPoolSize": 100
},
"Features": {
  "EnableTestGraphEndpoints": false
}
```

## Solución de problemas

### Error: `Neo4j password is not configured`

Configura User Secrets o `Neo4j__Password`.

### Error HTTPS/certificado de desarrollo

```powershell
dotnet dev-certs https --trust
```

Si quieres ejecutar solo por HTTP:

```powershell
dotnet run --no-launch-profile --urls http://localhost:5000
```

### Error de archivo bloqueado (`TasteItApi.exe is being used by another process`)

Detén la instancia previa de la API (Visual Studio/dotnet run) y vuelve a compilar.

### Ver logs de Neo4j

```powershell
docker logs neo4j-tasteit
```

## Comandos útiles Docker

```powershell
docker stop neo4j-tasteit
docker start neo4j-tasteit
docker logs neo4j-tasteit
```


# Películas API (ASP.NET Core)

Esta API repite los endpoints de `peliculas-backend` (NestJS): películas, registro, login y perfil. Usa Entity Framework Core con SQLite, JWT y Swagger UI. Corre en el puerto **5080** para poder dejar NestJS en el **3000**.

## Requisitos

- SDK de .NET 10.

Si `dotnet --version` no encuentra el comando, el SDK de este equipo está en la carpeta del usuario:

```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
dotnet --version
```

## Levantar la API

Desde la raíz del repositorio:

```bash
cd peliculas-aspnet
dotnet restore
dotnet ef database update
dotnet run
```

`dotnet run` también aplica las migraciones al arrancar. La base queda en `peliculas-aspnet/peliculas.db` y no se comparte con Prisma.

- API: `http://localhost:5080`
- Documento OpenAPI: `http://localhost:5080/openapi/v1.json`
- Swagger UI: `http://localhost:5080/swagger`

El secreto del JWT es el mismo valor por defecto que NestJS. Puedes cambiarlo con la variable `JWT_SECRET`.

## Probar los endpoints protegidos en Swagger

1. Abre `http://localhost:5080/swagger`.
2. Ejecuta `POST /auth/register` o `POST /auth/login`.
3. Copia el campo `token` de la respuesta. No incluyas la palabra `Bearer`.
4. Pulsa **Authorize**, pega el token y confirma.
5. Prueba `POST /pelicula`, `PATCH /pelicula/{id}`, `DELETE /pelicula/{id}` y `GET /auth/profile`.

Sin token, crear, editar y eliminar responden 401 con `message`. El listado y el detalle de películas son públicos.

## Apuntar los dos clientes a esta API

Vue e Ionic usan `http://localhost:5080` sin necesidad de `VITE_API_URL`. Levanta esta API y arranca cada cliente:

```bash
cd peliculas-ionic
npm run dev
```

```bash
cd peliculas-frontend
npm run dev
```

En el celular por Wi-Fi sustituye `localhost` por la IP de la computadora, por ejemplo `VITE_API_URL=http://192.168.1.24:5080`. Por USB, con la API en 5080:

```bash
adb reverse tcp:5080 tcp:5080
```

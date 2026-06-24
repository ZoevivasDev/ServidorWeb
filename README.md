# Servidor Web — Blog Personal.

Servidor web construido desde cero en C# (.NET 8), usando únicamente sockets TCP y parseo manual de HTTP. Sin frameworks, sin librerías de servidor externas.

## Tecnologías

| Componente     | Tecnología                              |
|----------------|------------------------------------------|
| Lenguaje       | C# (.NET 8)                              |
| Sockets        | `System.Net.Sockets` (`TcpListener` / `TcpClient`) |
| Concurrencia   | `System.Threading` (`Thread`, `lock`)    |
| Compresión     | `System.IO.Compression` (`GZipStream`)   |
| Configuración  | `System.Text.Json` (`config.json`)       |
| Logging        | `System.IO.File` (`AppendAllText`)       |

## Estructura del proyecto

```
ServidorWeb/
├── Program.cs              # Punto de entrada: carga config, abre el listener y lanza un hilo por conexión
├── ManejadorConexion.cs    # Ciclo de vida de una conexión: parseo, lógica, respuesta y log
├── SolicitudHttp.cs        # Parser HTTP: extrae método, ruta, query string, headers y cuerpo POST
├── RespuestaHttp.cs        # Construye la respuesta HTTP, aplica gzip y escribe en el socket
├── Logger.cs                # Log diario con exclusión mutua (lock) entre hilos
├── Configuracion.cs         # Lee config.json y expone puerto y carpeta raíz
├── config.json              # Puerto y carpeta raíz configurables sin recompilar
├── wwwroot/
│   ├── index.html / index.css
│   └── 404.html / 404.css
└── logs/                     # Se genera automáticamente, un archivo por día
```

## Cómo ejecutarlo

Requisito previo: [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado.

```bash
dotnet run
```

Luego abrir el navegador en `http://localhost:8080` (o el puerto definido en `config.json`).

Los logs del día aparecen en `logs/yyyy-MM-dd.log`.

## Configuración

El archivo `config.json` permite cambiar el puerto y la carpeta raíz sin recompilar:

```json
{
  "puerto": 8080,
  "carpetaRaiz": "wwwroot"
}
```

## Autores
-Zoe vivas

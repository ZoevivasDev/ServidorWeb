using System.IO.Compression;
using System.Text;

namespace ServidorWeb
{
    internal static class RespuestaHttp
    {
        private static readonly Dictionary<string, string> _tiposMime = new()
        {
            { ".html", "text/html; charset=utf-8" },
            { ".css",  "text/css" },
            { ".js",   "application/javascript" },
            { ".json", "application/json" },
            { ".png",  "image/png" },
            { ".jpg",  "image/jpeg" },
            { ".ico",  "image/x-icon" },
            { ".txt",  "text/plain" }
        };

        // Envía un archivo al cliente, comprimido con gzip
        public static void EnviarArchivo(Stream socketStream, string rutaArchivo)
        {
            byte[] contenido = File.ReadAllBytes(rutaArchivo);
            byte[] comprimido = Comprimir(contenido);
            string mime = ObtenerMime(rutaArchivo);

            Enviar(socketStream, 200, "OK", mime, comprimido);
        }

        // Envía la página 404 personalizada
        public static void Enviar404(Stream socketStream, string carpetaRaiz)
        {
            string ruta404 = Path.Combine(carpetaRaiz, "404.html");
            byte[] contenido = File.Exists(ruta404)
                ? File.ReadAllBytes(ruta404)
                : Encoding.UTF8.GetBytes("<h1>404 - No encontrado</h1>");

            byte[] comprimido = Comprimir(contenido);
            Enviar(socketStream, 404, "Not Found", "text/html; charset=utf-8", comprimido);
        }

        // Arma el mensaje HTTP completo y lo escribe en el socket
        private static void Enviar(Stream socketStream, int codigo, string estado, string mime, byte[] cuerpoComprimido)
        {
            // Construye los headers HTTP como texto
            string headers =
                $"HTTP/1.1 {codigo} {estado}\r\n" +
                $"Content-Type: {mime}\r\n" +
                $"Content-Encoding: gzip\r\n" +           // avisa que va comprimido
                $"Content-Length: {cuerpoComprimido.Length}\r\n" +
                $"Connection: close\r\n" +
                $"\r\n";                                   // línea vacía separa headers del cuerpo

            byte[] bytesHeaders = Encoding.ASCII.GetBytes(headers);

            // Escribe headers + cuerpo comprimido directamente en el socket
            socketStream.Write(bytesHeaders, 0, bytesHeaders.Length);
            socketStream.Write(cuerpoComprimido, 0, cuerpoComprimido.Length);
            socketStream.Flush();
        }

        // Comprime un array de bytes usando gzip 
        private static byte[] Comprimir(byte[] datos)
        {
            using var memStream = new MemoryStream();
            using var gzip = new GZipStream(memStream, CompressionMode.Compress);
            gzip.Write(datos, 0, datos.Length);
            gzip.Close(); // importante: cierra para que vuelque todos los bytes
            return memStream.ToArray();
        }

        private static string ObtenerMime(string rutaArchivo)
        {
            string extension = Path.GetExtension(rutaArchivo).ToLower();
            return _tiposMime.TryGetValue(extension, out string? mime) ? mime : "application/octet-stream";
        }
    }
}

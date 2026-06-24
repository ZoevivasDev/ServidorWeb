using System.Net.Sockets;

namespace ServidorWeb
{ 
    internal class ManejadorConexion
    {
        private readonly TcpClient _cliente;
        private readonly Configuracion _config;

        //constructor  guarda parametros

        public ManejadorConexion(TcpClient cliente, Configuracion config)
        {
            _cliente = cliente;
            _config = config;
        }

        // Punto de entrada del hilo — todo el ciclo de vida de una solicitud
        public void Atender()
        {
            // Obtener la IP del visitante para el log
            string ip = _cliente.Client.RemoteEndPoint?.ToString() ?? "desconocida";

            try
            {
                using NetworkStream socketStream = _cliente.GetStream();
                using StreamReader reader = new StreamReader(socketStream, leaveOpen: true);

                // Parsear la solicitud HTTP cruda
                SolicitudHttp solicitud = SolicitudHttp.Parsear(reader);

                if (string.IsNullOrEmpty(solicitud.Metodo))
                {
                    _cliente.Close();
                    return;
                }

                // 2. Loguear query string si existe.
                if (!string.IsNullOrEmpty(solicitud.QueryString))
                {
                    Console.WriteLine($"  → Query params recibidos: {solicitud.QueryString}");
                }

                // 3. Manejar POST: solo loguear el cuerpo.
                if (solicitud.Metodo == "POST")
                {
                    Console.WriteLine($"  → Datos POST recibidos: {solicitud.Cuerpo}");
                    // Para POST devolvemos 200 OK vacío
                    string respuestaPost =
                        "HTTP/1.1 200 OK\r\n" +
                        "Content-Length: 0\r\n" +
                        "Connection: close\r\n\r\n";
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(respuestaPost);
                    socketStream.Write(bytes, 0, bytes.Length);
                    Logger.Registrar(ip, "POST", solicitud.RutaArchivo, 200);
                    return;
                }

                // 4. Resolver qué archivo servir (GET)
                string ruta = solicitud.RutaArchivo;

                // Si la ruta es "/" servir index.html por defecto
                if (ruta == "/" || ruta == "")
                    ruta = "/index.html";

                string rutaAbsoluta = Path.GetFullPath(
                    Path.Combine(_config.CarpetaRaiz, ruta.TrimStart('/'))
                );

                // Seguridad: no permitir salir de la carpeta raíz
                string raizAbsoluta = Path.GetFullPath(_config.CarpetaRaiz);
                if (!rutaAbsoluta.StartsWith(raizAbsoluta))
                {
                    RespuestaHttp.Enviar404(socketStream, _config.CarpetaRaiz);
                    Logger.Registrar(ip, "GET", solicitud.RutaArchivo, 404);
                    return;
                }

                // 5. Archivo existe servir; no existe 404
                if (!File.Exists(rutaAbsoluta))
                {
                    RespuestaHttp.Enviar404(socketStream, _config.CarpetaRaiz);
                    Logger.Registrar(ip, "GET", solicitud.RutaArchivo, 404);
                    return;
                }

                RespuestaHttp.EnviarArchivo(socketStream, rutaAbsoluta);
                Logger.Registrar(ip, "GET", solicitud.RutaArchivo, 200);
            }
            catch (Exception ex)
            {
                // Captura de errores en el socket.
                Console.WriteLine($"  [ERROR] {ip}: {ex.Message}");
            }
            finally
            {
                _cliente.Close();
            }
        }
    }
}

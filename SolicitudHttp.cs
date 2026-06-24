namespace ServidorWeb
{
    internal class SolicitudHttp
    {
        public string Metodo { get; private set; } = "";
        public string RutaCompleta { get; private set; } = "";
        public string RutaArchivo { get; private set; } = "";     // sin query string
        public string QueryString { get; private set; } = "";      // clave=valor
        public Dictionary<string, string> Headers { get; private set; } = new();
        public string Cuerpo { get; private set; } = "";           // solo para POST

        // Parsea el stream de la conexión y construye la solicitud
        public static SolicitudHttp Parsear(StreamReader reader)
        {
            var solicitud = new SolicitudHttp();

            // Primera línea: "GET" 
            string? primeraLinea = reader.ReadLine();
            if (string.IsNullOrEmpty(primeraLinea)) return solicitud;

            string[] partes = primeraLinea.Split(' ');
            if (partes.Length < 2) return solicitud;

            solicitud.Metodo = partes[0].ToUpper();          // GET, POST
            solicitud.RutaCompleta = partes[1];               // /ruta?param=valor

            // Separar ruta del query string
            int signoInterrogacion = solicitud.RutaCompleta.IndexOf('?');
            if (signoInterrogacion >= 0)
            {
                solicitud.RutaArchivo = solicitud.RutaCompleta[..signoInterrogacion];
                solicitud.QueryString = solicitud.RutaCompleta[(signoInterrogacion + 1)..];
            }
            else
            {
                solicitud.RutaArchivo = solicitud.RutaCompleta;
            }

            // Lee headers línea por línea hasta línea vacía
            int contentLength = 0;
            string? linea;
            while (!string.IsNullOrEmpty(linea = reader.ReadLine()))
            {
                int dospuntos = linea.IndexOf(':');
                if (dospuntos < 0) continue;

                string clave = linea[..dospuntos].Trim();
                string valor = linea[(dospuntos + 1)..].Trim();
                solicitud.Headers[clave] = valor;

                if (clave.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    int.TryParse(valor, out contentLength);
            }

            // Leer cuerpo del POST si hay Content-Length
            if (solicitud.Metodo == "POST" && contentLength > 0)
            {
                char[] buffer = new char[contentLength];
                reader.Read(buffer, 0, contentLength);
                solicitud.Cuerpo = new string(buffer);
            }

            return solicitud;
        }
    }
}

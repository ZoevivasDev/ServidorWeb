namespace ServidorWeb
{
    internal static class Logger
    {
        // El lock es el "semáforo" que impide que dos hilos escriban al mismo tiempo.
        // Si dos visitantes llegan en el mismo instante, uno espera mientras el otro escribe.
        private static readonly object _lock = new object();

        private static readonly string _carpetaLogs = "logs";

        static Logger()
        {
            // Crea la carpeta de logs si no existe
            Directory.CreateDirectory(_carpetaLogs);
        }

        // Escribe una línea en el archivo de log del día actual
        public static void Registrar(string ip, string metodo, string ruta, int codigoRespuesta)
        {
            string fecha = DateTime.Now.ToString("yyyy-MM-dd");
            string hora  = DateTime.Now.ToString("HH:mm:ss");
            string archivoLog = Path.Combine(_carpetaLogs, $"{fecha}.log");

            string linea = $"[{hora}] IP={ip} | {metodo} {ruta} | HTTP {codigoRespuesta}";

            // lock: solo un hilo a la vez puede escribir al archivo
            lock (_lock)
            {
                File.AppendAllText(archivoLog, linea + Environment.NewLine);
            }

            // También mostramos en consola para ver el tráfico en tiempo real
            Console.WriteLine(linea);
        }
    }
}

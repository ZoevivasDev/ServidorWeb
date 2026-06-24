using System.Net;
using System.Net.Sockets;

namespace ServidorWeb
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Cargar la configuración desde archivo externo.
            Configuracion config;


            //tryCatch lo prongo para manjar errores.
            try
            {
                config = Configuracion.Cargar("config.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer config.json: {ex.Message}");
                return;
            }

            //Adorno para mi terminal. 
            Console.WriteLine(" Servidor Web — Blog de Martín ");

            Console.WriteLine($"  Puerto   : {config.Puerto}");
            Console.WriteLine($"  Carpeta  : {config.CarpetaRaiz}");
            Console.WriteLine($"  Logs     : logs/");
            Console.WriteLine();
            Console.WriteLine($"  Escuchando en http://localhost:{config.Puerto}");
            Console.WriteLine("  Ctrl+C para detener\n");

            // Creo el socket TCP servidor.
            var listener = new TcpListener(IPAddress.Any, config.Puerto);
            listener.Start();

            // Bucle principal: acepta conexiones indefinidamente.
            while (true)
            {
                try
                {
                    // Espera bloqueante hasta que llega una conexión
                    TcpClient cliente = listener.AcceptTcpClient();

                    // Cada conexión va a su propio hilo — no bloqueamos el bucle principal
                    // Esto implementa la concurrencia.
                    var manejador = new ManejadorConexion(cliente, config);
                    //creu un hilo y le doy funoin
                    Thread hilo = new Thread(manejador.Atender);
                    //el hilo se cierra si el programa termina
                    hilo.IsBackground = true;
                    //empieza 
                    hilo.Start();
                }
                catch (Exception ex)
                {//x si falla conexion
                    Console.WriteLine($"[ERROR en bucle principal] {ex.Message}");
                }
            }
        }
    }
}

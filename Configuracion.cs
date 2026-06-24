using System.Text.Json;

namespace ServidorWeb
{
    internal class Configuracion
    {
        public int Puerto { get; private set; }
        public string CarpetaRaiz { get; private set; }

        private Configuracion() 
        { 
            CarpetaRaiz = "wwwroot";
        }

        // Carga el archivo config.json y devuelve una instancia 
        public static Configuracion Cargar(string rutaArchivo = "config.json")
        {
            string json = File.ReadAllText(rutaArchivo);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement raiz = doc.RootElement;

            return new Configuracion
            {
                Puerto = raiz.GetProperty("puerto").GetInt32(),
                CarpetaRaiz = raiz.GetProperty("carpetaRaiz").GetString()!
            };
        }
    }
}

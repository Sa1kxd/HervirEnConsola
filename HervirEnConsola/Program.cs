using System;
using System.Diagnostics;
using System.Threading;

namespace SimulacionConsola
{
    class Program
    {
        // Configuraciones iniciales
        static string material = "Agua";
        static double cantidad = 5.0;
        static double diametroTuberia = 15.0;
        static double longitudTuberia = 3.0;

        // Variables de estado
        static double temperaturaActual = 25.0;
        static double gasRestante = 20.0;
        static double presionTanque = 100.0;
        static double tiempoTranscurrido = 0.0;

        // Controles en tiempo real
        static double aperturaTanque = 0.5; // 0.0 a 1.0
        static int posicionEstufa = 0;      // 0 a 5

        // Constantes físicas
        static double TemperaturaEbullicion => material == "Agua" ? 100.0 : 200.0;
        static double CalorEspecifico => material == "Agua" ? 4186.0 : 2000.0;
        static bool simulacionTerminada = false;

        static void Main()
        {
            ConfigurarSimulacion();

            Console.Clear();
            Console.CursorVisible = false;

            // Bucle principal de simulación (Game Loop)
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            double lastTime = 0;

            while (!simulacionTerminada)
            {
                double currentTime = stopwatch.Elapsed.TotalSeconds;
                double deltaTime = currentTime - lastTime;
                lastTime = currentTime;

                ProcesarEntradaUsuario();
                CalcularFisica(deltaTime);
                DibujarDashboard();

                // Pausa corta para no saturar el CPU (aprox 10 FPS)
                Thread.Sleep(100);
            }

            // Mostrar estado final
            DibujarDashboard();
            Console.SetCursorPosition(0, 16);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n¡SIMULACIÓN TERMINADA! El {material} está hirviendo.");
            Console.WriteLine($"Tiempo total: {Math.Round(tiempoTranscurrido, 2)} segundos.");
            Console.ResetColor();
            Console.CursorVisible = true;
        }

        static void ConfigurarSimulacion()
        {
            Console.Clear();
            Console.WriteLine("=== CONFIGURACIÓN DE LA SIMULACIÓN ===");

            Console.Write("Material (1: Agua, 2: Aceite) [Por defecto: 1]: ");
            string op = Console.ReadLine();
            if (op == "2") material = "Aceite";

            Console.Write("Cantidad (Litros/Kg) [Por defecto: 5]: ");
            if (double.TryParse(Console.ReadLine(), out double c)) cantidad = c;

            Console.Write("Diámetro Tubería (mm) [Por defecto: 15]: ");
            if (double.TryParse(Console.ReadLine(), out double d)) diametroTuberia = d;

            Console.Write("Longitud Tubería (m) [Por defecto: 3]: ");
            if (double.TryParse(Console.ReadLine(), out double l)) longitudTuberia = l;
        }

        static void ProcesarEntradaUsuario()
        {
            // Verificamos si el usuario presionó una tecla sin detener la simulación
            if (Console.KeyAvailable)
            {
                var tecla = Console.ReadKey(true).Key;

                switch (tecla)
                {
                    case ConsoleKey.W: // Abrir válvula
                        aperturaTanque = Math.Min(1.0, aperturaTanque + 0.1);
                        break;
                    case ConsoleKey.S: // Cerrar válvula
                        aperturaTanque = Math.Max(0.0, aperturaTanque - 0.1);
                        break;
                    case ConsoleKey.D: // Subir estufa
                    case ConsoleKey.RightArrow:
                        posicionEstufa = Math.Min(5, posicionEstufa + 1);
                        break;
                    case ConsoleKey.A: // Bajar estufa
                    case ConsoleKey.LeftArrow:
                        posicionEstufa = Math.Max(0, posicionEstufa - 1);
                        break;
                    case ConsoleKey.Q: // Salir de emergencia
                        Environment.Exit(0);
                        break;
                }
            }
        }

        static void CalcularFisica(double dt)
        {
            tiempoTranscurrido += dt;

            // 1. Presión del Tanque (depende del gas restante)
            presionTanque = (gasRestante / 20.0) * 100.0;
            if (presionTanque <= 0) presionTanque = 0;

            // 2. Dinámica de Fluidos
            double k_tuberia = (Math.Pow(diametroTuberia, 2) / (longitudTuberia * 10.0));
            double flujoGas = (presionTanque / 100.0) * aperturaTanque * k_tuberia;

            // 3. Consumo de Gas
            if (posicionEstufa > 0)
            {
                gasRestante -= flujoGas * (posicionEstufa / 5.0) * 0.05 * dt;
            }
            if (gasRestante < 0) gasRestante = 0;

            // 4. Termodinámica (Calor)
            double poderCalorifico = flujoGas * (posicionEstufa / 5.0) * 8000.0;
            double incrementoTemp = (poderCalorifico / (cantidad * CalorEspecifico)) * dt;

            // 5. Disipación térmica (Enfriamiento hacia los 25°C ambiente)
            double disipacion = (temperaturaActual - 25.0) * 0.01 * dt;

            temperaturaActual += (incrementoTemp - disipacion);
            if (temperaturaActual < 25.0) temperaturaActual = 25.0;

            // 6. Condición de victoria
            if (temperaturaActual >= TemperaturaEbullicion)
            {
                temperaturaActual = TemperaturaEbullicion;
                simulacionTerminada = true;
            }
        }

        static void DibujarDashboard()
        {
            // Volver al inicio de la consola para sobreescribir (evita parpadeo)
            Console.SetCursorPosition(0, 0);

            Console.WriteLine("==================================================");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("       SIMULADOR TERMODINÁMICO EN CONSOLA       ");
            Console.ResetColor();
            Console.WriteLine("==================================================");

            Console.WriteLine($"Material: {material} (Ebullición: {TemperaturaEbullicion}°C)");
            Console.WriteLine($"Olla: {cantidad} L/Kg | Tubería: {diametroTuberia}mm x {longitudTuberia}m");
            Console.WriteLine("--------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[CONTROLES EN TIEMPO REAL]");
            Console.ResetColor();
            Console.WriteLine($"(W/S) Válvula Tanque : {Math.Round(aperturaTanque * 100)}%   ");
            Console.WriteLine($"(A/D) Posición Estufa: {posicionEstufa} {ObtenerBarra(posicionEstufa, 5, '#')}  ");
            Console.WriteLine("(Q)   Salir");
            Console.WriteLine("--------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("[TELEMETRÍA]");
            Console.ResetColor();
            Console.WriteLine($"Tiempo Transcurrido  : {tiempoTranscurrido:F1} s     ");
            Console.WriteLine($"Gas Restante         : {gasRestante:F2} kg    ");
            Console.WriteLine($"Presión del Tanque   : {presionTanque:F1} psi   ");
            Console.WriteLine("--------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ESTADO DE LA OLLA]");
            Console.ResetColor();
            Console.WriteLine($"Temperatura Actual   : {temperaturaActual:F1} °C    ");

            double progreso = (temperaturaActual / TemperaturaEbullicion) * 100;
            Console.WriteLine($"Nivel de Calor       : {ObtenerBarra((int)progreso, 100, '=', 20)} {progreso:F0}%    ");
            Console.WriteLine("==================================================");
        }

        // Helper para dibujar barras de progreso en consola
        static string ObtenerBarra(int valorActual, int valorMaximo, char caracter, int longitudBarra = 5)
        {
            int llenos = (int)Math.Round((double)valorActual / valorMaximo * longitudBarra);
            llenos = Math.Clamp(llenos, 0, longitudBarra);
            string barra = new string(caracter, llenos);
            string vacio = new string('-', longitudBarra - llenos);
            return $"[{barra}{vacio}]";
        }
    }
}
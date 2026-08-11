using MonteCarloSIR.Simulador;
using MonteCarloSIR.Simulador.ParallelSim;
using MonteCarloSIR.Simulador.SequentialSim;
using System.Diagnostics;

namespace MonteCarloSIR
{
    class Program
    {
        const int INTERVALO_FRAMES = 5;

        static void Main(string[] args)
        {
            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                string? opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        EjecutarSecuencial(new ParametrosSimulacion(), guardarFrames: true, carpetaSalida: "Data/Secuencial");
                        break;
                    case "2":
                        EjecutarSecuencial(
                            new ParametrosSimulacion(tamGrilla: 20, diasSimulacion: 30, infectadosIniciales: 2),
                            guardarFrames: false,
                            carpetaSalida: "Data/Secuencial");
                        break;
                    case "3":
                        EjecutarParalelo(new ParametrosSimulacion(), Environment.ProcessorCount,
                            guardarFrames: true, carpetaSalida: "Data/Paralelo");
                        break;
                    case "4":
                        EjecutarParalelo(
                            new ParametrosSimulacion(tamGrilla: 20, diasSimulacion: 30, infectadosIniciales: 2),
                            gradoParalelismo: 4, guardarFrames: false, carpetaSalida: "Data/Paralelo");
                        break;
                    case "5":
                        BenchmarkScaling.Ejecutar(new ParametrosSimulacion(), new[] { 1, 2, 4, 8 }, "Data");
                        break;
                    case "6":
                        salir = true;
                        break;
                    default:
                        Console.WriteLine("Opcion invalida.\n");
                        break;
                }
            }
        }

        static void MostrarMenu()
        {
            Console.WriteLine("=== Simulacion Monte-Carlo de Epidemias (SIR) ===");
            Console.WriteLine("1. Secuencial - Simulacion completa (1000x1000, 365 dias)");
            Console.WriteLine("2. Secuencial - Caso pequeno de validacion (20x20, 30 dias)");
            Console.WriteLine("3. Paralelo   - Simulacion completa (1000x1000, 365 dias)");
            Console.WriteLine("4. Paralelo   - Caso pequeno de validacion (20x20, 30 dias)");
            Console.WriteLine("5. Paralelo   - Benchmark de strong scaling (1,2,4,8 hilos)");
            Console.WriteLine("6. Salir");
            Console.Write("Elige una opcion: ");
        }

        static void EjecutarSecuencial(ParametrosSimulacion parametros, bool guardarFrames, string carpetaSalida)
        {
            Directory.CreateDirectory(carpetaSalida);

            var grillaActual = new Grilla(parametros.TamGrilla);
            var grillaSiguiente = new Grilla(parametros.TamGrilla);
            grillaActual.InicializarPoblacion(parametros.Semilla, parametros.InfectadosIniciales);

            var motor = new MotorSimulacion(parametros);
            var historial = new List<EstadisticasDiarias>();

            int infectadosAyer = parametros.InfectadosIniciales;
            var cronometro = Stopwatch.StartNew();

            if (guardarFrames)
                grillaActual.GuardarComoBinario(Path.Combine(carpetaSalida, "frame_000.bin"));

            for (int dia = 1; dia <= parametros.DiasSimulacion; dia++)
            {
                var stats = motor.SimularDia(grillaActual, grillaSiguiente, dia, infectadosAyer);
                historial.Add(stats);
                infectadosAyer = stats.Infectados;

                (grillaActual, grillaSiguiente) = (grillaSiguiente, grillaActual);

                if (guardarFrames && dia % INTERVALO_FRAMES == 0)
                    grillaActual.GuardarComoBinario(Path.Combine(carpetaSalida, $"frame_{dia:D3}.bin"));

                if (stats.Infectados == 0)
                {
                    Console.WriteLine($"Epidemia extinguida en el dia {dia}.");
                    break;
                }
            }

            cronometro.Stop();
            Console.WriteLine($"\n[SECUENCIAL] Completado en {cronometro.ElapsedMilliseconds} ms " +
                               $"({parametros.TamGrilla}x{parametros.TamGrilla}, {historial.Count} dias)");
            Console.WriteLine($"R0 teorico: {parametros.R0Teorico:F2}");

            GuardarCsv(historial, Path.Combine(carpetaSalida, "estadisticas_diarias.csv"));
            File.WriteAllText(Path.Combine(carpetaSalida, "tiempo_ms.txt"), cronometro.ElapsedMilliseconds.ToString());
            Console.WriteLine();
        }

        static void EjecutarParalelo(ParametrosSimulacion parametros, int gradoParalelismo, bool guardarFrames, string carpetaSalida)
        {
            Directory.CreateDirectory(carpetaSalida);

            var grillaActual = new Grilla(parametros.TamGrilla);
            var grillaSiguiente = new Grilla(parametros.TamGrilla);
            grillaActual.InicializarPoblacion(parametros.Semilla, parametros.InfectadosIniciales);

            var motor = new MotorSimulacionParalelo(parametros, gradoParalelismo);
            var historial = new List<EstadisticasDiarias>();

            int infectadosAyer = parametros.InfectadosIniciales;
            var cronometro = Stopwatch.StartNew();

            if (guardarFrames)
                grillaActual.GuardarComoBinario(Path.Combine(carpetaSalida, "frame_000.bin"));

            for (int dia = 1; dia <= parametros.DiasSimulacion; dia++)
            {
                var stats = motor.SimularDia(grillaActual, grillaSiguiente, dia, infectadosAyer);
                historial.Add(stats);
                infectadosAyer = stats.Infectados;

                (grillaActual, grillaSiguiente) = (grillaSiguiente, grillaActual);

                if (guardarFrames && dia % INTERVALO_FRAMES == 0)
                    grillaActual.GuardarComoBinario(Path.Combine(carpetaSalida, $"frame_{dia:D3}.bin"));

                if (stats.Infectados == 0)
                {
                    Console.WriteLine($"Epidemia extinguida en el dia {dia}.");
                    break;
                }
            }

            cronometro.Stop();
            Console.WriteLine($"\n[PARALELO x{gradoParalelismo}] Completado en {cronometro.ElapsedMilliseconds} ms " +
                               $"({parametros.TamGrilla}x{parametros.TamGrilla}, {historial.Count} dias)");

            GuardarCsv(historial, Path.Combine(carpetaSalida, "estadisticas_diarias.csv"));
            File.WriteAllText(Path.Combine(carpetaSalida, "tiempo_ms.txt"), cronometro.ElapsedMilliseconds.ToString());
            Console.WriteLine();
        }

        static void GuardarCsv(List<EstadisticasDiarias> historial, string ruta)
        {
            using var writer = new StreamWriter(ruta);
            writer.WriteLine("Dia,Susceptibles,Infectados,Recuperados,NuevosContagios,R0Efectivo");
            foreach (var e in historial)
                writer.WriteLine($"{e.Dia},{e.Susceptibles},{e.Infectados},{e.Recuperados},{e.NuevosContagiosHoy},{e.R0Efectivo:F4}");
        }
    }
}
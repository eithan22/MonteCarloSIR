using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloSIR.Simulador.ParallelSim
{
    // Corre la MISMA simulacion con 1, 2, 4 y 8 hilos, y mide el tiempo de cada corrida.
    public static class BenchmarkScaling
    {
        public static void Ejecutar(ParametrosSimulacion parametros, int[] gradosDeHilos, string carpetaSalida)
        {
            Directory.CreateDirectory(carpetaSalida);
            var resultados = new List<(int hilos, long tiempoMs)>();

            Console.WriteLine($"Nucleos logicos disponibles en esta maquina: {Environment.ProcessorCount}\n");

            foreach (int hilos in gradosDeHilos)
            {
                Console.WriteLine($"--- Corriendo con {hilos} hilo(s) ---");

                var grillaActual = new Grilla(parametros.TamGrilla);
                var grillaSiguiente = new Grilla(parametros.TamGrilla);
                grillaActual.InicializarPoblacion(parametros.Semilla, parametros.InfectadosIniciales);

                var motor = new MotorSimulacionParalelo(parametros, hilos);
                int infectadosAyer = parametros.InfectadosIniciales;

                var cronometro = Stopwatch.StartNew();

                for (int dia = 1; dia <= parametros.DiasSimulacion; dia++)
                {
                    var stats = motor.SimularDia(grillaActual, grillaSiguiente, dia, infectadosAyer);
                    infectadosAyer = stats.Infectados;
                    (grillaActual, grillaSiguiente) = (grillaSiguiente, grillaActual);

                    if (stats.Infectados == 0) break;
                }

                cronometro.Stop();
                resultados.Add((hilos, cronometro.ElapsedMilliseconds));
                Console.WriteLine($"    Tiempo: {cronometro.ElapsedMilliseconds} ms\n");
            }

            long tiempoBase = resultados[0].tiempoMs;

            string ruta = Path.Combine(carpetaSalida, "scaling.csv");
            using (var writer = new StreamWriter(ruta))
            {
                writer.WriteLine("Hilos,TiempoMs,SpeedUp,Eficiencia");
                foreach (var (hilos, tiempoMs) in resultados)
                {
                    double speedUp = tiempoMs > 0 ? (double)tiempoBase / tiempoMs : 0;
                    double eficiencia = speedUp / hilos;
                    writer.WriteLine($"{hilos},{tiempoMs},{speedUp:F3},{eficiencia:F3}");
                }
            }

            Console.WriteLine($"Resultados de scaling guardados en {ruta}");
        }
    }
}

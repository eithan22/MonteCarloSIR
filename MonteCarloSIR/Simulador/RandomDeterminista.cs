using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloSIR.Simulador
{
    // Genera numeros aleatorios que dependen SOLO de la posicion (fila, columna, dia),
    // no del orden de calculo. Por eso secuencial y paralelo dan el mismo resultado.
    public static class RandomDeterminista
    {
        public static double Siguiente(int semilla, int dia, int fila, int columna, int intento)
        {
            unchecked
            {
                ulong x = (ulong)(uint)semilla;
                x = x * 6364136223846793005UL + (ulong)(uint)dia * 1000000007UL;
                x ^= (ulong)(uint)fila * 2654435761UL;
                x = x * 6364136223846793005UL + (ulong)(uint)columna * 2246822519UL;
                x ^= (ulong)(uint)intento * 3266489917UL;

                x ^= x >> 33;
                x *= 0xff51afd7ed558ccdUL;
                x ^= x >> 33;
                x *= 0xc4ceb9fe1a85ec53UL;
                x ^= x >> 33;

                return (x >> 11) * (1.0 / (1UL << 53));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloSIR.Simulador
{
    public class Grilla
    {
        private readonly EstadoPersona[,] _celdas;
        public int Tamano { get; }

        public Grilla(int tamano)
        {
            Tamano = tamano;
            _celdas = new EstadoPersona[tamano, tamano];
        }

        public EstadoPersona Obtener(int fila, int columna) => _celdas[fila, columna];

        public void Establecer(int fila, int columna, EstadoPersona estado)
        {
            _celdas[fila, columna] = estado;
        }

        public void InicializarPoblacion(int semilla, int infectadosIniciales)
        {
            for (int f = 0; f < Tamano; f++)
                for (int c = 0; c < Tamano; c++)
                    _celdas[f, c] = EstadoPersona.Susceptible;

            int sembrados = 0;
            int intento = 0;
            while (sembrados < infectadosIniciales)
            {
                double r1 = RandomDeterminista.Siguiente(semilla, -1, intento, 0, 0);
                double r2 = RandomDeterminista.Siguiente(semilla, -1, intento, 1, 0);
                int f = (int)(r1 * Tamano);
                int c = (int)(r2 * Tamano);
                intento++;

                if (_celdas[f, c] == EstadoPersona.Susceptible)
                {
                    _celdas[f, c] = EstadoPersona.Infectado;
                    sembrados++;
                }
            }
        }

        public int ContarVecinosInfectados(int fila, int columna)
        {
            int contador = 0;

            int norte = (fila - 1 + Tamano) % Tamano;
            int sur = (fila + 1) % Tamano;
            int oeste = (columna - 1 + Tamano) % Tamano;
            int este = (columna + 1) % Tamano;

            if (_celdas[norte, columna] == EstadoPersona.Infectado) contador++;
            if (_celdas[sur, columna] == EstadoPersona.Infectado) contador++;
            if (_celdas[fila, oeste] == EstadoPersona.Infectado) contador++;
            if (_celdas[fila, este] == EstadoPersona.Infectado) contador++;

            return contador;
        }

        public EstadoPersona[] ObtenerFila(int fila)
        {
            var resultado = new EstadoPersona[Tamano];
            for (int c = 0; c < Tamano; c++)
                resultado[c] = _celdas[fila, c];
            return resultado;
        }

        public void GuardarComoBinario(string ruta)
        {
            using var stream = new FileStream(ruta, FileMode.Create);
            var buffer = new byte[Tamano * Tamano];
            int idx = 0;
            for (int f = 0; f < Tamano; f++)
                for (int c = 0; c < Tamano; c++)
                    buffer[idx++] = (byte)_celdas[f, c];
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}

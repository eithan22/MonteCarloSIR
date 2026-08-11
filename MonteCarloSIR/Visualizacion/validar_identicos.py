"""
Compara los CSV de la validacion pequena (secuencial vs paralelo) y confirma
que dan resultados IDENTICOS. Esta es la prueba de que ghost-cells + el
generador aleatorio deterministico estan bien implementados.
"""
import pandas as pd

RUTA_SEC = "../Data/Secuencial/estadisticas_diarias.csv"
RUTA_PAR = "../Data/Paralelo/estadisticas_diarias.csv"


def validar():
    df_sec = pd.read_csv(RUTA_SEC)
    df_par = pd.read_csv(RUTA_PAR)

    if df_sec.equals(df_par):
        print("EXITO: Los resultados secuencial y paralelo son IDENTICOS.")
        print(f"   Dias comparados: {len(df_sec)}")
        return True

    print("DIFERENCIA ENCONTRADA entre secuencial y paralelo:")
    print(df_sec.compare(df_par))
    return False


if __name__ == "__main__":
    validar()
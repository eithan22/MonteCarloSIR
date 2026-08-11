"""
Lee Data/scaling.csv (generado por la opcion 5 del menu) y dibuja la grafica
de speed-up: tiempo real vs. el speed-up "ideal" si la paralelizacion fuera perfecta.
"""
import pandas as pd
import matplotlib.pyplot as plt

RUTA_CSV = "../Data/scaling.csv"
RUTA_SALIDA = "../Data/speedup_graph.png"


def graficar_scaling(ruta_csv=RUTA_CSV, ruta_salida=RUTA_SALIDA):
    df = pd.read_csv(ruta_csv)
    df = df.sort_values("Hilos")

    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(12, 5))

    ax1.plot(df["Hilos"], df["SpeedUp"], marker="o", linewidth=2,
              label="Speed-up medido", color="#2563eb")
    ax1.plot(df["Hilos"], df["Hilos"], linestyle="--", color="gray",
              label="Speed-up ideal (lineal)")
    ax1.set_xlabel("Numero de hilos")
    ax1.set_ylabel("Speed-up (Tiempo 1 hilo / Tiempo N hilos)")
    ax1.set_title("Strong Scaling - Speed-up")
    ax1.legend()
    ax1.grid(True, alpha=0.3)

    ax2.bar(df["Hilos"].astype(str), df["TiempoMs"], color="#16a34a")
    ax2.set_xlabel("Numero de hilos")
    ax2.set_ylabel("Tiempo total (ms)")
    ax2.set_title("Tiempo de ejecucion por numero de hilos")
    ax2.grid(True, alpha=0.3, axis="y")

    plt.tight_layout()
    plt.savefig(ruta_salida, dpi=150)
    print(f"Grafica guardada en {ruta_salida}")


if __name__ == "__main__":
    graficar_scaling()

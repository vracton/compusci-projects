import os
import io
import numpy as np
import matplotlib.pyplot as plt


DATA_FILE = os.path.join(os.path.dirname(__file__), "data", "levelthree.txt")

LINE_COLOR = "#fab387"
BG_COLOR = "#1e1e2e"
TEXT_COLOR = "#cdd6f4"


def load_data(path: str) -> np.ndarray:
    with open(path, "r", encoding="utf-8") as file:
        lines = file.readlines()

    numeric_lines = []
    for line in lines[1:]:
        parts = line.strip().split()
        if not parts:
            continue
        try:
            float(parts[0])
        except ValueError:
            continue
        numeric_lines.append(line)

    data = np.loadtxt(io.StringIO("".join(numeric_lines)))
    return data[:, :4]


def plot_trajectory(data: np.ndarray) -> None:
    x = data[:, 1]
    y = data[:, 2]
    z = data[:, 3]

    fig = plt.figure(facecolor=BG_COLOR)
    ax = fig.add_subplot(111, projection="3d")
    ax.set_facecolor(BG_COLOR)

    ax.xaxis.pane.set_facecolor(BG_COLOR)
    ax.yaxis.pane.set_facecolor(BG_COLOR)
    ax.zaxis.pane.set_facecolor(BG_COLOR)

    ax.plot3D(x, y, z, color=LINE_COLOR, linewidth=2)

    ax.set_xlabel("X", color=TEXT_COLOR)
    ax.set_ylabel("Y", color=TEXT_COLOR)
    ax.set_zlabel("Z", color=TEXT_COLOR)
    ax.set_title("Projectile Trajectory", color=TEXT_COLOR)

    ax.tick_params(colors=TEXT_COLOR)
    ax.xaxis.label.set_color(TEXT_COLOR)
    ax.yaxis.label.set_color(TEXT_COLOR)
    ax.zaxis.label.set_color(TEXT_COLOR)

    for axis in (ax.xaxis, ax.yaxis, ax.zaxis):
        axis._axinfo["grid"]["color"] = TEXT_COLOR
        axis._axinfo["tick"]["color"] = TEXT_COLOR
        axis._axinfo["axisline"]["color"] = TEXT_COLOR

    plt.show()


def main() -> None:
    data = load_data(DATA_FILE)
    plot_trajectory(data)


if __name__ == "__main__":
    main()
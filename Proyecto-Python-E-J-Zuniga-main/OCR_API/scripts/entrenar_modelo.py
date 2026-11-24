"""Entrenamiento legacy - usa el pipeline reproducible de Fase 1.

Mantiene compatibilidad con el nombre anterior, pero delega en entrenar_v2.py
para generar un Pipeline sklearn y guardar en CIVIL_PENAL_MODEL_PATH.
"""

from __future__ import annotations

import runpy
from pathlib import Path


if __name__ == "__main__":
    script = Path(__file__).resolve().parent / "entrenar_v2.py"
    print("⚠️ entrenar_modelo.py está en modo compatibilidad. Delegando a entrenar_v2.py")
    runpy.run_path(str(script), run_name="__main__")

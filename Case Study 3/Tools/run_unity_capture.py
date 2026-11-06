import os
import subprocess
import sys

unity_exe = r"C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe"
project_path = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "UnityProject"))
log_file = os.path.abspath(os.path.join(project_path, "unity_capture.log"))

cmd = [
    unity_exe,
    "-quit",
    "-batchmode",
    "-projectPath", project_path,
    "-executeMethod", "InteriorStudio.Editor.CaptureInteriorStudioOutputs.RunCapture",
    "-logFile", log_file
]

print(f"Executing Unity 6 batchmode capture...")
print(f"Project: {project_path}")
print(f"Log: {log_file}")
res = subprocess.run(cmd)
print(f"Unity Exit Code: {res.returncode}")

if os.path.exists(log_file):
    print("\n--- Last 50 lines of unity_capture.log ---")
    with open(log_file, "r", encoding="utf-8", errors="ignore") as f:
        lines = f.readlines()
        print("".join(lines[-50:]))
else:
    print(f"Log file {log_file} not found.")

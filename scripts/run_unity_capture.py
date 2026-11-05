import os
import subprocess
import sys

unity_exe = r"C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe"
project_path = os.path.abspath("Case Study/AutoVR")
log_file = os.path.abspath("Case Study/AutoVR/unity_capture.log")

cmd = [
    unity_exe,
    "-quit",
    "-batchmode",
    "-projectPath", project_path,
    "-executeMethod", "CaptureAutoVRUnityOutputs.RunCapture",
    "-logFile", log_file
]

print(f"Executing: {' '.join(cmd)}")
res = subprocess.run(cmd)
print(f"Exit code: {res.returncode}")

if os.path.exists(log_file):
    print("\n--- Last 30 lines of unity_capture.log ---")
    with open(log_file, "r", encoding="utf-8", errors="ignore") as f:
        lines = f.readlines()
        print("".join(lines[-30:]))
else:
    print(f"Log file {log_file} does not exist.")

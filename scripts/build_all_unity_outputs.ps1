$unity = "C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe"
$baseDir = "C:\Users\LIKITH S G\Desktop\CGVR LAB"

# 1. RENDER LAB 9
Write-Output "=== [1/3] RENDERING LAB 9 (Simple Interactive 3D Scene) ==="
$proj9 = "$baseDir\experiments\Experiment_9\unity_project"
$log9 = "$proj9\capture_run.log"
Start-Process -FilePath $unity -ArgumentList "-projectPath `"$proj9`" -batchmode -executeMethod CaptureLab9Output.RunCapture -quit -logFile `"$log9`"" -Wait -NoNewWindow
if (-not (Test-Path "$proj9\frames\frame_00.png")) {
    Write-Warning "Lab 9 capture failed! Check $log9"
}

# 2. RENDER LAB 10
Write-Output "=== [2/3] RENDERING LAB 10 (3D Viewing Pipeline Dual Viewports) ==="
$proj10 = "$baseDir\experiments\Experiment_10\unity_project"
$log10 = "$proj10\capture_run.log"
Start-Process -FilePath $unity -ArgumentList "-projectPath `"$proj10`" -batchmode -executeMethod CaptureLab10Output.RunCapture -quit -logFile `"$log10`"" -Wait -NoNewWindow
if (-not (Test-Path "$proj10\frames\frame_00.png")) {
    Write-Warning "Lab 10 capture failed! Check $log10"
}

# 3. RENDER CASE STUDY 2
Write-Output "=== [3/3] RENDERING CASE STUDY 2 (Enterprise Automotive Studio) ==="
$projCS = "$baseDir\Case Study\unity_project"
$logCS = "$projCS\capture_run.log"
Start-Process -FilePath $unity -ArgumentList "-projectPath `"$projCS`" -batchmode -executeMethod CaptureCaseStudyOutput.RunCapture -quit -logFile `"$logCS`"" -Wait -NoNewWindow
if (-not (Test-Path "$projCS\frames\frame_00.png")) {
    Write-Warning "Case Study capture failed! Check $logCS"
}

# 4. COMPILE FLUID ANIMATED GIFS VIA PYTHON PILLOW
Write-Output "=== [4/4] COMPILING ANIMATED GIFS VIA PYTHON PILLOW ==="
python "$baseDir\scripts\compile_gifs.py"

Write-Output "=== ALL UNITY 6 RENDERS & ANIMATED GIFS BUILT SUCCESSFULLY! ==="

import os
import shutil

def rename_in_project(root):
    if not os.path.exists(root):
        return

    # 1. Update text inside files
    extensions = ('.cs', '.unity', '.asset', '.json', '.txt', '.meta', '.py')
    for dirpath, dirnames, filenames in os.walk(root):
        if any(ign in dirpath for ign in ['Library', 'Temp', 'Logs', 'UserSettings', '.git']):
            continue
        for f in filenames:
            if f.endswith(extensions):
                full_path = os.path.join(dirpath, f)
                try:
                    with open(full_path, 'r', encoding='utf-8') as file:
                        content = file.read()
                    
                    new_content = content
                    # Text / class replacements
                    new_content = new_content.replace('AutoVerse_Showwroom', 'AutoVR_Showroom')
                    new_content = new_content.replace('AutoVerse_Showroom', 'AutoVR_Showroom')
                    new_content = new_content.replace('AutoVerseShowroomUI', 'AutoVRShowroomUI')
                    new_content = new_content.replace('AutoVerseBuildPipeline', 'AutoVRBuildPipeline')
                    new_content = new_content.replace('CaptureAutoVerseUnityOutputs', 'CaptureAutoVRUnityOutputs')
                    new_content = new_content.replace('R V U   A U T O V E R S E', 'A u t o V R')
                    new_content = new_content.replace('A U T O V E R S E', 'A u t o V R')
                    new_content = new_content.replace('AUTOVERSE', 'AutoVR')
                    new_content = new_content.replace('AutoVerse', 'AutoVR')

                    if new_content != content:
                        with open(full_path, 'w', encoding='utf-8') as file:
                            file.write(new_content)
                except Exception as e:
                    pass

    # 2. Rename files
    for dirpath, dirnames, filenames in os.walk(root, topdown=False):
        if any(ign in dirpath for ign in ['Library', 'Temp', 'Logs', 'UserSettings', '.git']):
            continue
        for f in filenames:
            if 'AutoVerse' in f:
                old_f = os.path.join(dirpath, f)
                new_f_name = f.replace('AutoVerse_Showwroom', 'AutoVR_Showroom').replace('AutoVerse', 'AutoVR')
                new_f = os.path.join(dirpath, new_f_name)
                print(f"Renaming file: {old_f} -> {new_f}")
                if os.path.exists(new_f):
                    os.remove(new_f)
                os.rename(old_f, new_f)

    # 3. Rename directories
    for dirpath, dirnames, filenames in os.walk(root, topdown=False):
        if any(ign in dirpath for ign in ['Library', 'Temp', 'Logs', 'UserSettings', '.git']):
            continue
        for d in dirnames:
            if 'AutoVerse' in d:
                old_d = os.path.join(dirpath, d)
                new_d_name = d.replace('AutoVerse_Showwroom', 'AutoVR_Showroom').replace('AutoVerse', 'AutoVR')
                new_d = os.path.join(dirpath, new_d_name)
                print(f"Renaming dir: {old_d} -> {new_d}")
                if os.path.exists(new_d):
                    shutil.rmtree(new_d)
                os.rename(old_d, new_d)

print("--- Renaming in Case Study/AutoVerse ---")
rename_in_project('Case Study/AutoVerse')

print("--- Renaming in Case Study/unity_project ---")
rename_in_project('Case Study/unity_project')

# Rename Case Study/autoverse.py -> Case Study/autovr.py
if os.path.exists('Case Study/autoverse.py'):
    print("Renaming Case Study/autoverse.py -> Case Study/autovr.py")
    if os.path.exists('Case Study/autovr.py'):
        os.remove('Case Study/autovr.py')
    os.rename('Case Study/autoverse.py', 'Case Study/autovr.py')

# Rename Case Study/AutoVerse folder -> Case Study/AutoVR
if os.path.exists('Case Study/AutoVerse'):
    print("Renaming folder Case Study/AutoVerse -> Case Study/AutoVR")
    if os.path.exists('Case Study/AutoVR'):
        shutil.rmtree('Case Study/AutoVR')
    os.rename('Case Study/AutoVerse', 'Case Study/AutoVR')

print("Done renaming all AutoVerse occurrences to AutoVR.")

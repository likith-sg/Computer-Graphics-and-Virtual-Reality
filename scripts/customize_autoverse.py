import os

def customize_project(base_dir):
    env_path = os.path.join(base_dir, 'Assets/Scripts/Showroom/ShowroomEnvironmentManager.cs')
    if os.path.exists(env_path):
        with open(env_path, 'r', encoding='utf-8') as f:
            c = f.read()
        c = c.replace('A U T O V E R S E', 'R V U   A U T O V E R S E')
        c = c.replace('THE AUTOMOTIVE ATELIER', 'COLLABORATIVE VR CAD STUDIO | BENGALURU - DETROIT')
        c = c.replace('new Vector3(9.5f, 1.7f, 0.12f)', 'new Vector3(11.8f, 1.8f, 0.12f)')
        with open(env_path, 'w', encoding='utf-8') as f:
            f.write(c)

    light_path = os.path.join(base_dir, 'Assets/Scripts/Showroom/ShowroomLightingManager.cs')
    if os.path.exists(light_path):
        with open(light_path, 'r', encoding='utf-8') as f:
            c = f.read()
        c = c.replace('"Luxury Studio"', '"Bengaluru Studio"')
        c = c.replace('"Midnight Studio"', '"Detroit Proving Grounds"')
        c = c.replace('"Bright Gallery"', '"CAD Inspection Pavilion"')
        c = c.replace('"Neon Garage"', '"Cyberpunk Prototyping Bay"')
        with open(light_path, 'w', encoding='utf-8') as f:
            f.write(c)

    ui_path = os.path.join(base_dir, 'Assets/Scripts/UI/AutoVerseShowroomUI.cs')
    if os.path.exists(ui_path):
        with open(ui_path, 'r', encoding='utf-8') as f:
            c = f.read()
        c = c.replace('"AUTOVERSE", 16, FontStyle.Bold, colAccentCyan', '"RVU AUTOVERSE", 15, FontStyle.Bold, colAccentCyan')
        c = c.replace('"ATELIER", 11, FontStyle.Normal, colTextMuted', '"CAD VR (BLR-DET)", 10, FontStyle.Normal, colAccentGold')
        with open(ui_path, 'w', encoding='utf-8') as f:
            f.write(c)

customize_project('Case Study/AutoVerse')
customize_project('Case Study/unity_project')
print('Successfully customized AutoVerse scripts across both project folders.')

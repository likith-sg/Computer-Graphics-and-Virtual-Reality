import re

with open('Case Study/README.md', 'r', encoding='utf-8') as f:
    text = f.read()

# Add RVU logo header if not present
if 'rvu_logo.png' not in text:
    logo_header = '<p align="center">\n  <img src="rvu_logo.png" alt="RV University Logo" width="300" />\n</p>\n\n'
    text = logo_header + text

# Replace variations
text = text.replace('AutoVerse VR', 'AutoVR')
text = text.replace('AUTOVERSE VR', 'AUTOVR')
text = text.replace('AutoVerse', 'AutoVR')
text = text.replace('AUTOVERSE', 'AUTOVR')
text = text.replace('autoverse.py', 'autovr.py')
text = text.replace('AutoVerse_Showwroom.unity', 'AutoVR_Showroom.unity')
text = text.replace('AutoVerse_Showroom.unity', 'AutoVR_Showroom.unity')

with open('Case Study/README.md', 'w', encoding='utf-8') as f:
    f.write(text)

with open('README.md', 'r', encoding='utf-8') as f:
    root_text = f.read()

root_text = root_text.replace('AutoVerse VR', 'AutoVR')
root_text = root_text.replace('AUTOVERSE VR', 'AUTOVR')
root_text = root_text.replace('AutoVerse', 'AutoVR')
root_text = root_text.replace('AUTOVERSE', 'AUTOVR')
root_text = root_text.replace('autoverse.py', 'autovr.py')
root_text = root_text.replace('AutoVerse_Showwroom.unity', 'AutoVR_Showroom.unity')
root_text = root_text.replace('AutoVerse_Showroom.unity', 'AutoVR_Showroom.unity')

with open('README.md', 'w', encoding='utf-8') as f:
    f.write(root_text)

print('Updated both README files successfully!')

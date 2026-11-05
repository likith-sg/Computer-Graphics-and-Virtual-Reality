path = "Case Study/autoverse.py"
with open(path, "r", encoding="utf-8") as f:
    c = f.read()

c = c.replace('"Luxury Studio"', '"Bengaluru Studio"')
c = c.replace('"Midnight Studio"', '"Detroit Proving Grounds"')
c = c.replace('"Bright Gallery"', '"CAD Inspection Pavilion"')
c = c.replace('"Neon Garage"', '"Cyberpunk Prototyping Bay"')

with open(path, "w", encoding="utf-8") as f:
    f.write(c)

print("Updated autoverse.py studio lighting names.")

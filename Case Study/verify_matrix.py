import numpy as np

def compute_matrices():
    print("=" * 70)
    print("CASE STUDY 2: WHEEL RIM COMPOSITE TRANSFORMATION MATRIX")
    print("=" * 70)

    # 1. Scale Matrix S (uniform scale 1.5)
    s = 1.5
    S = np.array([
        [s, 0, 0, 0],
        [0, s, 0, 0],
        [0, 0, s, 0],
        [0, 0, 0, 1]
    ], dtype=float)

    # 2. Rotation 45 degrees
    # Angle theta = 45 deg = pi / 4
    theta = np.radians(45.0)
    c = np.cos(theta) # 0.70710678118
    s_val = np.sin(theta) # 0.70710678118

    # Case A: Rotation about Z-axis (Rolling / Rim Face Rotation)
    R_z = np.array([
        [ c, -s_val, 0, 0],
        [ s_val,  c, 0, 0],
        [ 0,      0, 1, 0],
        [ 0,      0, 0, 1]
    ], dtype=float)

    # Case B: Rotation about Y-axis (Steering / Yaw Angle)
    R_y = np.array([
        [  c, 0, s_val, 0],
        [  0, 1,     0, 0],
        [ -s_val, 0, c, 0],
        [  0, 0,     0, 1]
    ], dtype=float)

    # 3. Translation (Forward by 50 mm)
    # Millimeter units (dz = 50.0 mm)
    T_mm = np.array([
        [1, 0, 0, 0],
        [0, 1, 0, 0],
        [0, 0, 1, 50.0],
        [0, 0, 0, 1]
    ], dtype=float)

    # Meter units (dz = 0.050 m)
    T_m = np.array([
        [1, 0, 0, 0],
        [0, 1, 0, 0],
        [0, 0, 1, 0.05],
        [0, 0, 0, 1]
    ], dtype=float)

    # Composite M = T * R * S (Column-vector convention)
    # Order: Scale first, then Rotate, then Translate
    M_z_mm = T_mm @ R_z @ S
    M_z_m  = T_m  @ R_z @ S
    M_y_mm = T_mm @ R_y @ S

    print("\n--- 1. Order of Transformations & Convention ---")
    print("Convention: Column-vector representation (p' = M * p)")
    print("Order of Operations: Scale (S) -> Rotate (R) -> Translate (T)")
    print("Composite Matrix: M = T * R * S\n")

    print("--- 2. Rotation around Z-axis (Rim Face Plane) ---")
    print("In Millimeters (50 mm):")
    print(np.round(M_z_mm, 4))
    print("\nIn Meters (0.05 m):")
    print(np.round(M_z_m, 4))

    print("\n--- 3. Rotation around Y-axis (Steering / Yaw) ---")
    print("In Millimeters (50 mm):")
    print(np.round(M_y_mm, 4))

    # Test transforming a test point on rim: (100, 0, 0, 1)
    p = np.array([100.0, 0.0, 0.0, 1.0])
    p_prime = M_z_mm @ p
    print(f"\nTransform test point {p.tolist()} -> {np.round(p_prime, 2).tolist()}")

if __name__ == "__main__":
    compute_matrices()


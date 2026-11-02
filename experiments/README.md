# Computer Graphics and Virtual Reality Laboratory Experiments

**Student Name:** Likith S G  
**USN:** 1RVU23CSE236  
**Repository:** Computer-Graphics-and-Virtual-Reality  

This directory contains the independent implementations of all ten laboratory experiments for the Computer Graphics and Virtual Reality (CGVR) laboratory curriculum. Each experiment is implemented in clean, documented Python using OpenGL (PyOpenGL, GLUT, and GLFW).

---

## Laboratory Index

| Sl. No. | Experiment Title | Directory | Source Script | Visual Output |
|:---:|:---|:---:|:---:|:---:|
| **1** | [DDA and Bresenham Line Drawing Algorithms](Experiment_1/) | [`Experiment_1/`](Experiment_1/) | [`exp1.py`](Experiment_1/exp1.py) | [Screenshot](Experiment_1/output.png) |
| **2** | [Midpoint Circle Drawing Algorithm](Experiment_2/) | [`Experiment_2/`](Experiment_2/) | [`exp2.py`](Experiment_2/exp2.py) | [Screenshot](Experiment_2/output.png) |
| **3** | [2D Transformations Using Homogeneous Coordinates](Experiment_3/) | [`Experiment_3/`](Experiment_3/) | [`exp3.py`](Experiment_3/exp3.py) | [Screenshot](Experiment_3/output.png) |
| **4** | [Composite 2D Transformations Using Matrix Representation](Experiment_4/) | [`Experiment_4/`](Experiment_4/) | [`exp4.py`](Experiment_4/exp4.py) | [Screenshot](Experiment_4/output.png) |
| **5** | [Cohen–Sutherland Line Clipping Algorithm](Experiment_5/) | [`Experiment_5/`](Experiment_5/) | [`exp5.py`](Experiment_5/exp5.py) | [Screenshot](Experiment_5/output.png) |
| **6** | [Z-Buffer Algorithm for Hidden Surface Removal](Experiment_6/) | [`Experiment_6/`](Experiment_6/) | [`exp6.py`](Experiment_6/exp6.py) | [Screenshot](Experiment_6/output.png) |
| **7** | [Phong Illumination Model for a 3D Object](Experiment_7/) | [`Experiment_7/`](Experiment_7/) | [`exp7.py`](Experiment_7/exp7.py) | [Screenshot](Experiment_7/output.png) |
| **8** | [OpenGL Vertex and Fragment Shaders](Experiment_8/) | [`Experiment_8/`](Experiment_8/) | [`exp8.py`](Experiment_8/exp8.py) | [Screenshot](Experiment_8/output.png) &bull; [GIF](Experiment_8/anim.gif) |
| **9** | [Simple Interactive 3D Scene](Experiment_9/) | [`Experiment_9/`](Experiment_9/) | [`exp9.py`](Experiment_9/exp9.py) | [Screenshot](Experiment_9/output.png) &bull; [GIF](Experiment_9/demo.gif) |
| **10** | [3D Viewing Pipeline, Projection, and Viewport Mapping](Experiment_10/) | [`Experiment_10/`](Experiment_10/) | [`exp10.py`](Experiment_10/exp10.py) | [Screenshot](Experiment_10/output.png) &bull; [GIF](Experiment_10/demo.gif) |

---

## Execution Guide

All experiments can be executed individually from the repository root:

```bash
# Experiment 1: DDA and Bresenham Line Drawing
python experiments/Experiment_1/exp1.py

# Experiment 2: Midpoint Circle Drawing Algorithm
python experiments/Experiment_2/exp2.py

# Experiment 3: 2D Transformations Using Homogeneous Coordinates
python experiments/Experiment_3/exp3.py

# Experiment 4: Composite 2D Transformations
python experiments/Experiment_4/exp4.py

# Experiment 5: Cohen-Sutherland Line Clipping
python experiments/Experiment_5/exp5.py

# Experiment 6: Z-Buffer Hidden Surface Removal
python experiments/Experiment_6/exp6.py

# Experiment 7: Phong Illumination Model
python experiments/Experiment_7/exp7.py

# Experiment 8: OpenGL Vertex and Fragment Shaders
python experiments/Experiment_8/exp8.py

# Experiment 9: Simple Interactive 3D Scene
python experiments/Experiment_9/exp9.py

# Experiment 10: 3D Viewing Pipeline
python experiments/Experiment_10/exp10.py
```

### Automated Headless Execution & Capture
To re-generate framebuffer screenshots without an interactive window:
```bash
python experiments/Experiment_1/exp1.py --headless
python experiments/Experiment_2/exp2.py --headless
python experiments/Experiment_3/exp3.py --headless
python experiments/Experiment_4/exp4.py --headless
python experiments/Experiment_5/exp5.py --headless
python experiments/Experiment_6/exp6.py --headless
python experiments/Experiment_7/exp7.py --headless
python experiments/Experiment_8/exp8.py --headless
python experiments/Experiment_9/exp9.py --headless
python experiments/Experiment_10/exp10.py --headless
```

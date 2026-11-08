# GPR202 Assessment 2

An interactive 3D scene built with OpenTK and modern OpenGL (3.3 core). A multi-part, textured RV model sits on a desert ground plane under a cube-map skybox. You can drive the RV, steer its front wheels, and fly around it with a first-person camera.

## Running it

Run `GPR202_Assessment_2.exe` from the build folder. The `Engine/Shaders` and `Assets` folders need to stay next to the exe.

To build from source you need the .NET 8 SDK. Run `dotnet run` from the project folder.

## Controls

| Input | What it does |
|---|---|
| Mouse | Look around |
| W / A / S / D | Move forward / left / back / right |
| Q / E | Move down / up |
| Left Shift (hold) | Move faster |
| Up / Down arrows | Drive the RV forward / reverse |
| Left / Right arrows | Steer the front wheels (clamped to ±30°) |
| R | Spin the wheels on the spot |
| Escape | Close the window |

## Features

### Task 1: Textured multi-part model

- The RV is loaded from a Wavefront `.obj` file with its `.mtl` materials by my own loader (`ObjectLoader`). Each object in the file becomes a separate part with its own mesh and material.
- Each vertex has a position, a normal and a UV coordinate. Parts with an image texture are textured; parts with only a material colour (`Kd`), such as the windows, are drawn in that colour.
- Each texture is loaded once and shared by every part that uses it.
- The model is recentred on load so it sits on the ground at the origin.
- The scene is a tree of `ObjectNode`s. Each node has its own position, rotation and scale relative to its parent, so moving the RV's root moves the whole vehicle.
- Lighting uses the Phong model (ambient, diffuse and specular) from a single directional "sun" light.

### Task 2: Movable parts

- The wheels were separated in Blender into four objects. On load, each wheel's vertices are re-centred on its axle so it spins in place instead of swinging around the RV.
- The front wheels steer left and right relative to the body, clamped to ±30°.
- Driving moves and turns the root node, so the body and wheels follow. The wheels spin as the RV drives and turn backwards when reversing.
- All movement is scaled by delta time, so it runs at the same speed at any frame rate.
- The RV's behaviour lives in `RVController`. The loader, nodes and renderer are generic and know nothing about the RV.

### Task 3: First-person camera

- The mouse controls yaw and pitch. Pitch is clamped to ±89° so the camera can't flip over.
- WASD moves relative to where the camera is looking, and Q/E move straight down/up.
- Holding Shift multiplies the move speed.
- The cursor is captured so the mouse can turn the camera freely.

### Task 4: Skybox

- The six desert faces are loaded into a cube-map texture with `ClampToEdge` wrapping, so no seams show where the faces meet.
- The skybox has its own class and its own vertex and fragment shaders.
- Translation is removed from the view matrix, so the sky turns with the camera but never gets closer.
- It is drawn last at the far plane (`gl_Position = pos.xyww` with `DepthFunc(Lequal)`), so it only fills pixels the scene hasn't covered.

## Known issues

- The OBJ loader expects the order Blender exports in (`mtllib` first, then each `o` before its `usemtl` and faces) and triangulated faces with position/UV/normal on every corner. Files in another order won't load.
- The ground is a separate hard-coded plane rather than part of the scene tree.

## Files

- `Program.cs` creates and runs the window.
- `Engine/Window.cs` is the main window. It loads the scene, handles input each frame and draws.
- `Engine/ObjectLoader.cs` reads `.obj` and `.mtl` files into model parts.
- `Engine/ObjectNode.cs` is a node in the scene tree. It builds, draws and cleans up itself and its children.
- `Engine/Mesh.cs` uploads vertex and index data to the GPU (VAO, VBO, EBO) and draws it.
- `Engine/Shader.cs` loads, compiles and links shader programs, and sets uniforms.
- `Engine/Texture.cs` loads an image and uploads it as a 2D texture.
- `Engine/CubemapTexture.cs` loads six images into a cube-map texture.
- `Engine/Skybox.cs` draws the skybox cube with its own shader.
- `Engine/Camera.cs` is the first-person camera and builds the view matrix.
- `Engine/Time.cs` tracks delta time.
- `Engine/Shaders/shader.vert` and `shader.frag` are the main shaders (transforms, texturing and Phong lighting).
- `Engine/Shaders/skybox.vert` and `skybox.frag` are the skybox shaders.
- `Assets/Controllers/RVController.cs` holds the RV's behaviour: wheel origins, steering, spinning and driving.
- `Assets/Ground.cs` holds the ground plane's vertex data.

## Use of AI

I used Claude (Anthropic) to explain the structure of `.obj` and `.mtl` files while planning the loader, and for help fixing the wheel origins. Both are noted in the relevant source files. Claude was used to explain concepts like Phong lighting and scene node heirachy. Claude was used to reformat code comments, point out errors that were missed and reformat references for each file. Claude was used to write the readme which was then read and edited
afterwards.

The desert skybox image was generated with Rosebud AI, then converted into six cube-map faces with HDRI to CubeMap.

## References

References for code techniques and OpenGL functions are listed at the bottom of each source file. Assets and software:

Anthropic. (2026). *Claude* [Large language model]. https://claude.ai

OpenTK. (2025). *OpenTK* (Version 4.9.4) [Computer software]. https://opentk.net/

Rosebud AI. (2026). *Rosebud AI* [AI image generator]. Used to generate the desert skybox image. https://rosebud.ai

Seervi, R. (2023, October 26). *Red sand* [Texture]. Poly Haven. https://polyhaven.com/a/red_sand

StbSharp. (2026). *StbImageSharp* (Version 2.30.16) [Computer software]. https://github.com/StbSharp/StbImageSharp

VectorMan. (2020, July 26). *Breaking Bad 1986 Fleetwood Bounder RV* [3D model]. Sketchfab. https://sketchfab.com/3d-models/breaking-bad-1986-fleetwood-bounder-rv-28006272fe514cb68e3a0ac2125734aa

Wisniowski, M. (2018). *HDRI to CubeMap* [Computer software]. https://matheowis.github.io/HDRI-to-CubeMap/
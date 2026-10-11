# GPR202 Assessment 1

An OpenTK project that draws an animated torus using the modern OpenGL pipeline.

The torus is generated in code (Torus.cs) rather than typed in by hand. Each vertex has a position, a colour and a UV coordinate, and is drawn with a VBO, VAO and EBO. The model scales, bobs up and down, rocks on X and Z, and spins on Y. You can orbit the camera around it with the mouse.

## Running it

Run `GPR202_Assessment_1.exe` from the build folder. The `shaders` and `textures` folders need to stay next to the exe.

To build from source you need the .NET 8 SDK. Run `dotnet run` from the project folder.

## Controls

| Input | What it does |
|---|---|
| Left mouse drag | Orbit the camera |
| Scroll wheel | Zoom in and out |
| W | Wireframe on/off |
| D | Depth test on/off |
| F | Face culling: off, cull front, cull back |
| R | Swap between the normal shader and the ripple shader |
| T | Texture on/off |

Some toggles are worth trying together. Turning off the depth test (D) makes the back of the torus draw over the front. Culling back faces (F twice) fixes most of that, even with the depth test still off. Because a torus isn't convex, though, some overlap is still visible where the far side of the ring shows through the hole, so culling alone can't replace the depth test. Culling front faces shows the inside of the tube.

## Files

- `Program.cs` creates and runs the window.
- `Window.cs` is the main window class. It sets things up, updates the animation and input each frame, and draws.
- `Torus.cs` generates the torus vertices and indices.
- `Mesh.cs` uploads the vertex and index data and draws it.
- `Shader.cs` loads, compiles and links shader programs, and sets uniforms.
- `Camera.cs` is an orbit camera that builds the view matrix.
- `Texture.cs` loads an image and uploads it as an OpenGL texture.
- `Time.cs` tracks total time and delta time.
- `shaders/shader.vert` is the standard vertex shader.
- `shaders/ripple.vert` is the vertex shader with a sine wave applied to each vertex.
- `shaders/shader.frag` is the fragment shader. It blends between the vertex colours and the texture.

## References

<!-- TODO: check the release years on nuget.org -->

OpenTK. (2024). *OpenTK* (Version 4.9.4) [Computer software]. https://opentk.net/

StbImageSharp. (2024). *StbImageSharp* (Version 2.30.16) [Computer software]. https://github.com/StbSharp/StbImageSharp

<!-- TODO: fill in the texture source -->
Author/Site. (Year). *Title of the water texture* [Image]. Site name. URL

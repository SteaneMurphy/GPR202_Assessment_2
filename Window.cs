using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLWindow;

/*
    The main window. Inherits from OpenTK's GameWindow, which runs the loop:
    OnUpdateFrame handles input and animation, OnRenderFrame draws the frame.

    Controls: left mouse drag to orbit, scroll to zoom,
    D depth test, W wireframe, F face culling, R ripple shader, T texture.
*/
public class MainWindow : GameWindow
{
    // file paths
    string vertShaderPath = "shaders/shader.vert";
    string skyboxVertShaderPath = "shaders/skybox.vert";
    string fragShaderPath = "shaders/shader.frag";
    string skyboxFragShaderPath = "shaders/skybox.frag";
    string texturePath = "textures/water.png";
    string objectPath = "models/BreakingBadVan/breaking_bad_1986_fleetwood_bounder_rv.obj";
    string[] skyboxPaths =                                                  // cubemap face order: +X, -X, +Y, -Y, +Z, -Z
    {
        "Skybox/Desert/px.png",
        "Skybox/Desert/nx.png",
        "Skybox/Desert/py.png",
        "Skybox/Desert/ny.png",
        "Skybox/Desert/pz.png",
        "Skybox/Desert/nz.png",
    };

    // classes & objects
    ShaderPipeline _standard = null!;
    ShaderPipeline _current = null!;
    Texture texture = null!;
    List<Mesh> meshes = null!;
    Matrix4 scale;
    Matrix4 translation;
    Matrix4 rotation;
    Matrix4 view;
    Matrix4 projection;
    Time time = new Time();
    Camera camera = null!;

    // skybox
    Skybox skybox = null!;

    // adjustable values - to be exposed to a GUI later
    float scalePulse;
    float scaleBase;
    float scaleSpeed;
    float translationAmt;
    float translationSpeed;
    float rotationSpeed;
    float rotationAngle;
    float amplitudeX;
    float amplitudeSpeedX;
    float amplitudeZ;
    float amplitudeSpeedZ;

    // toggles
    bool depthTest = true;
    bool wireframe = false;
    bool faceCullFront = false;
    bool faceCullBack = false;
    bool textureOn = false;

    /*
        Main window setup. Create new extended GameWindow class.

        Args:

        width, height: dimensions of the window
        windowTitle: name of the window
    */
    public MainWindow(int width, int height, string windowTitle) :
    base(GameWindowSettings.Default, new NativeWindowSettings()
    {
        ClientSize = (width, height),
        Title = windowTitle,
    })
    {
        
    }

    /*
        Runs once when the window opens. Sets the animation values, creates the
        camera, texture, both shader programs and the object mesh.
    */
    protected override void OnLoad()
    {
        base.OnLoad();
        // meshes
        meshes = new List<Mesh>();

        // texture
        texture = new Texture(texturePath);

        // skybox
        skybox = new Skybox(skyboxPaths, skyboxVertShaderPath, skyboxFragShaderPath);

        // view
        camera = new Camera(4f, 34f, 22f, 0.2f, 0.5f, 1.5f, 10f);
        camera.SetTarget(new Vector3(-50f, -30f, -46f));

        // scale
        scaleSpeed = 2f;
        scaleBase = 1f;
        scalePulse = 0.2f;

        // translation
        translationAmt = 1f;
        translationSpeed = 1.5f;

        // rotation
        rotationSpeed = 5f;
        rotationAngle = 10f;
        amplitudeX = 45f;
        amplitudeSpeedX = 1f;
        amplitudeZ = 70f;
        amplitudeSpeedZ = 2f;

        Vector4 screenColour = new Vector4(.1f, .1f, .1f, 1f);
        GL.ClearColor(screenColour.X, screenColour.Y, screenColour.Z, screenColour.W);

        /*
            Builds all shaders and sets the _standard as default
        */
        _standard = new ShaderPipeline(vertShaderPath, fragShaderPath);
        _standard.BuildShaders();
        _current = _standard;

        /*
            Loads and converts the obj file data into material data (vertices, indices, faces, material references).
            Treats each material as a seperate object to render and builds seperate meshes.
        */
        MaterialPart[] materialData = ObjectLoader.ParseObjectData(objectPath);

        foreach (MaterialPart material in materialData)
        {
            Mesh mesh = new Mesh(material.vertices.ToArray(), material.indices.ToArray());
            mesh.BuildMesh();
            meshes.Add(mesh);
        }
    }

    /*
        Draws one frame. Applies the toggled GL states, clears the screen, picks the
        active shader, sends the uniforms, binds the texture and draws the object.
    */
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        if (depthTest) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
        if (wireframe) GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line); else GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        if (faceCullFront)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Front);
        }
        else if (faceCullBack)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);
        }
        else
        {
            GL.Disable(EnableCap.CullFace);
        }

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _current.UseShader();                                            // uniforms below go to whichever program is active

        // scale, then rotate, then translate. Rotation before translation or else the translated movement also gets rotated
        // Matrix4 model = scale * rotation * translation;
        Matrix4 model = Matrix4.Identity;
        // Matrix4 mvp = model * view * projection;

        _current.SetMatrix4("model", model);
        _current.SetMatrix4("view", view);
        _current.SetMatrix4("projection", projection);
        // _current.SetMatrix4("mvp", mvp);

        _current.SetFloat("textureMix", textureOn ? 1f : 0f);
        texture.UseTexture();

        foreach (Mesh mesh in meshes)
        {
            mesh.DrawMesh();
        }

        skybox.DrawSkyBox(view, projection);

        SwapBuffers();
    }

    /*
        Runs every frame before drawing. Reads mouse and keyboard input, updates the
        camera, and builds the scale, rotation and translation matrices from time.
    */
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // input
        if(MouseState.IsButtonDown(MouseButton.Left))
        {
            camera.RotateCamera(MouseState.Delta.X, MouseState.Delta.Y);   
        }

        camera.ZoomCamera(MouseState.ScrollDelta.Y);

        if(KeyboardState.IsKeyPressed(Keys.D))
        {
            depthTest = !depthTest;
        }

        if(KeyboardState.IsKeyPressed(Keys.W))
        {
            wireframe = !wireframe;
        }

        // F cycles: off -> cull front -> cull back -> off
        if(KeyboardState.IsKeyPressed(Keys.F))
        {
            if(!faceCullFront && !faceCullBack)
            {
                faceCullFront = true;
            }
            else if(faceCullFront)
            {
                faceCullFront = false;
                faceCullBack = true;
            }
            else if(faceCullBack)
            {
                faceCullFront = false;
                faceCullBack = false;
            }
        }

        if(KeyboardState.IsKeyPressed(Keys.T))
        {
            textureOn = !textureOn;
        }

        // view
        view = camera.CalculateViewMatrix();

        // // scale calc
        // double scaleCalc = scaleBase + scalePulse * MathHelper.Sin(time.TotalTime() * scaleSpeed);

        // // rotation calcs
        // double angleX = amplitudeX * MathHelper.Sin(time.TotalTime() * amplitudeSpeedX);
        // double angleZ = amplitudeZ * MathHelper.Sin(time.TotalTime() * amplitudeSpeedZ);

        // //translation calc
        // double translationCalc = translationAmt * MathHelper.Sin(time.TotalTime() * translationSpeed);

        // // scale
        // scale = Matrix4.CreateScale((float)scaleCalc);

        // // rotation
        // rotationAngle += (float)(rotationSpeed * time.DeltaTime());

        // rotation = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(angleX)) *
        //            Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(angleZ)) *
        //            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotationAngle));

        // // translation
        // translation = Matrix4.CreateTranslation(0f, (float)translationCalc, 0f);
    }

    /*
        Runs when the window is resized. Updates the viewport and rebuilds the
        projection matrix so the aspect ratio stays correct.
    */
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);

        projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(70f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100f);
    }

    /*
        Runs when the window closes. Frees everything created on the GPU.
    */
    protected override void OnUnload()
    {
        base.OnUnload();

        _standard.DeleteShader();
        texture.Cleanup();
        foreach (Mesh mesh in meshes)
        {
            mesh.Cleanup();
        }

        skybox.Cleanup();
    }
}

/*
    References

    de Vries, J. (n.d.-a). Coordinate systems. LearnOpenGL.
        https://learnopengl.com/Getting-started/Coordinate-Systems

    de Vries, J. (n.d.-b). Depth testing. LearnOpenGL.
        https://learnopengl.com/Advanced-OpenGL/Depth-testing

    de Vries, J. (n.d.-c). Face culling. LearnOpenGL.
        https://learnopengl.com/Advanced-OpenGL/Face-culling

    de Vries, J. (n.d.-d). Transformations. LearnOpenGL.
        https://learnopengl.com/Getting-started/Transformations

    Khronos Group. (n.d.-a). glClear [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glClear.xhtml

    Khronos Group. (n.d.-b). glClearColor [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glClearColor.xhtml

    Khronos Group. (n.d.-c). glCullFace [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glCullFace.xhtml

    Khronos Group. (n.d.-d). glEnable [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glEnable.xhtml

    Khronos Group. (n.d.-e). glPolygonMode [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glPolygonMode.xhtml

    Khronos Group. (n.d.-f). glViewport [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glViewport.xhtml
*/

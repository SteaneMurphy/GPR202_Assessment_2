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
    string objectPath = "models/BreakingBadVan/RV.obj";
    string[] skyboxPaths =                                                  // cubemap face order: +X, -X, +Y, -Y, +Z, -Z
    {
        "Skybox/AnimeDesert/px.png",
        "Skybox/AnimeDesert/nx.png",
        "Skybox/AnimeDesert/py.png",
        "Skybox/AnimeDesert/ny.png",
        "Skybox/AnimeDesert/pz.png",
        "Skybox/AnimeDesert/nz.png",
    };

    // classes & objects
    ShaderPipeline _standard = null!;
    ShaderPipeline _current = null!;
    MaterialPart[] materialData = null!;
    Dictionary<string, Texture> textures;
    List<Mesh> meshes = null!;
    ObjectNode root = null!;
    Matrix4 scale;
    Matrix4 translation;
    Matrix4 rotation;
    Matrix4 view;
    Matrix4 projection;
    Time time = new Time();
    Camera camera = null!;
    RVController rvController = null!;

    // skybox
    Skybox skybox = null!;

    // mouse
    bool firstDelta = true;

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
        // CursorState = CursorState.Grabbed;
        GL.Enable(EnableCap.DepthTest);

        // meshes
        meshes = new List<Mesh>();

        // scene specific data - RV
        rvController = new RVController();

        /*
            Loads and converts the obj file data into material data (vertices, indices, faces, material references).
            Treats each material as a seperate object to render and builds seperate meshes.
        */
        materialData = ObjectLoader.ParseObjectData(objectPath);

        root = new ObjectNode("root");

        foreach (MaterialPart material in materialData)
        {
            ObjectNode node = new ObjectNode(material.name);

            if(material.name.Contains("wheel"))
            {
                rvController.SetWheelReference(node, material);    
            }

            Mesh mesh = new Mesh(material.vertices.ToArray(), material.indices.ToArray());
            mesh.BuildMesh();
            node.SetParams(material.materialPath, material.colour, mesh);
            root.childNodes.Add(node);
        }

        // texture
        textures = new Dictionary<string, Texture>();
        foreach(MaterialPart material in materialData)
        {
            if(material.materialPath != "" && !textures.ContainsKey(material.materialPath))
            {
                textures[material.materialPath] = new Texture(material.materialPath);
            }
        }

        // skybox
        skybox = new Skybox(skyboxPaths, skyboxVertShaderPath, skyboxFragShaderPath);

        // view
        camera = new Camera(0.1f, 3f, new Vector3(-12f, 1.5f, 0f));

        Vector4 screenColour = new Vector4(.1f, .1f, .1f, 1f);
        GL.ClearColor(screenColour.X, screenColour.Y, screenColour.Z, screenColour.W);

        /*
            Builds all shaders and sets the _standard as default
        */
        _standard = new ShaderPipeline(vertShaderPath, fragShaderPath);
        _standard.BuildShaders();
        _current = _standard;
    }

    /*
        Draws one frame. Applies the toggled GL states, clears the screen, picks the
        active shader, sends the uniforms, binds the texture and draws the object.
    */
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _current.UseShader();                                            // uniforms below go to whichever program is active

        // scale, then rotate, then translate. Rotation before translation or else the translated movement also gets rotated
        // Matrix4 model = scale * rotation * translation;
        // Matrix4 model = Matrix4.Identity;
        // Matrix4 mvp = model * view * projection;

        root.RenderNode(textures, _current, Matrix4.Identity);
        // _current.SetMatrix4("model", model);
        _current.SetMatrix4("view", view);
        _current.SetMatrix4("projection", projection);
        // _current.SetMatrix4("mvp", mvp);


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
        // deltaTime
        double deltaTime = time.DeltaTime();

        if(KeyboardState.IsKeyPressed(Keys.Escape))
        {
            Close();
        }

        // WASD movement
        float forwardInput = 0f, rightInput = 0f, upInput = 0f;
        if (KeyboardState.IsKeyDown(Keys.W)) forwardInput += 1f;
        if (KeyboardState.IsKeyDown(Keys.S)) forwardInput -= 1f;
        if (KeyboardState.IsKeyDown(Keys.D)) rightInput += 1f;
        if (KeyboardState.IsKeyDown(Keys.A)) rightInput -= 1f;
        if (KeyboardState.IsKeyDown(Keys.E)) upInput += 1f;
        if (KeyboardState.IsKeyDown(Keys.Q)) upInput -= 1f;

        if (firstDelta)
        {
            firstDelta = false;                                                     // skip first mouse read
        }
        else
        {
            camera.UpdateDirection(MouseState.Delta.X, MouseState.Delta.Y);   
        }

        if (KeyboardState.IsKeyDown(Keys.R)) rvController.RotateWheels((float)deltaTime);
        if (KeyboardState.IsKeyDown(Keys.Left)) rvController.SteerWheels(Keys.Left, (float)deltaTime);
        if (KeyboardState.IsKeyDown(Keys.Right)) rvController.SteerWheels(Keys.Right, (float)deltaTime);

        camera.UpdateVectors(forwardInput, rightInput, upInput, deltaTime);
        camera.UpdateMoveSpeed(KeyboardState.IsKeyDown(Keys.LeftShift));

        // view
        view = camera.UpdateViewMatrix();

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

        foreach (Texture texture in textures.Values)
        {
            texture.Cleanup();
        }

        root.Cleanup();

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

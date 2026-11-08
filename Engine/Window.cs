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
    string vertShaderPath = "Engine/Shaders/shader.vert";
    string skyboxVertShaderPath = "Engine/Shaders/skybox.vert";
    string fragShaderPath = "Engine/Shaders/shader.frag";
    string skyboxFragShaderPath = "Engine/Shaders/skybox.frag";
    string texturePath = "textures/water.png";
    string objectPath = "Assets/Models/BreakingBadVan/RV.obj";
    string groundTexturePath = "Assets/Textures/sand.jpg";
    string[] skyboxPaths =                                                  // cubemap face order: +X, -X, +Y, -Y, +Z, -Z
    {
        "Assets/Skybox/AnimeDesert/px.png",
        "Assets/Skybox/AnimeDesert/nx.png",
        "Assets/Skybox/AnimeDesert/py.png",
        "Assets/Skybox/AnimeDesert/ny.png",
        "Assets/Skybox/AnimeDesert/pz.png",
        "Assets/Skybox/AnimeDesert/nz.png",
    };

    // classes & objects
    ShaderPipeline _standard = null!;
    ShaderPipeline _current = null!;
    MaterialPart[] materialData = null!;
    Dictionary<string, Texture> textures;
    List<Mesh> meshes = null!;
    ObjectNode root = null!;
    ObjectNode sceneRoot = null!;
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

    // ground
    Mesh groundMesh = null!;
    Texture groundTexture = null!;

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

        // scene specific data - controllers
        rvController = new RVController();

        /*
            Loads and converts the obj file data into material data (vertices, indices, faces, material references).
            Treats each material as a seperate object to render and builds seperate meshes.
        */
        sceneRoot = new ObjectNode("sceneRoot");

        foreach (string objectPath in Directory.GetFiles("Assets/Models", "*.obj", SearchOption.AllDirectories))
        {
            materialData = ObjectLoader.ParseObjectData(objectPath);

            root = new ObjectNode("root");
            root.AddParts(materialData);            // build the node tree

            sceneRoot.childNodes.Add(root);         // add object tree to the scene root

            rvController.Setup(root);               // RV-specific fixes to the wheel nodes' vertex data

            root.BuildMeshes();                     // Build meshes on GPU after individual model vertex changes (RV above)   
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

        // ground
        groundMesh = new Mesh(Ground.vertices, Ground.indices);
        groundMesh.BuildMesh();
        groundTexture = new Texture(groundTexturePath);

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

        _current.SetMatrix4("view", view);
        _current.SetMatrix4("projection", projection);
        sceneRoot.RenderNode(textures, _current, Matrix4.Identity);

        groundTexture.UseTexture();
        _current.SetBool("useTexture", true);
        _current.SetVector3("colour", Vector3.One);
        _current.SetMatrix4("model", Matrix4.Identity);
        groundMesh.DrawMesh();

        _current.SetVector3("lightDir", new Vector3(-0.3f, -1.0f, -0.5f));
        _current.SetVector3("viewPos", camera.GetPosition());

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

        camera.UpdateVectors(forwardInput, rightInput, upInput, deltaTime);
        camera.UpdateMoveSpeed(KeyboardState.IsKeyDown(Keys.LeftShift));

        // RV controller
        rvController.UpdateInput(KeyboardState, (float)deltaTime);

        // view
        view = camera.UpdateViewMatrix();
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

        - shaders
        - all meshes (root currently needs to be set to scene node)
        - ground mesh (seperate atm)
        - textures
        - skybox
    */
    protected override void OnUnload()
    {
        base.OnUnload();

        _standard.DeleteShader();

        foreach (Texture texture in textures.Values)
        {
            texture.Cleanup();
        }

        sceneRoot.Cleanup();

        groundMesh.Cleanup();
        groundTexture.Cleanup();

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

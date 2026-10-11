using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLWindow;

public class MainWindow : GameWindow
{
    /*
        Main window setup. Create new extended GameWindow class.

        Args:

        width, height: dimensions of the window
        windowTitle: name of the window
    */
    string vertShaderPath = "shaders/shader.vert";
    string rippleShaderPath = "shaders/ripple.vert";
    string fragShaderPath = "shaders/shader.frag";
    string texturePath = "textures/water.png";
    ShaderPipeline _standard = null!;
    ShaderPipeline _ripple = null!;
    ShaderPipeline _current = null!;
    Texture texture = null!;
    Mesh mesh = null!;
    Matrix4 scale;
    Matrix4 translation;
    Matrix4 rotation;
    Matrix4 view;
    Matrix4 projection;
    Time time = new Time();
    Camera camera = null!;

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
    bool rippleShader = false;
    bool textureOn = false;

    public MainWindow(int width, int height, string windowTitle) :
    base(GameWindowSettings.Default, new NativeWindowSettings()
    {
        ClientSize = (width, height),
        Title = windowTitle,
    })
    {
        
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        // texture
        texture = new Texture(texturePath);

        // view
        camera = new Camera(4f, 34f, 22f, 0.2f, 0.5f, 1.5f, 10f);

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

        _standard = new ShaderPipeline(vertShaderPath, fragShaderPath);
        _standard.BuildShaders();
        _ripple = new ShaderPipeline(rippleShaderPath, fragShaderPath);
        _ripple.BuildShaders();
        _current = _standard;

        Torus torus = new Torus(100, 50, 0.7f, 0.3f);
        float[] vertexData = torus.CreateTorus();
        uint[] indexData = torus.CreateIndex();

        mesh = new Mesh(vertexData, indexData);
        mesh.BuildMesh();

        // torus indices wind clockwise when viewed from outside
        GL.FrontFace(FrontFaceDirection.Cw);
    }

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

        _current.UseShader();

        Matrix4 model = scale * rotation * translation;
        Matrix4 mvp = model * view * projection;

        if (rippleShader)
        {
            _current.SetFloat("time", (float)time.TotalTime());
        }
        _current.SetMatrix4("mvp", mvp);

        _current.SetFloat("textureMix", textureOn ? 1f : 0f);
        texture.UseTexture();

        mesh.DrawMesh();

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        /*
            Rotation before translation or else the translated movement also gets rotated
        */


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

        if(KeyboardState.IsKeyPressed(Keys.R))
        {
            rippleShader = !rippleShader;
            _current = rippleShader ? _ripple : _standard;
        }

        if(KeyboardState.IsKeyPressed(Keys.T))
        {
            textureOn = !textureOn;
        }

        // view
        view = camera.CalculateViewMatrix();

        // scale calcs
        double scaleCalc = scaleBase + scalePulse * MathHelper.Sin(time.TotalTime() * scaleSpeed);

        // rotation calcs
        double angleX = amplitudeX * MathHelper.Sin(time.TotalTime() * amplitudeSpeedX);
        double angleZ = amplitudeZ * MathHelper.Sin(time.TotalTime() * amplitudeSpeedZ);

        //translation calc
        double translationCalc = translationAmt * MathHelper.Sin(time.TotalTime() * translationSpeed);

        // scale
        scale = Matrix4.CreateScale((float)scaleCalc);

        // rotation
        rotationAngle += (float)(rotationSpeed * time.DeltaTime());

        rotation = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(angleX)) *
                   Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(angleZ)) *
                   Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotationAngle));

        // translation
        translation = Matrix4.CreateTranslation(0f, (float)translationCalc, 0f);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);

        projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(70f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100f);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        _standard.DeleteShader();
        _ripple.DeleteShader();
        texture.Cleanup();
        mesh.Cleanup();
    }
}
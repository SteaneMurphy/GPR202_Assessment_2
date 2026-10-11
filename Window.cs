using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

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
    string fragShaderPath = "shaders/shader.frag";
    ShaderPipeline _shader = null!;
    Mesh mesh = null!;
    Matrix4 scale;
    Matrix4 translation;
    Matrix4 rotation;
    Matrix4 view;
    Matrix4 projection;
    Time time = new Time();

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

        base.OnLoad();

        Vector4 screenColour = new Vector4(.1f, .1f, .1f, 1f);
        GL.ClearColor(screenColour.X, screenColour.Y, screenColour.Z, screenColour.W);

        _shader = new ShaderPipeline(vertShaderPath, fragShaderPath);
        _shader.BuildShaders();

        GL.Enable(EnableCap.DepthTest);

        Torus torus = new Torus(100, 50, 0.7f, 0.3f);
        float[] vertexData = torus.CreateTorus();
        uint[] indexData = torus.CreateIndex();

        mesh = new Mesh(vertexData, indexData);
        mesh.BuildMesh();

        view = Matrix4.LookAt(new Vector3(2f, 1.5f, 3f), Vector3.Zero, Vector3.UnitY);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.UseShader();

        Matrix4 model = scale * rotation * translation;
        Matrix4 mvp = model * view * projection;

        _shader.SetMatrix4("mvp", mvp);

        mesh.DrawMesh();

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        /*
            Rotation before translation or else the translated movement also gets rotated
        */
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

        _shader.DeleteShader();
        mesh.Cleanup();
    }
}
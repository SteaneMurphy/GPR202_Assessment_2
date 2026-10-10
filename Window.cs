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
    ShaderPipeline _shader;
    Mesh mesh;
    Matrix4 model;
    Matrix4 view;
    Matrix4 projection;

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

        Vector4 screenColour = new Vector4(.1f, .1f, .1f, 1f);
        GL.ClearColor(screenColour.X, screenColour.Y, screenColour.Z, screenColour.W);

        _shader = new ShaderPipeline(vertShaderPath, fragShaderPath);
        _shader.BuildShaders();

        GL.Enable(EnableCap.DepthTest);

        mesh = new Mesh(Geometry.cube);
        mesh.BuildMesh();

        view = Matrix4.LookAt(new Vector3(2f, 1.5f, 3f), Vector3.Zero, Vector3.UnitY);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.UseShader();

        model = Matrix4.Identity;
        Matrix4 mvp = model * view * projection;
        _shader.SetMatrix4("mvp", mvp);

        mesh.DrawMesh();

        SwapBuffers();
    }

    // protected override void OnUpdateFrame(FrameEventArgs args)
    // {
    //     base.OnUpdateFrame(args);
    // }

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
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

        mesh = new Mesh(Geometry.cube);
        mesh.BuildMesh();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        _shader.UseShader();

        mesh.DrawMesh();

        SwapBuffers();
    }

    // protected override void OnUpdateFrame(FrameEventArgs args)
    // {
    //     base.OnUpdateFrame(args);
    // }

    // protected override void OnResize(ResizeEventArgs e)
    // {
    //     base.OnResize(e);
    // }

    protected override void OnUnload()
    {
        base.OnUnload();

        _shader.DeleteShader();
        mesh.Cleanup();
    }
}
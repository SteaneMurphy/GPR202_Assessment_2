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
    }

    // protected override void OnRenderFrame(FrameEventArgs args)
    // {
    //     base.OnRenderFrame(args);
    // }

    // protected override void OnUpdateFrame(FrameEventArgs args)
    // {
    //     base.OnUpdateFrame(args);
    // }

    // protected override void OnResize(ResizeEventArgs e)
    // {
    //     base.OnResize(e);
    // }

    // protected override void OnUnload()
    // {
    //     base.OnUnload();
    // }
}
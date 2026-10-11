using OpenGLWindow;

/*
    Main game window. Sets size and window title.

    Args:
        - width (px)
        - height (px)
        - title (string)
*/
using (MainWindow window = new MainWindow(1100, 720, "GPR202 Assessment 1"))
{
    window.Run();
}
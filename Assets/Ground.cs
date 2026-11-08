public class Ground
{
    /*
        A temporary ground plane for this assessment. Generated seperately from model .obj files passed into the loader.
        Size of plane adjustable with s, tiling amount adjustable with t.
    */
    const float s = 100f;                   // half size of the plane
    const float t = 40f;                    // how many times the texture tiles across it

    public static float[] vertices =
    {
    //   x    y    z     nx  ny  nz    u   v
        -s,  0f, -s,     0f, 1f, 0f,   0f, 0f,     // 0
        -s,  0f,  s,     0f, 1f, 0f,   0f, t,      // 1
         s,  0f,  s,     0f, 1f, 0f,   t,  t,      // 2
         s,  0f, -s,     0f, 1f, 0f,   t,  0f,     // 3
    };

    public static uint[] indices =
    {
        0, 1, 2,  2, 3, 0,                      
    };
}

/*
    References (Plane vertices and build from this code snippet)

    de Vries, J. (n.d.). advanced_lighting.cpp [Source code]. LearnOpenGL. GitHub.
        https://github.com/JoeyDeVries/LearnOpenGL/blob/master/src/5.advanced_lighting/1.advanced_lighting/advanced_lighting.cpp
*/

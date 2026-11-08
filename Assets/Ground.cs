public class Ground
{
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
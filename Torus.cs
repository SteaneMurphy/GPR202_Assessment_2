/*
    This class generates the vertex and index data for a torus.
    The shape is calculated from two radii and two segment counts rather than
    typed in by hand, so its size and smoothness can be changed with parameters.
*/
public class Torus
{
    private int ringSegments;
    private int tubeSegments;
    private float majorRadius;
    private float minorRadius;

    /*
        ringSegments: how many points around the big ring
        tubeSegments: how many points around the tube
        majorRadius:  distance from the centre of the hole to the middle of the tube
        minorRadius:  thickness (radius) of the tube
    */
    public Torus(int _ringSegments, int _tubeSegments, float _majorRadius, float _minorRadius)
    {
        ringSegments = _ringSegments;
        tubeSegments = _tubeSegments;
        majorRadius = _majorRadius;
        minorRadius = _minorRadius;
    }

    /*
        Builds the vertex array using two nested loops. The outer loop goes around
        the ring (majorAngle) and the inner loop goes around the tube (minorAngle).
        Each vertex is 8 floats: position, colour and UV.

        Position uses the parametric torus equation:
        x = (R + r cos(minor)) cos(major)
        y = r sin(minor)
        z = (R + r cos(minor)) sin(major)
    */
    public float[] CreateTorus()
    {
        List<float> vertices = new List<float>();

        for(int i = 0; i < ringSegments; i++)
        {
            for(int j = 0; j < tubeSegments; j++)
            {
                float majorAngle = (float)i / ringSegments * (2 * MathF.PI);
                float minorAngle = (float)j / tubeSegments * (2 * MathF.PI);

                // position
                vertices.Add((majorRadius + (minorRadius * MathF.Cos(minorAngle))) * MathF.Cos(majorAngle));
                vertices.Add(minorRadius * MathF.Sin(minorAngle));
                vertices.Add((majorRadius + (minorRadius * MathF.Cos(minorAngle))) * MathF.Sin(majorAngle));

                // colour: the direction the surface faces, mapped from -1..1 to 0..1
                vertices.Add(MathF.Cos(minorAngle) * MathF.Cos(majorAngle) * 0.5f + 0.5f);
                vertices.Add(MathF.Sin(minorAngle) * 0.5f + 0.5f);
                vertices.Add(MathF.Cos(minorAngle) * MathF.Sin(majorAngle) * 0.5f + 0.5f);

                // UV: how far around the ring (u) and around the tube (v), 0..1
                vertices.Add((float)i / ringSegments);
                vertices.Add((float)j / tubeSegments);
            }
        }

        return vertices.ToArray();
    }

    /*
        Builds the index array. The vertices form a grid that wraps around in both
        directions, so each grid square (a, b, c, d) is split into two triangles.
        Vertex (i, j) sits at position i * tubeSegments + j in the vertex array.
        % wraps the last row and column back to 0 so the torus closes up.
        Corners are added anticlockwise as seen from outside, which is what OpenGL
        treats as the front face by default, so face culling works correctly.
    */
    public uint[] CreateIndex()
    {
        List<uint> indices = new List<uint>();

        for(int i = 0; i < ringSegments; i++)
        {
            for(int j = 0; j < tubeSegments; j++)
            {
                int nextI = (i + 1) % ringSegments;
                int nextJ = (j + 1) % tubeSegments;

                uint a = (uint)(i * tubeSegments + j);
                uint b = (uint)(nextI * tubeSegments + j);
                uint c = (uint)(nextI * tubeSegments + nextJ);
                uint d = (uint)(i * tubeSegments + nextJ);

                indices.Add(a);
                indices.Add(c);
                indices.Add(b);

                indices.Add(a);
                indices.Add(d);
                indices.Add(c);
            }
        }

        return indices.ToArray();
    }
}

/*
    References

    Ahn, S. H. (n.d.). Torus, torus knot & trefoil knot. Song Ho Ahn.
        http://www.songho.ca/opengl/gl_torus.html

    Weisstein, E. W. (n.d.). Torus. MathWorld--A Wolfram Web Resource.
        https://mathworld.wolfram.com/Torus.html
*/

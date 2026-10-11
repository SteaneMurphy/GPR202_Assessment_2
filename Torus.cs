public class Torus
{
    private int ringSegments;
    private int tubeSegments;
    private float majorRadius;
    private float minorRadius;

    public Torus(int _ringSegments, int _tubeSegments, float _majorRadius, float _minorRadius)
    {
        ringSegments = _ringSegments;
        tubeSegments = _tubeSegments;
        majorRadius = _majorRadius;
        minorRadius = _minorRadius;
    }

    public float[] CreateTorus()
    {
        List<float> vertices = new List<float>();

        for(int i = 0; i < ringSegments; i++)
        {
            for(int j = 0; j < tubeSegments; j++)
            {
                float majorAngle = (float)i / ringSegments * (2 * MathF.PI);
                float minorAngle = (float)j / tubeSegments * (2 * MathF.PI);

                vertices.Add((majorRadius + (minorRadius * MathF.Cos(minorAngle))) * MathF.Cos(majorAngle));
                vertices.Add(minorRadius * MathF.Sin(minorAngle));
                vertices.Add((majorRadius + (minorRadius * MathF.Cos(minorAngle))) * MathF.Sin(majorAngle));

                vertices.Add(MathF.Cos(minorAngle) * MathF.Cos(majorAngle) * 0.5f + 0.5f);
                vertices.Add(MathF.Sin(minorAngle) * 0.5f + 0.5f);
                vertices.Add(MathF.Cos(minorAngle) * MathF.Sin(majorAngle) * 0.5f + 0.5f);
            }
        }

        return vertices.ToArray();
    }

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
                indices.Add(b);
                indices.Add(c);

                indices.Add(a);
                indices.Add(c);
                indices.Add(d);
            }
        }

        return indices.ToArray();
    }
}
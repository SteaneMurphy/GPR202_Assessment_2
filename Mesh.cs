using OpenTK.Graphics.OpenGL4;

public class Mesh
{
    private float[] vertexData;
    private int _vao;
    private int _vbo;
    int stride;

    public Mesh(float[] _vertexData)
    {
        vertexData = _vertexData;
        stride = 3 * sizeof(float);
    }

    public void BuildMesh()
    {
        InitialiseVertices();
    }

    private void InitialiseVertices()
    {
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);

        // GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        // GL.EnableVertexAttribArray(1);

        GL.BindVertexArray(0);
    }

    public void DrawMesh()
    {
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, vertexData.Length / 3);
        GL.BindVertexArray(0);
    }

    public void Cleanup()
    {
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteBuffer(_vao);
        GL.DeleteBuffer(_vbo);
    }
}
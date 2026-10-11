using OpenTK.Graphics.OpenGL4;

public class Mesh
{
    private float[] vertexData;
    private uint[] indexArray;
    private int _vao;
    private int _vbo;
    private int _ebo;
    int stride;

    public Mesh(float[] _vertexData, uint[] _indexArray)
    {
        vertexData = _vertexData;
        indexArray = _indexArray;
        stride = 6 * sizeof(float);
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

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indexArray.Length * sizeof(uint), indexArray, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);
    }

    public void DrawMesh()
    {
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, indexArray.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    public void Cleanup()
    {
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
    }
}
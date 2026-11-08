using OpenTK.Graphics.OpenGL4;

/*
    This class takes vertex and index data, uploads it to the GPU and draws it.
    Each vertex is 8 floats: position (3), colour (3) and UV (2).
*/
public class Mesh
{
    private float[] vertexData;
    private uint[] indexArray;
    private int _vao;                       // vertex buffer object
    private int _vbo;                       // vertex array object
    private int _ebo;                       // element buffer object
    int stride;

    /*
        Stores the vertex and index arrays. The stride is the size of one whole
        vertex in bytes, so OpenGL knows how far to step to the next one.
    */
    public Mesh(float[] _vertexData, uint[] _indexArray)
    {
        vertexData = _vertexData;
        indexArray = _indexArray;
        stride = 8 * sizeof(float);
    }

    /*
        Sets up the GPU buffers for this mesh. Called once in OnLoad.
    */
    public void BuildMesh()
    {
        InitialiseVertices();
    }

    /*
        Creates the VAO, VBO and EBO and uploads the data.
        The VAO records the attribute layout and which EBO is bound, so drawing
        later only needs the VAO bound. The EBO is bound while the VAO is still bound,
        otherwise the VAO wouldn't remember it.
    */
    private void InitialiseVertices()
    {
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);                      // position: 3 floats from the start
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));      // normal: 3 floats after the position
        GL.EnableVertexAttribArray(1);

        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));      // UV: 2 floats after the colour
        GL.EnableVertexAttribArray(2);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indexArray.Length * sizeof(uint), indexArray, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);
    }

    /*
        Binds the VAO and draws the triangles using the index buffer.
    */
    public void DrawMesh()
    {
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, indexArray.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    /*
        Unbinds and deletes the VAO, VBO and EBO from the GPU. Called when the window closes.
    */
    public void Cleanup()
    {
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
    }
}

/*
    References

    Khronos Group. (n.d.-a). glBindBuffer [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBindBuffer.xhtml

    Khronos Group. (n.d.-b). glBindVertexArray [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBindVertexArray.xhtml

    Khronos Group. (n.d.-c). glBufferData [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glBufferData.xhtml

    Khronos Group. (n.d.-d). glDeleteBuffers [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glDeleteBuffers.xhtml

    Khronos Group. (n.d.-e). glDeleteVertexArrays [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glDeleteVertexArrays.xhtml

    Khronos Group. (n.d.-f). glDrawElements [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glDrawElements.xhtml

    Khronos Group. (n.d.-g). glEnableVertexAttribArray [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glEnableVertexAttribArray.xhtml

    Khronos Group. (n.d.-h). glGenBuffers [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGenBuffers.xhtml

    Khronos Group. (n.d.-i). glGenVertexArrays [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGenVertexArrays.xhtml

    Khronos Group. (n.d.-j). glVertexAttribPointer [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glVertexAttribPointer.xhtml

    OpenTK. (n.d.-a). Element buffer objects. OpenTK.
        https://opentk.net/learn/chapter1/3-element-buffer-objects.html

    OpenTK. (n.d.-b). Hello triangle. OpenTK.
        https://opentk.net/learn/chapter1/2-hello-triangle.html
*/

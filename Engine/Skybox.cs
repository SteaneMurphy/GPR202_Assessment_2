using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

/*
    Draws a cube around the camera with the cubemap on its inside, so the sky
    surrounds the scene. It has its own shader, and its own buffers because it
    only needs positions: each vertex's position doubles as the direction to
    sample the cubemap in.
*/
public class Skybox
{
    CubemapTexture cubemap;
    ShaderPipeline shader;
    int _vao;
    int _vbo;
    int _ebo;

    float[] vertices =
    {
        -1f, -1f, -1f,      // 0
         1f, -1f, -1f,      // 1
         1f,  1f, -1f,      // 2
        -1f,  1f, -1f,      // 3
        -1f, -1f,  1f,      // 4
         1f, -1f,  1f,      // 5
         1f,  1f,  1f,      // 6
        -1f,  1f,  1f,      // 7
    };

    uint[] indices =
    {
        0, 1, 2,  2, 3, 0,  // back
        4, 6, 5,  6, 4, 7,  // front
        0, 3, 7,  7, 4, 0,  // left
        1, 5, 6,  6, 2, 1,  // right
        0, 4, 5,  5, 1, 0,  // bottom
        3, 2, 6,  6, 7, 3,  // top
    };

    /*
        Loads the 6 cubemap faces, builds the skybox shader, and uploads the
        cube's 8 corners and 36 indices to the GPU.
    */
    public Skybox(string[] paths, string vertShaderPath, string fragShaderPath)
    {
        cubemap = new CubemapTexture(paths);

        shader = new ShaderPipeline(vertShaderPath, fragShaderPath);
        shader.BuildShaders();

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);                           // position
        GL.EnableVertexAttribArray(0);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);
    }

    /*
        Drawn last, after the scene. The vertex shader sets the depth to 1.0 (the far plane)
        with pos.xyww, and the depth test is changed to Lequal so the sky only shows where
        nothing else has been drawn. The view matrix has its translation removed so the
        sky stays centred on the camera and never gets closer.
    */
    public void DrawSkyBox(Matrix4 view, Matrix4 projection)
    {
        GL.DepthFunc(DepthFunction.Lequal);
        GL.Disable(EnableCap.CullFace);                                                    // seen from inside, so don't cull

        shader.UseShader();
        shader.SetMatrix4("view", new Matrix4(new Matrix3(view)));                         // remove translation, sky never moves
        shader.SetMatrix4("projection", projection);
        cubemap.UseTextures();

        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);

        GL.DepthFunc(DepthFunction.Less);                                                  // back to default
    }

    // Frees the cube's buffers, the skybox shader and the cubemap. Called when the window closes.
    public void Cleanup()
    {
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        shader.DeleteShader();
        cubemap.Cleanup();
    }
}

/*
    References

    de Vries, J. (n.d.). Cubemaps. LearnOpenGL.
        https://learnopengl.com/Advanced-OpenGL/Cubemaps

    Khronos Group. (n.d.). glDepthFunc [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glDepthFunc.xhtml

    Rosebud AI. (2026). Rosebud AI [AI image generator]. Used to generate the desert skybox image.
        https://rosebud.ai

    Wisniowski, M. (2018). HDRI to CubeMap [Computer software].
        https://matheowis.github.io/HDRI-to-CubeMap/
*/
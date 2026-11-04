using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

/*
    This class prepares the shader scripts into one shader instance to be used by the
    OpenGL program.

    It combines one vertex shader and one fragment shader, passed in from the main window.
    It compiles the shaders and creates and links the shader program.
*/
public class ShaderPipeline
{
    Dictionary<string, ShaderType> shaderSources;                                                   // shader file paths and types
    int shaderInstance;                                                                             // shader instance of linked shader program

    /*
        Stores the vertex and fragment shader file paths. Nothing is compiled
        until BuildShaders is called.
    */
    public ShaderPipeline(string _vertShaderPath, string _fragShaderPath)
    {
        shaderSources = new Dictionary<string, ShaderType>()                                        // assigns the shader file paths and type
        {
          {_vertShaderPath, ShaderType.VertexShader},
          {_fragShaderPath, ShaderType.FragmentShader},  
        };
    }

    /*
        Loops through each shader file, compiles it, then links them all into one
        shader program. Using a loop means more shader stages could be added later
        without changing this function.
    */
    public void BuildShaders()
    {
        int[] compiledShaders = new int[shaderSources.Count];
        int shaderCount = 0;

        foreach(KeyValuePair<string, ShaderType> shaderPath in shaderSources)
        {
            compiledShaders[shaderCount] = CompileShader(shaderPath.Key, shaderPath.Value);
            shaderCount++;
        }

        LinkShaders(compiledShaders);
    }

    /*
        Reads a shader file, creates a shader of the given type and compiles it.
        Prints the compile log if it fails. Returns the shader's handle.
    */
    private int CompileShader(string shaderPath, ShaderType shaderType)
    {
        int shader = GL.CreateShader(shaderType);

        string source = "";
        if(File.Exists(shaderPath))
        {
            source = File.ReadAllText(shaderPath);
        }
        else
        {
            Console.WriteLine($"Error: Shader file not found: {shaderPath}");                      // compile and link will also fail below
        }

        GL.ShaderSource(shader, source);

        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if(success == 0)
        {
            string log = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"Error: Compile shader: {shaderPath}, {log}");
        }

        return shader;
    }

    /*
        Creates the shader program, attaches the compiled shaders and links them.
        Prints the link log if linking fails.
    */
    private void LinkShaders(int[] compiledShaders)
    {
        shaderInstance = GL.CreateProgram();

        foreach(int shader in compiledShaders)
        {
            GL.AttachShader(shaderInstance, shader);
        }

        GL.LinkProgram(shaderInstance);
        GL.GetProgram(shaderInstance, GetProgramParameterName.LinkStatus, out int success);
        if(success == 0)
        {
            string log = GL.GetProgramInfoLog(shaderInstance);
            Console.WriteLine($"Error: New program linking failed, {log}");
        }

        Cleanup(shaderInstance, compiledShaders);
    }

    /*
        Detaches and deletes the individual shaders once they're linked.
        The program keeps its own copy, so they're no longer needed.
    */
    private void Cleanup(int newInstance, int[] compiledShaders)
    {
        foreach(int shader in compiledShaders)
        {
            GL.DetachShader(newInstance, shader);
            GL.DeleteShader(shader);
        }
    }

    /*
        Sends a float uniform (e.g. time) to the shader program.
        UseShader must be called first.
    */
    public void SetFloat(string name, float value)
    {
        int location = GL.GetUniformLocation(shaderInstance, name);
        GL.Uniform1(location, value);
        
    }

    public void SetVector3(string name, Vector3 vector)
    {
        int location = GL.GetUniformLocation(shaderInstance, name);
        GL.Uniform3(location, vector);
    }

    public void SetBool(string name, bool value)
    {
        int location = GL.GetUniformLocation(shaderInstance, name);
        GL.Uniform1(location, value ? 1 : 0);
    }

    /*
        Sends a 4x4 matrix uniform (e.g. mvp) to the shader program.
    */
    public void SetMatrix4(string name, Matrix4 matrix)
    {
        int location = GL.GetUniformLocation(shaderInstance, name);
        GL.UniformMatrix4(location, true, ref matrix);
    }

    /*
        Makes this the active shader program for the next draw call.
    */
    public void UseShader()
    {
        GL.UseProgram(shaderInstance);
    }

    /*
        Deletes the shader program from the GPU. Called when the window closes.
    */
    public void DeleteShader()
    {
        GL.DeleteProgram(shaderInstance);
    }
}
/*
    References

    Khronos Group. (n.d.-a). glAttachShader [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glAttachShader.xhtml

    Khronos Group. (n.d.-b). glCompileShader [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glCompileShader.xhtml

    Khronos Group. (n.d.-c). glCreateProgram [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glCreateProgram.xhtml

    Khronos Group. (n.d.-d). glCreateShader [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glCreateShader.xhtml

    Khronos Group. (n.d.-e). glDeleteProgram [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glDeleteProgram.xhtml

    Khronos Group. (n.d.-f). glDeleteShader [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glDeleteShader.xhtml

    Khronos Group. (n.d.-g). glDetachShader [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glDetachShader.xhtml

    Khronos Group. (n.d.-h). glGetProgram [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGetProgram.xhtml

    Khronos Group. (n.d.-i). glGetProgramInfoLog [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGetProgramInfoLog.xhtml

    Khronos Group. (n.d.-j). glGetShader [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGetShader.xhtml

    Khronos Group. (n.d.-k). glGetShaderInfoLog [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGetShaderInfoLog.xhtml

    Khronos Group. (n.d.-l). glGetUniformLocation [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGetUniformLocation.xhtml

    Khronos Group. (n.d.-m). glLinkProgram [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glLinkProgram.xhtml

    Khronos Group. (n.d.-n). glShaderSource [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glShaderSource.xhtml

    Khronos Group. (n.d.-o). glUniform [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glUniform.xhtml

    Khronos Group. (n.d.-p). glUseProgram [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/glUseProgram.xhtml
*/

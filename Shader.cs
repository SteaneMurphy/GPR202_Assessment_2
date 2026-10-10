using OpenTK.Graphics.OpenGL4;

public class ShaderPipeline
{
    Dictionary<string, ShaderType> shaderSources;
    int shaderInstance;

    public ShaderPipeline(string _vertShaderPath, string _fragShaderPath)
    {
        shaderSources = new Dictionary<string, ShaderType>()
        {
          {_vertShaderPath, ShaderType.VertexShader},
          {_fragShaderPath, ShaderType.FragmentShader},  
        };
    }

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

    private int CompileShader(string shaderPath, ShaderType shaderType)
    {
        int shader = GL.CreateShader(shaderType);

        GL.ShaderSource(shader, File.ReadAllText(shaderPath));

        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if(success == 0)
        {
            string log = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"Error: Compile shader: {shaderPath}, {log}");
        }

        return shader;
    }

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

    private void Cleanup(int newInstance, int[] compiledShaders)
    {
        foreach(int shader in compiledShaders)
        {
            GL.DetachShader(newInstance, shader);
            GL.DeleteShader(shader);
        }
    }

    public void UseShader()
    {
        GL.UseProgram(shaderInstance);
    }

    public void DeleteShader()
    {
        GL.DeleteProgram(shaderInstance);
    }
}
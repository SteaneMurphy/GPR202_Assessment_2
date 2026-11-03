using OpenTK.Graphics.OpenGL4;
using StbImageSharp;


public class CubemapTexture
{
    private string[] paths;
    int textureInstance;

    public CubemapTexture(string[] _paths)
    {
        paths = _paths;
        CreateCubemap();
        LoadTextures();
    }

    private void LoadTextures()
    {
        for(int i = 0; i < paths.Length; i++)
        {
            ImageResult image;
            if(!File.Exists(paths[i]))
            {
                Console.WriteLine($"Error: Texture file not found: {paths[i]}");
                return;
            }
            try
            {
                StbImage.stbi_set_flip_vertically_on_load(0);                                       // No flip on skybox textures
                using Stream stream = File.OpenRead(paths[i]);
                image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                GL.TexImage2D(
                    TextureTarget.TextureCubeMapPositiveX + i,                                      // each face is consequtive
                    0, 
                    PixelInternalFormat.Rgba, 
                    image.Width, 
                    image.Height, 
                    0, 
                    PixelFormat.Rgba, 
                    PixelType.UnsignedByte, 
                    image.Data
                );
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error: Could not load texture: {paths[i]}, {e.Message}");           // e.g. corrupt or unsupported file
            }   
        }
    }

    private void CreateCubemap()
    {
        textureInstance = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, textureInstance);

        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
    }

    public void UseTextures()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.TextureCubeMap, textureInstance);
    }

    public void Cleanup()
    {
        GL.DeleteTexture(textureInstance);
    }
}

using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

public class Texture
{
    string path;
    ImageResult image;
    int textureInstance;

    public Texture(string _path)
    {
        path = _path;
        LoadImage();
        CreateTexture();
    }

    public void LoadImage()
    {
        StbImage.stbi_set_flip_vertically_on_load(1); // OpenGL reads bottom-up
        using Stream stream = File.OpenRead(path);
        image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
    }

    public void CreateTexture()
    {
        textureInstance = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureInstance);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public void UseTexture()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textureInstance);
    }

    public void Cleanup()
    {
        GL.DeleteTexture(textureInstance);
    }
}
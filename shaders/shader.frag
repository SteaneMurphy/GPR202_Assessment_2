#version 330 core

/*
    This shader adjusts each pixel of the Torus. It outputs a final
    colour which is applied to the specific pixel.

    The shader takes in the texture reference and a textureMix value, which
    is a mix of how much colour is applied in place of the texture on the same
    pixel.
*/
uniform sampler2D tex;              // texture to be rendered
uniform float textureMix;           // amount of texture/colour mix to be applied (0 = full vertex colour, 1 = full texture)

in vec3 vCol;                       // colours sent to frag
in vec2 vUV;                        // UVs sent to frag

out vec4 fragColour;                // pixel colour out

void main()
{
    fragColour = mix(vec4(vCol, 1.0), texture(tex, vUV), textureMix);
}

/*
    References

    de Vries, J. (n.d.-a). Shaders. LearnOpenGL.
        https://learnopengl.com/Getting-started/Shaders

    de Vries, J. (n.d.-b). Textures. LearnOpenGL.
        https://learnopengl.com/Getting-started/Textures

    Khronos Group. (n.d.-a). mix [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/mix.xhtml

    Khronos Group. (n.d.-b). texture [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/texture.xhtmlrt
*/

#version 330 core

/*
    Skybox fragment shader. Samples the cubemap in the direction from the
    centre of the cube to this pixel, so no UVs are needed.
*/

in vec3 vDir;                       // direction from the cube's centre
out vec4 fragColour;

uniform samplerCube skybox;

void main()
{
    fragColour = texture(skybox, vDir);
}

/*
    References

    de Vries, J. (n.d.). Cubemaps. LearnOpenGL.
        https://learnopengl.com/Advanced-OpenGL/Cubemaps

    Khronos Group. (n.d.). texture [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/texture.xhtml
*/

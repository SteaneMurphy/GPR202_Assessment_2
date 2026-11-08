#version 330 core

/*
    Skybox vertex shader. Takes the cube's corner positions only.

    The view matrix has its translation removed in Skybox.cs, so the cube stays
    centred on the camera. Setting z to w makes the depth 1.0 (the far plane)
    after the perspective divide, so the sky is always behind everything else.
*/

layout (location = 0) in vec3 aPos;

uniform mat4 view;
uniform mat4 projection;

out vec3 vDir;

void main()
{
    vDir = aPos;                                        // cube position = direction to look up in the cubemap
    vec4 pos = vec4(aPos, 1.0) * view * projection;     // same order as shader.vert
    gl_Position = pos.xyww;                             // z = w
}

/*
    References

    de Vries, J. (n.d.). Cubemaps. LearnOpenGL.
        https://learnopengl.com/Advanced-OpenGL/Cubemaps

    Khronos Group. (n.d.). gl_Position [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/gl_Position.xhtml
*/

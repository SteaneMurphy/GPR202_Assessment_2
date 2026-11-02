#version 330 core

/*
    This shader takes in a 8-float vertex whereby the values are
    split into vertex position, colour and UV coordinates.

    It provides a matrix calculation on the position vertices using the
    pre-calculated "mvp" and outputs the colour/UVs to the frag shader.

    gl_Position results in clip space.
*/

layout (location = 0) in vec3 aPos;             // vertices position (vector3)
layout (location = 1) in vec3 aNorm;            // vertices normal (vector3)
layout (location = 2) in vec2 aUV;              // vertices UV (vector2)

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
// uniform mat4 mvp;                               // mvp = model * view * projection (matrix4)

out vec3 vNorm;                                 // normal vertices out to frag
out vec2 vUV;                                   // UV vertices out to frag

void main()
{
    vec4 worldPos = vec4(aPos, 1.0) * model;    // creates the world space vector from the model matrix -> world space (split for lighting later)
    gl_Position = worldPos * view * projection; // multiplies each vertice by view and projection matrix -> clip space
    vNorm = aNorm;                              // assigns the colours out
    vUV = aUV;                                  // assigns the UVs out
}

/*
    References

    de Vries, J. (n.d.-a). Coordinate systems. LearnOpenGL.
        https://learnopengl.com/Getting-started/Coordinate-Systems

    de Vries, J. (n.d.-b). Shaders. LearnOpenGL.
        https://learnopengl.com/Getting-started/Shaders

    Khronos Group. (n.d.). gl_Position [OpenGL 4 reference page].
        https://registry.khronos.org/OpenGL-Refpages/gl4/html/gl_Position.xhtml
*/

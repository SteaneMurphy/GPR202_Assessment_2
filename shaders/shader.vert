#version 330 core

/*
    This shader takes in a 8-float vertex whereby the values are
    split into vertex position, colour and UV coordinates.

    It provides a matrix calculation on the position vertices using the
    pre-calculated "mvp" and outputs the colour/UVs to the frag shader.

    gl_Position results in clip space.
*/

layout (location = 0) in vec3 aPos;             // vertices position (vector3)
layout (location = 1) in vec3 aCol;             // vertices colour (vector3)
layout (location = 2) in vec2 aUV;              // vertices UV (vector2)

uniform mat4 mvp;                               // mvp = model * view * projection (matrix4)

out vec3 vCol;                                  // colour vertices out to frag
out vec2 vUV;                                   // UV vertices out to frag

void main()
{
    gl_Position = vec4(aPos, 1.0) * mvp;        // multiplies the position vertices by the mvp (model/view/projection) matrix
    vCol = aCol;                                // assigns the colours out
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

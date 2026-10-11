#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aCol;
out vec3 vCol;
uniform mat4 mvp;
uniform float time;

void main()
{
    gl_Position = vec4(aPos, 1.0) * mvp;
    vCol = aCol;
}
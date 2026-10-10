#version 330 core

out vec4 fragColour;
in vec3 vCol;

void main()
{
    fragColour = vec4(vCol, 1.0);
}
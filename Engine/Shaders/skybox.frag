#version 330 core

in vec3 vDir;
out vec4 fragColour;

uniform samplerCube skybox;

void main()
{
    fragColour = texture(skybox, vDir);
}
#version 330
const int MaxLights = 3;

//============================================TEXTURE====================================================//
uniform sampler2D TextureSampler;
in vec2 oTexCoords;

vec3 TextureData()
{
	return vec3(texture(TextureSampler, oTexCoords).rgb);
}
//=======================================================================================================//


//=============================================LIGHT=====================================================//
uniform vec4 uEyePosition;

in vec4 oNormal; 
in vec4 oSurfacePosition;

layout(location = 0) out vec4 FragColour;

struct SpotLightProperties {
	vec4 Position;
	vec3 AmbientLight;
	vec3 DiffuseLight;
	vec3 SpecularLight;
	
	vec3 Attenuation;

	vec3 Direction;
	float CutOff;
	float OuterCutOff;
};

uniform SpotLightProperties spotLight;

struct LightProperties {
	vec4 Position;
	vec3 AmbientLight;
	vec3 DiffuseLight;
	vec3 SpecularLight;

	vec3 Attenuation;
};

uniform LightProperties uLight[MaxLights];

struct MaterialProperties {
	vec3 AmbientReflectivity; 
	vec3 DiffuseReflectivity; 
	vec3 SpecularReflectivity; 
	float Shininess;
};

uniform MaterialProperties uMaterial;

vec4 PointLights()
{
	vec4 returnColour;

	for(int i = 0; i < uLight.length(); i++) 
	{
		vec4 lightDir = normalize(uLight[i].Position - oSurfacePosition);
		vec4 eyeDirection = normalize(uEyePosition - oSurfacePosition);
		vec4 reflectedVector = reflect(-lightDir, oNormal);

		float specularFactor = pow(max(dot(reflectedVector, eyeDirection), 0), uMaterial.Shininess);
		float diffuseFactor = max(dot(oNormal, lightDir), 0); 
		float ambientFactor = 0.2f;

		//Attenuation
		float distance = length(uLight[i].Position - oSurfacePosition);
		float attenuationFactor = uLight[i].Attenuation.x + (uLight[i].Attenuation.y * distance) + (uLight[i].Attenuation.z * distance * distance); 
		diffuseFactor = diffuseFactor / attenuationFactor;
		specularFactor = specularFactor / attenuationFactor;


		FragColour = FragColour + vec4(
		uLight[i].AmbientLight * uMaterial.AmbientReflectivity * ambientFactor + 
		uLight[i].DiffuseLight * uMaterial.DiffuseReflectivity * diffuseFactor * TextureData() + 
		uLight[i].SpecularLight * uMaterial.SpecularReflectivity * specularFactor, 1);
	}

	return returnColour;
}

void SpotLight()
{
	//SpotLight
	vec4 lightDir = normalize(spotLight.Position - oSurfacePosition);
	vec4 eyeDirection = normalize(uEyePosition - oSurfacePosition);
	vec4 reflectedVector = reflect(-lightDir, oNormal);

	float specularFactor = pow(max(dot(reflectedVector, eyeDirection), 0), uMaterial.Shininess);
	float diffuseFactor = max(dot(oNormal, lightDir), 0); 
	float ambientFactor = 0.2f;
	

	//Spotlight
	float angle = degrees(acos(dot(normalize(vec4(spotLight.Direction,0)), lightDir)));
	float epsilon =  spotLight.CutOff - spotLight.OuterCutOff;
	float intensity = clamp((angle - spotLight.OuterCutOff) / epsilon, 0.0, 1.0);
	diffuseFactor *= intensity;
	specularFactor *= intensity;

	//Attenuation
	float distance = length(spotLight.Position - oSurfacePosition);
	float attenuationFactor = spotLight.Attenuation.x + (spotLight.Attenuation.y * distance) + (spotLight.Attenuation.z * distance * distance); 
	diffuseFactor = diffuseFactor / attenuationFactor;
	specularFactor = specularFactor / attenuationFactor;

	 FragColour = FragColour + vec4(
	 spotLight.AmbientLight * uMaterial.AmbientReflectivity * ambientFactor +
	 spotLight.DiffuseLight * uMaterial.DiffuseReflectivity * diffuseFactor + 
	 spotLight.SpecularLight * uMaterial.SpecularReflectivity * specularFactor, 1);
}
//=======================================================================================================//


void main() 
{ 
	PointLights();
	SpotLight();
}


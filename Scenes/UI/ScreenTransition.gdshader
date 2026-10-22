shader_type canvas_item;

uniform sampler2D wipeImage;
uniform float percent : hint_range(0, 1);

void fragment() {
	float textVal = texture(wipeImage, UV).r;
	
	if (textVal < percent){
		COLOR.a = 0.0;
	}
}
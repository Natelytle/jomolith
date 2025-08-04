extends Camera3D

@onready
var spring_arm := $"../PlayerCamera/SpringPosition"
var lerp_power: float = 20.0

func _process(delta: float) -> void:
	position = lerp(position, spring_arm.position, delta*lerp_power)

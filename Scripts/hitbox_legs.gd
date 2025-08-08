extends Node3D

@onready var center: RayCast3D = $"Center/RayCast3D"
@onready var left: RayCast3D = $"Left/RayCast3D"
@onready var right: RayCast3D = $"Right/RayCast3D"
@onready var front: RayCast3D = $"Front/RayCast3D"
@onready var back: RayCast3D = $"Back/RayCast3D"
@onready var frontLeft: RayCast3D = $"FrontLeft/RayCast3D"
@onready var frontRight: RayCast3D = $"FrontRight/RayCast3D"
@onready var backLeft: RayCast3D = $"BackLeft/RayCast3D"
@onready var backRight: RayCast3D = $"BackRight/RayCast3D"

var centerPoints = []
var otherPoints = []

func _ready() -> void:
	centerPoints.append_array([center, front, back])
	otherPoints.append_array([left, right, frontLeft, frontRight, backLeft, backRight])

func get_average_length() -> float:
	var centerDistances = []

	for point in centerPoints:
		if point.is_colliding():
			centerDistances.append(point.global_transform.origin.distance_to(point.get_collision_point()))

	if centerDistances.size() > 0:
		centerDistances.sort()

		var sum = 0
		var div = 0

		for i in min(centerDistances.size(), 6):
			sum += centerDistances[i]
			div += 1

		return sum / div
	
	var minDistance = 4

	for point in otherPoints:
		if point.is_colliding():
			minDistance = min(point.global_transform.origin.distance_to(point.get_collision_point()), minDistance)

	return minDistance
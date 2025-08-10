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

var leftPoints = []
var centerPoints = []
var rightPoints = []

var allPoints = []

func _ready() -> void:
	leftPoints.append_array([left, frontLeft, backLeft])
	centerPoints.append_array([center, front, back])
	rightPoints.append_array([right, frontRight, backRight])
	allPoints.append_array([leftPoints, centerPoints, rightPoints])

func get_average_length() -> float:
	var averages = []

	for points in [leftPoints, centerPoints, rightPoints]:
		var sum = 0
		var div = 0

		for point in points:
			if point.is_colliding():
				# subtract 0.05 because we shifted all the raycasts into the body by that length
				var length = point.global_transform.origin.distance_to(point.get_collision_point()) - 0.05

				sum += length
				div += 1
		
		if div > 0:
			averages.append(sum / div)

	
	if averages.size() == 0:
		return 4

	return _calculate_median(averages)


# func get_min_length() -> float:



func _calculate_median(data_list: Array) -> float:
	# Sort the list in ascending order.
	data_list.sort()

	var list_size = data_list.size()
	var median_value: float

	if list_size % 2 == 1:
		# If the list size is odd, the median is the middle element.
		var middle_index = list_size / 2
		median_value = float(data_list[middle_index])
	else:
		# If the list size is even, the median is the average of the two middle elements.
		var index1 = (list_size / 2) - 1
		var index2 = list_size / 2
		median_value = (float(data_list[index1]) + float(data_list[index2])) / 2.0

	return median_value

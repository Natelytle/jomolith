extends RigidBody3D

@onready var baseNode: Node3D = $".."
@onready var camera: Node3D = $"../PlayerCameraPivot"
@onready var hitboxLegs: Node3D = $"PlayerLegs"
@onready var cameraAnchor: Node3D = $"CameraAnchor"

# movement
const MAX_SPEED = 16
const GROUND_ACCELERATION = 800
const AIR_ACCELERATION = 150
const JUMP_VELOCITY = 50
const COYOTE_TIME = 0.08
const PICKLE_TIME = "All The Damn Time"

var currentSpeed = 0
var coyoteTimeTimer = 0

# camera
const SHIFTLOCK_OFFSET = 1.75

var mouse_locked = false
var shift_lock = false
@export var mouse_sensitivity: float = 0.4

func _input(event: InputEvent) -> void:
	if event.is_action("zoom_in") and event.is_pressed():
		camera.zoom_in()
		mouse_locked = shift_lock || camera.first_person

	if event.is_action("zoom_out") and event.is_pressed():
		camera.zoom_out(shift_lock)
		mouse_locked = shift_lock || camera.first_person
	
	if event.is_action("shift_lock") and event.is_pressed():
		shift_lock = !shift_lock
		mouse_locked = shift_lock || camera.first_person

		if shift_lock:
			camera.set_horizontal_offset(SHIFTLOCK_OFFSET)
		else:
			camera.set_horizontal_offset(0)

	if event is InputEventMouseMotion:
		var mouseEvent = event as InputEventMouseMotion

		if Input.is_action_pressed("right_click"):
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
			camera.move(mouseEvent, mouse_sensitivity)
		elif mouse_locked:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
			camera.move(mouseEvent, mouse_sensitivity)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)


func _physics_process(delta: float) -> void:
	var averageLength = hitboxLegs.get_average_length()

	var isOnFloor = averageLength < 2.1

	if not isOnFloor:
		coyoteTimeTimer += delta
	else:
		# 1.65 perfectly counteracts gravity for some reason so we need to add it into our calc
		set_axis_velocity(Vector3.UP * 1.65 + Vector3.UP * (2 - averageLength) * 50)
		coyoteTimeTimer = 0

	if Input.is_action_pressed("jump") and coyoteTimeTimer <= COYOTE_TIME:
		set_axis_velocity(Vector3.UP * JUMP_VELOCITY)
		coyoteTimeTimer += COYOTE_TIME

	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("left", "right", "forward", "backward")
	
	var direction = (baseNode.transform.basis.rotated(Vector3.UP, camera.rotation.y) * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	var targetMovementVector = direction * MAX_SPEED

	if isOnFloor:
		accelerate(targetMovementVector, GROUND_ACCELERATION)
	else:
		accelerate(targetMovementVector, AIR_ACCELERATION)
	
	# we always attempt to upright the character.

	if not mouse_locked:
		rotate_to_direction(direction)


func _integrate_forces(state: PhysicsDirectBodyState3D) -> void:
	# Handle turning with shift-lock
	if mouse_locked:
		rotation.y = camera.rotation.y


func accelerate(target: Vector3, accel: float) -> void:
	var correctionVector = target - Vector3(linear_velocity.x, 0, linear_velocity.z)

	var length = min(accel, 100 * correctionVector.length())

	correctionVector = correctionVector.normalized() * length

	apply_central_force(correctionVector)


func rotate_to_direction(target: Vector3) -> void:
	if target.length() > 0:
		var torque = (-transform.basis.z).signed_angle_to(target, Vector3.UP)

		angular_velocity = baseNode.transform.basis.y * torque * 10
	else:
		angular_velocity = Vector3.UP * 0

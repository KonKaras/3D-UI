using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;


/// <summary>
///     Controller that handles the character controls and camera controls of the first person player.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour, ICharacterSignals
{
	#region Character Signals

	public IObservable<Vector3> Moved => _moved;
	private Subject<Vector3> _moved;

	public ReactiveProperty<bool> IsRunning => _isRunning;
	private ReactiveProperty<bool> _isRunning;

	public IObservable<Unit> Landed => _landed;
	private Subject<Unit> _landed;

	public IObservable<Unit> Jumped => _jumped;
	private Subject<Unit> _jumped;

	public IObservable<Unit> Stepped => _stepped;
	private Subject<Unit> _stepped;

	#endregion

	#region Variables
	[Header("References")]
	[SerializeField] private FirstPersonControllerInput firstPersonControllerInput;
	private CharacterController _characterController;
	private Camera _camera;
	private float walkingSpeed = 5f;
	private float runSpeed = 10f;
	private float jumpSpeed = 5f;
	[SerializeField] private float strideLength = 4f;
	public float StrideLength => strideLength;

	private float stickToGround = 5.0f;

	[Range(0, 80)] [SerializeField] private float minVTopAngle = 20f;
	[Range(0, 80)] [SerializeField] private float minVBottomAngle = 30f;
	#endregion

	public struct MoveInputData
	{
		public readonly Vector2 move;
		public readonly bool jump;
		public MoveInputData(Vector2 move, bool jump)
		{
			this.move = move;
			this.jump = jump;
		}
	}

	private void Awake()
	{
		_characterController = GetComponent<CharacterController>();
		_camera = GetComponentInChildren<Camera>();

		_isRunning = new ReactiveProperty<bool>(false);
		_moved = new Subject<Vector3>().AddTo(this);
	}

	private void Start()
	{
		_characterController.Move(-stickToGround * transform.up);

		MoveHandler();
		LookHandler();
	}

	private void MoveHandler()
	{
		bool wasGrounded = _characterController.isGrounded;

		var jumpLatch = LatchObservables.Latch(this.UpdateAsObservable(), firstPersonControllerInput.Jump, false);
		firstPersonControllerInput.Move
			.Zip(jumpLatch, (m, j) => new MoveInputData(m, j))
			.Where(moveInputData => moveInputData.jump || moveInputData.move != Vector2.zero)
			.Subscribe(input =>
			{
				float speed = firstPersonControllerInput.Run.Value ? runSpeed : walkingSpeed;

				float verticalVelocity = 0f;
				if (input.jump && wasGrounded)
				{// We're on the ground and want to jump.
					verticalVelocity = jumpSpeed;
				}
				else if (!wasGrounded)
				{// We're in the air: apply gravity.
					verticalVelocity = _characterController.velocity.y + (Physics.gravity.y * Time.deltaTime * 3.0f);
				}
				else
				{
					// We're on the ground: push us down a little.
					// (Required for character.isGrounded to work.)
					verticalVelocity = -Mathf.Abs(stickToGround);
				}

				Vector2 horizontalVelocity = input.move * speed;
				Vector3 characterVelocity = transform.TransformVector(new Vector3(
					horizontalVelocity.x,
					verticalVelocity,
					horizontalVelocity.y));
				Vector3 distance = characterVelocity * Time.deltaTime;
				_characterController.Move(distance);

				//Update Values
				bool tempIsRunning = false;
				if (wasGrounded && _characterController.isGrounded)
				{
					// Both started and ended this frame on the ground.
					_moved.OnNext(_characterController.velocity * Time.deltaTime);
					if (_characterController.velocity.magnitude > 0)
					{
						// The chaarcter is running if the input is active and the character is actually moving on the ground
						tempIsRunning = firstPersonControllerInput.Run.Value;
					}
				}
				_isRunning.Value = tempIsRunning;

			}).AddTo(this);
	}

	private void LookHandler()
	{
		firstPersonControllerInput.Look
					.Where(v => v != Vector2.zero)
					.Subscribe(rotation =>
					{
						float horizontal = rotation.x * Time.deltaTime;//TODO multiply mouse sensitivity value
						float vertical = -rotation.y * Time.deltaTime;

						Vector3 old = _camera.transform.localRotation.eulerAngles;
						//Down = 90, forward = 0/360, Up = 270
						float dest = old.x + vertical;
						if (dest > 180)
						{
							float upperBorder = 270 + minVTopAngle;
							if (dest < upperBorder)
							{
								dest = upperBorder;
							}
						}
						else
						{
							float lowerBorder = 90 - minVBottomAngle;
							if (dest > lowerBorder)
							{
								dest = lowerBorder;
							}
						}
						_camera.transform.localRotation = Quaternion.Euler(dest, old.y, 0);
						transform.rotation *= Quaternion.Euler(0, horizontal, 0);

					}).AddTo(this);
	}
}
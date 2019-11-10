using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class MouseController : MonoBehaviour
    {
        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            public float RunMultiplier = 2.0f;   // Speed when sprinting
            public float ClimbSpeed = 4.0f;
            public KeyCode RunKey = KeyCode.LeftShift;
            public float JumpForce = 30f;
            public float m_ThrowForce = 1000f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
            private bool m_Running;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
                if (input == Vector2.zero) return;
                if (input.x > 0 || input.x < 0)
                {
                    //strafe
                    CurrentTargetSpeed = StrafeSpeed;
                }
                if (input.y < 0)
                {
                    //backwards
                    CurrentTargetSpeed = BackwardSpeed;
                }
                if (input.y > 0)
                {
                    //forwards
                    //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                    CurrentTargetSpeed = ForwardSpeed;
                }
#if !MOBILE_INPUT
                if (Input.GetKey(RunKey))
                {
                    CurrentTargetSpeed *= RunMultiplier;
                    m_Running = true;
                }
                else
                {
                    m_Running = false;
                }
#endif
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif
        }


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }


        public Camera cam;
        public int playerNumber = 1;
        public int playerHealth = 3;
        public Transform m_HandTransform = null;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();
        public CatController cat;


        private Rigidbody m_RigidBody;
        private BoxCollider m_Box;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded, m_IsClimbing, m_PreviouslyClimbing, m_CanGrab;
        private Rigidbody m_HoldingItem;
        private bool dead = false;

        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Running
        {
            get
            {
#if !MOBILE_INPUT
                return movementSettings.Running;
#else
	            return false;
#endif
            }
        }


        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Box = GetComponent<BoxCollider>();
            mouseLook.Init(transform, cam.transform, playerNumber);
        }


        private void Update()
        {
            RotateView();

            if (CrossPlatformInputManager.GetButtonDown("Jump" + playerNumber) && !m_Jump)
            {
                m_Jump = true;
            }

            if (playerHealth == 0)
            {
                dead = true;

                m_RigidBody.useGravity = false;
                m_RigidBody.isKinematic = false;
                Vector3 movePosition = transform.position;
 
                movePosition.x = Mathf.MoveTowards(transform.position.x, cat.transform.position.x, 3f * Time.deltaTime);
                movePosition.y = Mathf.MoveTowards(transform.position.y, cat.transform.position.y, 3f * Time.deltaTime);
                movePosition.z = Mathf.MoveTowards(transform.position.z, cat.transform.position.z, 3f * Time.deltaTime);

                m_RigidBody.MovePosition(movePosition);
            }
        }


        private void FixedUpdate()
        {
            if (!dead)
            {
                GroundCheck();
                Vector2 input = GetInput();

                if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
                {
                    // always move along the camera forward as it is the direction that it being aimed at
                    Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
                    desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                    WallCheck(desiredMove);

                    desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                    desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
                    desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;

                    if (m_IsClimbing)
                    {
                        m_RigidBody.velocity = Vector3.up * movementSettings.ClimbSpeed;
                    }
                    else
                    {
                        if (m_RigidBody.velocity.sqrMagnitude <
                            (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
                        {
                            m_RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
                        }
                    }
                }

                if (m_IsGrounded)
                {
                    m_RigidBody.drag = 5f;

                    if (m_Jump)
                    {
                        m_RigidBody.drag = 0f;
                        m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                        m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                        m_Jumping = true;
                    }

                    if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                    {
                        m_RigidBody.Sleep();
                    }
                }
                else
                {
                    m_RigidBody.drag = 0f;
                    if (m_PreviouslyGrounded && !m_Jumping)
                    {
                        StickToGroundHelper();
                    }
                }
                m_Jump = false;
                Collider hit = CheckGrab(cam.transform.forward).collider;
                if (hit != null)
                {
                    IInteractable food = hit.transform.GetComponent<IInteractable>();
                    if (Input.GetButtonDown("Hold" + playerNumber) && m_CanGrab)
                    {
                        if (food != null)
                        {
                            food.Interact(this);
                        }
                    }
                    if (Input.GetButtonDown("Throw" + playerNumber))
                    {
                        if (food != null)
                        {
                            food.Action(this);
                        }
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (dead && collision.gameObject.tag == "Cat")
            {
                Debug.Log("Died");
                cam.enabled = false;
            }
        }

        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, 1f))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }


        private Vector2 GetInput()
        {

            Vector2 input = new Vector2
            {
                x = CrossPlatformInputManager.GetAxis("Horizontal" + playerNumber),
                y = CrossPlatformInputManager.GetAxis("Vertical" + playerNumber)
            };
            movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation(transform, cam.transform);

            if (m_IsGrounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            Debug.DrawRay(transform.position, -Vector3.up, Color.red);
            if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, .5f))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }

        private void WallCheck(Vector3 direction)
        {
            m_PreviouslyClimbing = m_IsClimbing;
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, direction, out hitInfo, 1f))
            {
                m_IsClimbing = true;
            }
            else
            {
                m_IsClimbing = false;
            }
        }

        private RaycastHit CheckGrab(Vector3 direction)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, direction, out hitInfo, 3.5f) && hitInfo.collider.transform.tag == "Interactable")
            {
                m_CanGrab = true;
            }
            else
            {
                m_CanGrab = false;
            }
            return hitInfo;
        }
    }
}

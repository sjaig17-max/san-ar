using UnityEngine;

namespace SAN_AR.Dragon
{
    /// <summary>
    /// Controls dragon procedural or clip-based animations (Idle, Walk, Flying, Roar).
    /// Supports both standard Animator components and procedural fallback
    /// (breathing bob, wing flapping, foot sway) if models lack skinned animation clips.
    /// </summary>
    public class DragonAnimationController : MonoBehaviour
    {
        [Header("Animator Setup (Optional)")]
        [SerializeField] private Animator animator;
        [SerializeField] private string isWalkingParam = "IsWalking";
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string roarTriggerParam = "Roar";

        [Header("Procedural Animation (Fallback)")]
        [Tooltip("Enable organic procedural breathing and wing bobbing")]
        [SerializeField] private bool useProceduralAnimations = true;

        [SerializeField] private Transform dragonBody;
        [SerializeField] private Transform leftWing;
        [SerializeField] private Transform rightWing;

        [SerializeField] private float idleBobFrequency = 2.5f;
        [SerializeField] private float idleBobAmplitude = 0.03f;

        [SerializeField] private float walkFlapFrequency = 6f;
        [SerializeField] private float walkTiltAmplitude = 5f;

        private bool isMoving;
        private float movementMagnitude;
        private Vector3 initialBodyLocalPos;
        private Quaternion initialLeftWingRot;
        private Quaternion initialRightWingRot;
        private float animTimer;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (dragonBody != null)
            {
                initialBodyLocalPos = dragonBody.localPosition;
            }
            else
            {
                initialBodyLocalPos = Vector3.zero;
            }

            if (leftWing != null) initialLeftWingRot = leftWing.localRotation;
            if (rightWing != null) initialRightWingRot = rightWing.localRotation;
        }

        public void SetMoving(bool moving, float magnitude)
        {
            isMoving = moving;
            movementMagnitude = magnitude;

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetBool(isWalkingParam, moving);
                animator.SetFloat(speedParam, magnitude);
            }
        }

        public void TriggerRoarOrReset()
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger(roarTriggerParam);
            }
        }

        private void Update()
        {
            if (!useProceduralAnimations) return;

            animTimer += Time.deltaTime;

            float currentFreq = isMoving ? walkFlapFrequency : idleBobFrequency;
            float currentAmp = isMoving ? (idleBobAmplitude * 1.5f) : idleBobAmplitude;

            // Breathing / Hopping vertical bob
            float verticalOffset = Mathf.Sin(animTimer * currentFreq) * currentAmp;
            
            if (dragonBody != null)
            {
                dragonBody.localPosition = initialBodyLocalPos + new Vector3(0, verticalOffset, 0);

                // Body walk tilt sway
                if (isMoving)
                {
                    float tilt = Mathf.Cos(animTimer * currentFreq) * walkTiltAmplitude;
                    dragonBody.localRotation = Quaternion.Euler(0, 0, tilt);
                }
                else
                {
                    dragonBody.localRotation = Quaternion.identity;
                }
            }

            // Wing flapping
            if (leftWing != null && rightWing != null)
            {
                float flapAngle = Mathf.Sin(animTimer * (isMoving ? 8f : 3f)) * (isMoving ? 20f : 8f);
                leftWing.localRotation = initialLeftWingRot * Quaternion.Euler(flapAngle, 0, flapAngle * 0.5f);
                rightWing.localRotation = initialRightWingRot * Quaternion.Euler(-flapAngle, 0, -flapAngle * 0.5f);
            }
        }
    }
}

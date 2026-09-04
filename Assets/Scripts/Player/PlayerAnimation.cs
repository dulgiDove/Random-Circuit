using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CharacterController characterController;
    [SerializeField]
    private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        Debug.Assert(characterController != null);
        Debug.Assert(animator != null);
    }

    private void Update()
    {
        Vector3 velocity = characterController.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;

        animator.SetFloat(SpeedHash, speed);
    }
}
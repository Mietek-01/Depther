using UnityEngine;

[CreateAssetMenu(fileName = "newItem", menuName = "SO/WallMovementSO", order = 359)]
public class WallMovementSO : ScriptableObject
{
    public float wallSlidingSpeed = 2f;
    public float xWallForce = 700f;
    public float yWallForce = 30f;
    public float wallJumpingTime = 0.05f;

    public bool WallSliding { get; set; }
    public bool WallJumping { get; set; }

}
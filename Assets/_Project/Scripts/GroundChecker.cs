using UnityEngine;

namespace Platformer
{
  public class GroundChecker : MonoBehaviour
  {
    [SerializeField] private Transform groundCheck;
    [SerializeField] float groundDistance = 0.1f;
    [SerializeField] LayerMask groundLayers;
    public bool IsGrounded { get; private set; }


    void Update()
    {
      IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayers);
    }
  }
}

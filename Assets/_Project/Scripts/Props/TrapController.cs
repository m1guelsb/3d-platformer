
using UnityEngine;

namespace Platformer
{
    public class TrapController : MonoBehaviour
    {
        [SerializeField] Vector3 rotateTo = Vector3.zero;
        [SerializeField] float rotationSpeed = 100f;
        void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime * rotateTo);
        }
    }
}
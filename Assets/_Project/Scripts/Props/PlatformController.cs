using System;
using DG.Tweening;
using UnityEngine;

namespace Platformer
{
  public class PlatformerController : MonoBehaviour
  {
    [SerializeField] Vector3 moveTo = Vector3.zero;
    [SerializeField] float duration = 1f;
    [SerializeField] Ease ease = Ease.InOutQuad;
    [SerializeField] LoopType loopType = LoopType.Incremental;


    Vector3 startPosition;

    void Start()
    {
      startPosition = transform.position;
      Move();
    }

    void Move()
    {
      transform.DOMove(startPosition + moveTo, duration).SetEase(ease).SetLoops(-1, loopType);
    }
  }
}

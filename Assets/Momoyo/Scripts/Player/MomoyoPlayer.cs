using rinCore;
using System.Collections.Generic;
using UnityEngine;

namespace Momoyo
{
    public class MomoyoPlayer : MonoBehaviour
    {
        [SerializeField] Rigidbody2D swingArm;
        [SerializeField] List<Collider2D> intercollisionDisable = new();
        [SerializeField] Transform centerTransform;
        Vector2 center => centerTransform.position;
        private void Awake()
        {
            foreach (Collider2D c in intercollisionDisable)
            {
                foreach (var other in intercollisionDisable)
                {
                    if (c == other)
                    {
                        continue;
                    }
                    Physics2D.IgnoreCollision(c, other);
                }
            }
        }
        private void Update()
        {
            if (RenderClickHandler.GetHandler(0, out RenderClickHandler h))
            {
                Vector2 cursor = RenderClickHandler.CursorWorldPosition;
                Vector2 direction = cursor - (Vector2)centerTransform.position;
                float distance = (direction - cursor).magnitude;
                float lerp = distance.MapTo01(0.35f, 4f, true);
                swingArm.MovePosition(center.LerpUnclamped(center + direction.ScaleToMagnitude(1f), 1f - lerp));
                swingArm.transform.Lookat2D(center + direction.ScaleToMagnitude(2f));
            }
        }
    }
}

using rinCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Momoyo
{
    #region Ground Check
    [System.Serializable]
    public class GroundCheck
    {
        [SerializeField] Transform trackedObject;
        [SerializeField] float radius;
        [SerializeField] LayerMask blockingMask;
        Vector2 trackedCenter => !hasObject ? Vector2.zero : trackedObject.position;
        bool hasObject => trackedObject != null;
        public bool IsGrounded
        {
            get
            {
                if (!hasObject)
                {
                    return false;
                }
                if (Physics2D.OverlapCircle(trackedCenter, radius, blockingMask))
                {
                    return true;
                }
                return false;
            }
        }
        public void DrawGizmo()
        {
            if (hasObject)
            {
                Color32 c = IsGrounded ? ColorHelper.PastelGreen : ColorHelper.PastelYellow;
                RinHelper.GizmosDrawCircle(trackedCenter, radius, c, 20);
            }
        }
    }
    #endregion
    #region Jumping
    public partial class MomoyoPlayer
    {
        [SerializeField] InputActionReference jumpAction;
        [SerializeField] GroundCheck groundCheck;
        Coroutine jumpCancel;
        void StartJumpCancel(float forceMultiplier = 0.67f)
        {
            IEnumerator CO_Run()
            {
                yield return null;
                while (RB.linearVelocity.y > 0f && jumpAction.IsPressed())
                {
                    yield return null;
                }
                if (RB.linearVelocity.y > 2f && !jumpAction.IsPressed())
                {
                    RB.linearVelocity = new Vector2(RB.linearVelocity.x, RB.linearVelocity.y * forceMultiplier);
                }
                jumpCancel = null;
            }
            if (jumpCancel == null)
            {
                jumpCancel = StartCoroutine(CO_Run());
            }
        }
        float coyoteEndTime = 0f;
        bool coyoteGrounded => Time.time < coyoteEndTime;
        bool CoyoteJumpQueue
        {
            get
            {
                if (jumpAction.IsPressed() && !jumpAction.PressedLongerThan(0.125f) && coyoteGrounded)
                {
                    return true;
                }
                return false;
            }
        }
        bool NormalJump
        {
            get
            {
                bool isGrounded = groundCheck.IsGrounded;
                if (jumpAction.JustPressed() && isGrounded)
                {
                    return true;
                }
                return false;
            }
        }
    }
    #endregion
    public partial class MomoyoPlayer : FumoUnit
    {
        [SerializeField] Transform centerTransformReference;
        [SerializeField] InputActionReference swingAction;
        public override Vector2 CurrentPosition
        {
            get
            {
                return centerTransformReference.position;
            }
        }
        private void OnDrawGizmos()
        {
            groundCheck.DrawGizmo();
        }
        protected override bool CalculateAlive()
        {
            return gameObject != null && gameObject.activeInHierarchy;
        }
        protected override void WhenAwake()
        {

        }
        protected override void WhenDestroy()
        {

        }
        protected override void WhenStart()
        {
            MineChunker._GEN(0, -1);
            MineChunker.GenPlayerGap(CurrentPosition);
        }
        protected override void WhenUpdate()
        {
            bool realGrounded = groundCheck.IsGrounded;
            if (realGrounded)
            {
                coyoteEndTime = Time.time + 0.125f;
            }
            void moveLoop(Vector2 input)
            {
                input = input.QuantizeToStepSize(45f).ScaleToMagnitude(5f);
                if (input.magnitude < 0.25f)
                {
                    RB.VelocityTowardsX(Vector2.zero, 40f);
                }
                else
                {
                    RB.VelocityTowardsX(input, 60f);
                }
                if (NormalJump || CoyoteJumpQueue)
                {
                    Debug.Log("Test");
                    RB.linearVelocityY = 16f;
                    StartJumpCancel();
                }
            }
            moveLoop(GenericInput.Move);
            if (RenderClickHandler.GetHandler(0, out RenderClickHandler h) && swingAction.IsPressed())
            {
                worldHitPacket packet = new(RenderClickHandler.CursorWorldPosition, 3f);
                if (TryHitWorld(packet, out Vector2 direction))
                {
                    if (MineChunker.TryPlayerCarve(packet.position))
                    {

                    }
                }
            }
        }
        struct worldHitPacket
        {
            public Vector2 position;
            public float maxRange;
            public worldHitPacket(Vector2 position, float range)
            {
                this.position = position;
                this.maxRange = range;
            }
        }
        bool TryHitWorld(worldHitPacket packet, out Vector2 direction)
        {
            direction = packet.position - CurrentPosition;
            Vector2 deltaInt = packet.position.Floor() - CurrentPosition.Floor();
            packet.maxRange = packet.maxRange.Max(2);
            if (deltaInt.x.Absolute() > packet.maxRange || deltaInt.y < -(packet.maxRange - 1) || deltaInt.y > packet.maxRange)
            {
                return false;
            }
            return true;
        }
    }
}

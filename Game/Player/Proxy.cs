using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Game
{
    /// <summary>
    /// No longer really needed but was needed before I started publicizing the assemblies.
    /// </summary>
    internal partial class Player
    {
        public bool IsLocal { get { return Controller != null && GameNetworkManager.Instance != null && Controller == GameNetworkManager.Instance.localPlayerController; } }
        public bool IsJumping { get { if (Controller == null) return false; return Controller.isJumping; } set { if (Controller != null) Controller.isJumping = value; } }
        public bool IsPlayerSliding { get { if (Controller == null) return false; return Controller.isPlayerSliding; } set { if (Controller != null) Controller.isPlayerSliding = value; } }
        public bool IsOwner { get { if (Controller == null) return false; return Controller.IsOwner; } }
        public Coroutine JumpCoroutine { get { if (Controller == null) return null; return Controller.jumpCoroutine; } set { if (Controller != null) Controller.jumpCoroutine = value; } }
        public IEnumerator PlayerJump() { if (Controller == null) return null; return Controller.PlayerJump(); }
        public float PlayerSlidingTimer { get { if (Controller == null) return 0f; return Controller.playerSlidingTimer; } set { if (Controller != null) Controller.playerSlidingTimer = value; } }
        public bool IsPlayerControlled { get { if (Controller == null) return false; return Controller.isPlayerControlled; } }
        public bool IsServer { get { if (Controller == null) return false; return Controller.IsServer; } }
        public bool IsHostPlayerObject { get { if (Controller == null) return false; return Controller.isHostPlayerObject; } }
        public bool IsTestingPlayer { get { if (Controller == null) return false; return Controller.isTestingPlayer; } }
        public bool IsFallingFromJump { get { if (Controller == null) return false; return Controller.isFallingFromJump; } set { if (Controller != null) Controller.isFallingFromJump = value; } }
        public bool IsFallingNoJump { get { if (Controller == null) return false; return Controller.isFallingNoJump; } set { if (Controller != null) Controller.isFallingNoJump = value; } }
        public bool MovingForward { get { if (Controller == null) return false; return Controller.movingForward; } set { if (Controller != null) Controller.movingForward = value; } }
        public bool IsWalking { get { if (Controller == null) return false; return Controller.isWalking; } set { if (Controller != null) Controller.isWalking = value; } }
        public float SprintMultiplier { get { if (Controller == null) return 0f; return Controller.sprintMultiplier; } set { if (Controller != null) Controller.sprintMultiplier = value; } }
        public Vector3 WalkForce { get { if (Controller == null) return Vector3.zero; return Controller.walkForce; } set { if (Controller != null) Controller.walkForce = value; } }
        public QuickMenuManager QuickMenuManager { get { if (Controller == null) return null; return Controller.quickMenuManager; } }
        public bool InSpecialInteractAnimation { get { if (Controller == null) return false; return Controller.inSpecialInteractAnimation; } set { if (Controller != null) Controller.inSpecialInteractAnimation = value; } }
        public bool IsTypingChat { get { if (Controller == null) return false; return Controller.isTypingChat; } }
        public bool IsUnderwater { get { if (Controller == null) return false; return Controller.isUnderwater; } }
        public int IsMovementHindered { get { if (Controller == null) return 0; return Controller.isMovementHindered; } set { if (Controller != null) Controller.isMovementHindered = value; } }
        public int MovementHinderedPrev { get { if (Controller == null) return 0; return Controller.movementHinderedPrev; } set { if (Controller != null) Controller.movementHinderedPrev = value; } }
        public bool IsCrouching { get { if (Controller == null) return false; return Controller.isCrouching; } }
        public bool IsGrounded { get { if (Controller == null) return false; return Controller.thisController.isGrounded; } }
        public float FallValue { get { if (Controller == null) return 0f; return Controller.fallValue; } set { if (Controller != null) Controller.fallValue = value; } }
        public float FallValueUncapped { get { if (Controller == null) return 0f; return Controller.fallValueUncapped; } set { if (Controller != null) Controller.fallValueUncapped = value; } }
        public float JumpForce { get { if (Controller == null) return 0f; return Controller.jumpForce; } set { if (Controller != null) Controller.jumpForce = value; } }
    }
}

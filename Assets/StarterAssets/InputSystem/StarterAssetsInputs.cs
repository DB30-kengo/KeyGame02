using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;
		
		[Header("Input Control")]
		[Tooltip("プレイヤー入力を有効にするかどうか")]
		public bool inputEnabled = true;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		public void OnMove(InputValue value)
		{
			if (inputEnabled)
			{
				MoveInput(value.Get<Vector2>());
			}
			else
			{
				MoveInput(Vector2.zero);
			}
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook && inputEnabled)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			if (inputEnabled)
			{
				JumpInput(value.isPressed);
			}
			else
			{
				JumpInput(false);
			}
		}

		public void OnSprint(InputValue value)
		{
			if (inputEnabled)
			{
				SprintInput(value.isPressed);
			}
			else
			{
				SprintInput(false);
			}
		}


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		/// <summary>
		/// プレイヤー入力の有効/無効を設定
		/// </summary>
		/// <param name="enabled">trueで入力有効、falseで入力無効</param>
		public void SetInputEnabled(bool enabled)
		{
			inputEnabled = enabled;
			
			// 入力を無効にする場合は全ての入力値をクリア
			if (!enabled)
			{
				move = Vector2.zero;
				look = Vector2.zero;
				jump = false;
				sprint = false;
			}
			
			Debug.Log($"[StarterAssetsInputs] プレイヤー入力を{(enabled ? "有効" : "無効")}にしました");
		}
		
		/// <summary>
		/// 現在の入力有効状態を取得
		/// </summary>
		/// <returns>入力が有効かどうか</returns>
		public bool IsInputEnabled()
		{
			return inputEnabled;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}
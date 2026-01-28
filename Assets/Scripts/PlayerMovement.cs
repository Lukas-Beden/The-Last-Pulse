using System;
using System.Collections;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class PlayerMovement : MonoBehaviour
{
    [Header("======| Actions Reference |======")]
    [SerializeField] private InputActionReference _moveActionReference;
    [SerializeField] private InputActionReference _runActionReference;
    [SerializeField] private InputActionReference _lightAttackActionReference;
    [SerializeField] private InputActionReference _heavyAttackActionReference;

    [Header("======| Player Movement |======")]
    [SerializeField] private int _walkSpeed = 5;
    [SerializeField] private int _runSpeed = 15;
    [SerializeField] private float _rotationSpeed = 360f;

    [Header("======| Components Reference |======")]
    [SerializeField] private PlayerState _playerState;
    [SerializeField] private Animator _animator;

    private int _layerIndex;
    private bool _isMovementInputActive;
    private bool _isRunInputHeld;
    private PlayerState.PlayerStateEnum _lastState;

    // Nouvelles variables pour le Blend Tree
    private Vector2 _currentMoveInput;

    void Start()
    {
        _playerState = GetComponent<PlayerState>();
        _layerIndex = _animator.GetLayerIndex("Top Layer");

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SetupInputActions();
    }

    private void SetupInputActions()
    {
        _moveActionReference.action.Enable();
        _runActionReference.action.Enable();
        _lightAttackActionReference.action.Enable();
        _heavyAttackActionReference.action.Enable();

        _moveActionReference.action.started += Move_started;
        _moveActionReference.action.canceled += Move_canceled;

        _runActionReference.action.started += Run_started;
        _runActionReference.action.canceled += Run_canceled;

        _lightAttackActionReference.action.started += LightAttack_started;
        _heavyAttackActionReference.action.started += HeavyAttack_started;
    }

    private void Update()
    {
        UpdateMovementState();
        ExecuteStateAction();
    }

    private void UpdateMovementState()
    {
        if (_playerState.IsInLockingState())
            return;

        if (_isMovementInputActive)
        {
            if (_isRunInputHeld)
                _playerState.SetState(PlayerState.PlayerStateEnum.RUN);
            else
                _playerState.SetState(PlayerState.PlayerStateEnum.WALK);
        }
        else
        {
            _playerState.SetState(PlayerState.PlayerStateEnum.IDLE);
        }
    }

    private void ExecuteStateAction()
    {
        PlayerState.PlayerStateEnum currentState = _playerState.GetPlayerState();

        bool stateChanged = currentState != _lastState;
        _lastState = currentState;

        switch (currentState)
        {
            case PlayerState.PlayerStateEnum.IDLE:
                SetAnimatorIdle();
                break;

            case PlayerState.PlayerStateEnum.WALK:
                SetAnimatorWalk();
                Move(_walkSpeed);
                break;

            case PlayerState.PlayerStateEnum.RUN:
                SetAnimatorRun();
                Move(_runSpeed);
                break;

            case PlayerState.PlayerStateEnum.LIGHTATTACK:
                if (stateChanged)
                {
                    SetAnimatorAction();
                }
                break;

            case PlayerState.PlayerStateEnum.HEAVYATTACK:
                if (stateChanged)
                {
                    SetAnimatorAction();
                }
                break;
        }
    }

    private void SetAnimatorIdle()
    {
        _animator.SetLayerWeight(_layerIndex, 1);
        _animator.SetFloat("Speed", 0f);
        _animator.SetFloat("MoveX", 0f);
        _animator.SetFloat("MoveY", 0f);
    }

    private void SetAnimatorWalk()
    {
        _animator.SetLayerWeight(_layerIndex, 1);
        _animator.SetFloat("Speed", _walkSpeed);
        UpdateBlendTreeDirection();
    }

    private void SetAnimatorRun()
    {
        _animator.SetLayerWeight(_layerIndex, 1);
        _animator.SetFloat("Speed", _runSpeed);
        UpdateBlendTreeDirection();
    }

    private void UpdateBlendTreeDirection()
    {
        // Récupérer l'input brut
        Vector2 moveInput = _moveActionReference.action.ReadValue<Vector2>();

        if (moveInput == Vector2.zero)
            return;

        // Convertir l'input en direction relative à la caméra
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;
        moveDirection.Normalize();

        // Convertir la direction mondiale en direction locale du personnage
        Vector3 localDirection = transform.InverseTransformDirection(moveDirection);

        // Envoyer à l'Animator (X = gauche/droite, Y = avant/arrière dans l'espace local)
        _animator.SetFloat("MoveX", localDirection.x);
        _animator.SetFloat("MoveY", localDirection.z);
    }

    private void SetAnimatorAction()
    {
        _animator.SetLayerWeight(_layerIndex, 0);
    }

    private void Move(int speed)
    {
        Vector2 moveInput = _moveActionReference.action.ReadValue<Vector2>();

        if (moveInput == Vector2.zero)
            return;

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            _rotationSpeed * Time.deltaTime
        );

        transform.position += moveDirection * (speed * Time.deltaTime);
    }

    #region Input Events

    private void Move_started(InputAction.CallbackContext obj)
    {
        _isMovementInputActive = true;
    }

    private void Move_canceled(InputAction.CallbackContext obj)
    {
        _isMovementInputActive = false;
    }

    private void Run_started(InputAction.CallbackContext obj)
    {
        _isRunInputHeld = true;
    }

    private void Run_canceled(InputAction.CallbackContext obj)
    {
        _isRunInputHeld = false;
    }

    private void LightAttack_started(InputAction.CallbackContext obj)
    {
        _playerState.SetState(PlayerState.PlayerStateEnum.LIGHTATTACK);
    }

    private void HeavyAttack_started(InputAction.CallbackContext obj)
    {
        _playerState.SetState(PlayerState.PlayerStateEnum.HEAVYATTACK);
    }

    #endregion

    private void OnDestroy()
    {
        _moveActionReference.action.Disable();
        _runActionReference.action.Disable();
        _lightAttackActionReference.action.Disable();
        _heavyAttackActionReference.action.Disable();
    }
}
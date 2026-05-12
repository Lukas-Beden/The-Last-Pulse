using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public enum PlayerStateEnum
    {
        IDLE,
        WALK,
        RUN,
        //DASH,
        LIGHTATTACK,
        HEAVYATTACK//,
        //SKILL1,
        //SKILL2
    }

    [SerializeField] private PlayerStateEnum _currentPlayerState = PlayerStateEnum.IDLE;

    // États qui bloquent le mouvement
    private static readonly HashSet<PlayerStateEnum> _lockingStates = new()
    {
        //PlayerStateEnum.DASH,
        PlayerStateEnum.LIGHTATTACK,
        PlayerStateEnum.HEAVYATTACK,
        //PlayerStateEnum.SKILL1,
        //PlayerStateEnum.SKILL2
    };

    private static readonly Dictionary<PlayerStateEnum, HashSet<PlayerStateEnum>> _allowedTransitions = new()
    {
        { PlayerStateEnum.IDLE, new() { PlayerStateEnum.WALK, PlayerStateEnum.RUN, /*PlayerStateEnum.DASH,*/ PlayerStateEnum.LIGHTATTACK, PlayerStateEnum.HEAVYATTACK/*, PlayerStateEnum.SKILL1, PlayerStateEnum.SKILL2*/ } },
        { PlayerStateEnum.WALK, new() { PlayerStateEnum.IDLE, PlayerStateEnum.RUN, /*PlayerStateEnum.DASH,*/ PlayerStateEnum.LIGHTATTACK, PlayerStateEnum.HEAVYATTACK/*, PlayerStateEnum.SKILL1, PlayerStateEnum.SKILL2*/ } },
        { PlayerStateEnum.RUN, new() { PlayerStateEnum.IDLE, PlayerStateEnum.WALK, /*PlayerStateEnum.DASH,*/ PlayerStateEnum.LIGHTATTACK, PlayerStateEnum.HEAVYATTACK/*, PlayerStateEnum.SKILL1, PlayerStateEnum.SKILL2*/ } },
        //{ PlayerStateEnum.DASH, new() { PlayerStateEnum.IDLE, PlayerStateEnum.WALK, PlayerStateEnum.RUN } },
        { PlayerStateEnum.LIGHTATTACK, new() { PlayerStateEnum.IDLE, PlayerStateEnum.WALK, PlayerStateEnum.RUN } },
        { PlayerStateEnum.HEAVYATTACK, new() { PlayerStateEnum.IDLE, PlayerStateEnum.WALK, PlayerStateEnum.RUN } }//,
        //{ PlayerStateEnum.SKILL1, new() { PlayerStateEnum.IDLE, PlayerStateEnum.WALK, PlayerStateEnum.RUN } },
        //{ PlayerStateEnum.SKILL2, new() { PlayerStateEnum.IDLE, PlayerStateEnum.WALK, PlayerStateEnum.RUN } }
    };

    public bool SetState(PlayerStateEnum newState)
    {
        if (IsAllowed(newState))
        {
            _currentPlayerState = newState;
            return true;
        }
        return false;
    }

    public PlayerStateEnum GetPlayerState()
    {
        return _currentPlayerState;
    }

    public bool IsInLockingState()
    {
        return _lockingStates.Contains(_currentPlayerState);
    }

    private bool IsAllowed(PlayerStateEnum newState)
    {
        return _allowedTransitions[_currentPlayerState].Contains(newState);
    }

    // Appelée par Animation Event à la fin des animations de DASH/ATTACK
    public void OnActionAnimationComplete()
    {
        _currentPlayerState = PlayerStateEnum.IDLE;
    }
}
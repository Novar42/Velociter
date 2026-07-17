using UnityEngine;
using System.Collections;

public class Shooter : MonoBehaviour
{
    public float _range, _enableRange, _aimDuration, _shootDuration;
    public Vector3 _aimPoint;
    public enum CurrentState {Aiming, Shooting, Waiting, Disabled}
    public CurrentState _currentState;
    public Weapon _weapon;
    public Rigidbody2D _body;
    public LineRenderer line;

    public void Awake()
    {
        _currentState = CurrentState.Disabled;
    }

    void Start()
    {
        Player._player.enemies.Add(gameObject);
    }

    void FixedUpdate()
    {
        if (_currentState == CurrentState.Disabled)
        {
            if (Vector2.Distance(Player._player.gameObject.transform.position, transform.position) <= _enableRange)
            {
                _currentState = CurrentState.Waiting;
            }
        }
        else
        {
            if (Vector2.Distance(Player._player.gameObject.transform.position, transform.position) <= _range)
            {
                UpdateDirection();
                if (_weapon._currentWeaponState == Weapon.CurrentState.Working)
                {
                    switch (_weapon._currentWeapon)
                    {
                        case Weapon.CurrentWeapon.Laser:
                            switch (_currentState)
                            {
                                case CurrentState.Aiming:
                                    _weapon._laser.AimCheck((Vector2)_aimPoint - (Vector2)transform.position, Color.red);
                                    break;
                                case CurrentState.Shooting:
                                    _weapon._laser.AimCheck((Vector2)_aimPoint - (Vector2)transform.position, Color.yellow);
                                    break;
                                case CurrentState.Waiting:
                                    StartCoroutine(IAim(_aimDuration));
                                    break;
                            }
                            break;
                        case Weapon.CurrentWeapon.Rocket:
                            switch (_currentState)
                            {
                                case CurrentState.Waiting:
                                    StartCoroutine(IShoot(_shootDuration));
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    StopAllCoroutines();
                    _currentState = CurrentState.Waiting;
                }
            }
            else if (_currentState != CurrentState.Waiting)
            {
                StopAllCoroutines();
                _currentState = CurrentState.Waiting;
                line.enabled = false;
            }
        }
    }

    public void UpdateDirection()
    {
        if (_weapon._laser.equiped == true && _currentState == CurrentState.Aiming)
        {
            _aimPoint = (Vector2)Player._player.gameObject.transform.position + Player._player._body.linearVelocity * _shootDuration;
            transform.rotation = Quaternion.Euler(0f, 0f, Player._player.DirectionToAngle(_aimPoint - transform.position));
        }
        else if (_currentState != CurrentState.Shooting)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Player._player.DirectionToAngle(Player._player.gameObject.transform.position - transform.position));
        }
    }

    public IEnumerator IAim(float duration)
    {
        _currentState = CurrentState.Aiming;
        yield return new WaitForSeconds(duration);
        StartCoroutine(IShoot(_shootDuration));
    }
    
    public IEnumerator IShoot(float duration)
    {
        _currentState = CurrentState.Shooting;
        yield return new WaitForSeconds(duration);
        switch (_weapon._currentWeapon)
        {
            case Weapon.CurrentWeapon.Laser:
                _weapon._laser.Shoot((Vector2)_aimPoint - (Vector2)transform.position, Color.white);
                break;
            case Weapon.CurrentWeapon.Rocket:
                _weapon._rocket.Shoot(Player._player.gameObject);
                break;
        }
    }
}

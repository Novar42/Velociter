using UnityEngine;
using System.Collections;

public class Shooter : MonoBehaviour
{
    public float _health, _damages, _range, _enableRange, _aimDuration, _shootDuration, _cooldownDuration;
    public LayerMask _shootInteract;
    public enum CurrentState {Aiming, Shooting, Cooldown, Waiting, Disabled}
    public Vector3 _aimPoint;
    public CurrentState _currentState;
    public Rigidbody2D _body;
    public LineRenderer line;

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
                line.SetPosition(0, transform.position);
                switch (_currentState)
                {
                    case CurrentState.Aiming:
                        AimCheck((Vector2)_aimPoint);
                        break;
                    case CurrentState.Shooting:
                        if (AimCheck((Vector2)_aimPoint).transform == Player._player.gameObject.transform)
                        {
                            Player._player.HealthChange(-1);
                            StopAllCoroutines();
                            StartCoroutine(ICooldown(_cooldownDuration));
                        }
                        break;
                    case CurrentState.Cooldown:
                        break;
                    case CurrentState.Waiting:
                        StartCoroutine(IAim(_aimDuration));
                        break;
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
        if (_currentState == CurrentState.Aiming)
        {
            _aimPoint = (Vector2)Player._player.gameObject.transform.position + Player._player._body.linearVelocity * _shootDuration;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2((_aimPoint - transform.position).y, (_aimPoint - transform.position).x) * Mathf.Rad2Deg - 90f);
        }
        else if (_currentState == CurrentState.Cooldown)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(Player._player.gameObject.transform.position.y, Player._player.gameObject.transform.position.x) * Mathf.Rad2Deg - 90f);
        }
    }

    public RaycastHit2D AimCheck(Vector2 target)
    {
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, target - (Vector2)transform.position, _range, _shootInteract);
        if (hit)
        {
            line.SetPosition(1, hit.point);
        }
        else
        {
            line.SetPosition(1, ((Vector3)target - transform.position).normalized * _range + transform.position);
        } 
        return hit;
    }

    public IEnumerator IAim(float duration)
    {
        _currentState = CurrentState.Aiming;
        line.enabled = true;
        line.material.SetColor("_Color", Color.red);
        line.material.SetColor("_EmissionColor", Color.red);
        yield return new WaitForSeconds(duration);
        StartCoroutine(IShoot(_shootDuration));
    }
    
    public IEnumerator IShoot(float duration)
    {
        _currentState = CurrentState.Shooting;
        line.material.SetColor("_Color", Color.yellow);
        line.material.SetColor("_EmissionColor", Color.yellow);
        yield return new WaitForSeconds(duration);
        StartCoroutine(ICooldown(_cooldownDuration));
    }

    public IEnumerator ICooldown(float duration)
    {
        _currentState = CurrentState.Cooldown;
        line.enabled = false;
        yield return new WaitForSeconds(duration);
        StartCoroutine(IAim(_aimDuration));
    }
}

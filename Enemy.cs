using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public Weapon _weapon;
    public Rigidbody2D _body;
    public Animator _anim;
    public Renderer _renderer;
    public GameObject _eye, _pupil, _pinPrefab, _pin;
    public int _difficulty;

    void Awake()
    {
        _weapon._currentTarget = Player._player.gameObject;
        _pupil = _eye.transform.GetChild(0).GetChild(0).gameObject;
        StartCoroutine(IEyeBlink(5f));
    }

    void FixedUpdate()
    {
        _eye.transform.rotation = Quaternion.Euler(0, 0, 0);
        _pupil.transform.position = _pupil.transform.parent.TransformPoint((Player._player.gameObject.transform.position - _pupil.transform.parent.position).normalized * 0.2f);
        if (!_renderer.isVisible)
        {
            if (_pin == null)
            {
                _pin = GameObject.Instantiate(_pinPrefab);
                _pin.transform.parent = Player._player.gameObject.transform;
                _pin.transform.position = Player._player.gameObject.transform.position + Vector3.up * 5f;
            }
            else
            {
                float angleTarget = GameManager.DirectionToAngle((Vector2)(transform.position - Player._player.gameObject.transform.position).normalized);
                float anglePin = GameManager.DirectionToAngle((Vector2)(_pin.transform.position - Player._player.gameObject.transform.position).normalized);
                _pin.transform.RotateAround(Player._player.gameObject.transform.position, Vector3.forward, angleTarget - anglePin);
            }
        }
        else
        {
            if (_pin != null)
            {
                Destroy(_pin);
            }
        }
    }

    public IEnumerator IEyeBlink(float duration)
    {
        yield return new WaitForSeconds(duration);
        _anim.Play("EyeBlinking");
        StartCoroutine(IEyeBlink(duration));
    }
}

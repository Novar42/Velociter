using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
public class Player : MonoBehaviour
{
    public static Player _player;
    public GameObject _cam;
    public Rigidbody2D _body;
    [Header("health")]
    public int _health;
    public int _maxHealth;
    public GameObject _prefabHealthUI;
    public GameObject _healthCountUI;
    [Header("Dash")]
    public int _dashCount;
    public int _maxDash;
    public GameObject _prefabDashUI;
    public GameObject _dashCountUI;
    [Header("Movement")]
    public float _speed;
    public float _acceleration;
    public float _maxSpeed;
    public TMP_Text _speedUI;
    public Vector2 _movement;
    [Header("Inputs")]
    public Vector3 _worldMousePosition;
    public enum CurrentDevice {Gamepad, Keyboard, Mobile}
    public CurrentDevice _currentDevice;
    private InputAction _mousePosition, _direction, _dash, _drift, _shoot;

    void Awake()
    {
        _player = this;
        _mousePosition = InputSystem.actions.FindAction("Mouse Position");
        _direction = InputSystem.actions.FindAction("Direction");
        _dash = InputSystem.actions.FindAction("Dash");
        _drift = InputSystem.actions.FindAction("Drift");
        _shoot = InputSystem.actions.FindAction("Shoot");
        for (int i = 0; i < _maxHealth; i++)
        {
            Instanciate(_prefabHealthUI, _healthCountUI.transform);
        }
        for (int i = 0; i < _maxDash; i++)
        {
            Instanciate(_prefabDashUI, _dashCountUI.transform);
        }
    }

    void FixedUpdate()
    {
        _movement = Vector2.zero;
        _cam.transform.position = new Vector3(transform.position.x, transform.position.y, _cam.transform.position.z);
        _body.linearVelocity = Vector2.ClampMagnitude(_body.linearVelocity, _maxSpeed);
        _worldMousePosition = _cam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(_mousePosition.ReadValue<Vector2>().x, _mousePosition.ReadValue<Vector2>().y, 0)) - _cam.transform.position;
        UpdateSpeed();
        UpdateDirection();
        if (!_drift.IsPressed())
        {
            _movement = Vector2.up * _acceleration;
        }
        if (_dash.WasPerformedThisFrame())
        {
            Dash();
        }
        _body.AddRelativeForce(_movement);
    }

    public void UpdateSpeed()
    {
        _speed = _body.linearVelocity.magnitude;
        _speedUI.text = (Mathf.Round(_speed * 10) * 0.1f) + " m/s";
    }

    public void UpdateDirection()
    {
        if (_currentDevice == CurrentDevice.Gamepad)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_direction.ReadValue<Vector2>().y, _direction.ReadValue<Vector2>().x) * Mathf.Rad2Deg - 90f);
        }
        if (_currentDevice == CurrentDevice.Keyboard || _currentDevice == CurrentDevice.Mobile)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_worldMousePosition.y, _worldMousePosition.x) * Mathf.Rad2Deg - 90f);
        }
    }

    public void Dash(Vector2 input = default)
    {
        if (input != default)
        {
            _body.linearVelocity = input * _speed;
        }
        else
        {
            if (_currentDevice == CurrentDevice.Keyboard || _currentDevice == CurrentDevice.Mobile)
            {
                _body.linearVelocity = _worldMousePosition.normalized * _speed;
            }
            else if (_currentDevice == CurrentDevice.Gamepad)
            {
                _body.linearVelocity = _direction.ReadValue<Vector2>() * _speed;
            }
        }
    }

    public void HealthChange(int amout)
    {
        _health = Mathf.Clamp(0, _maxHealth, _health + amout);
        if (Mathf.Sign(amout) == 1)
        {
            for (int i = 0; i < _healthCountUI.transform.childCount; )
        }
        if (_health == 0)
        {
            Die();
        }
    }

    public void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CheckCurrentDevice(InputAction.CallbackContext call)
    {
        if (call.control.device is Gamepad)
        {
            _currentDevice = CurrentDevice.Gamepad;
            print("gamepad");
        }
        else if (call.control.device is Keyboard || call.control.device is Mouse)
        {
            _currentDevice = CurrentDevice.Keyboard;
            print("keyboard");
        }
        else if (call.control.device is Touchscreen)
        {
            _currentDevice = CurrentDevice.Mobile;
            print("mobile");
        }
    }
}

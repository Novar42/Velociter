using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
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
    public bool _canDash = true;

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
            Instantiate(_prefabHealthUI, _healthCountUI.transform);
        }
        for (int i = 0; i < _maxDash; i++)
        {
            Instantiate(_prefabDashUI, _dashCountUI.transform);
        }
    }

    void OnEnable()
    {
        _mousePosition.performed += InputHub;
        _mousePosition.Enable();
        _direction.performed += InputHub;
        _direction.Enable();
        _dash.performed += InputHub;
        _dash.Enable();
    }

    void OnDisable()
    {
        _mousePosition.performed -= InputHub;
        _mousePosition.Enable();
        _direction.performed -= InputHub;
        _direction.Enable();
        _dash.performed -= InputHub;
        _dash.Enable();
    }

    void FixedUpdate()
    {
        _movement = Vector2.zero;
        _cam.transform.position = new Vector3(transform.position.x, transform.position.y, _cam.transform.position.z);
        _body.linearVelocity = Vector2.ClampMagnitude(_body.linearVelocity, _maxSpeed);
        UpdateSpeed();
        if (!_drift.IsPressed())
        {
            _movement = Vector2.up * _acceleration;
        }
        _body.AddRelativeForce(_movement);
    }

    public void InputHub(InputAction.CallbackContext context)
    {
        if (context.control.device is Gamepad)
        {
            _currentDevice = CurrentDevice.Gamepad;
        }
        else if (context.control.device is Keyboard || context.control.device is Mouse)
        {
            _currentDevice = CurrentDevice.Keyboard;
        }
        else if (context.control.device is Touchscreen)
        {
            _currentDevice = CurrentDevice.Mobile;
        }
        if (context.action == _mousePosition || context.action == _direction)
        {
            UpdateDirection();
        }
        if (context.action == _dash)
        {
            Dash();
        }
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
            _worldMousePosition = _cam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(_mousePosition.ReadValue<Vector2>().x, _mousePosition.ReadValue<Vector2>().y, 0)) - _cam.transform.position;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_worldMousePosition.y, _worldMousePosition.x) * Mathf.Rad2Deg - 90f);
        }
    }

    public void Dash(Vector2 input = default)
    {
        if (_canDash)
        {
            DashChange(-1);
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
    }

    public void HealthChange(int amout)
    {
        _health = Mathf.Clamp(_health + amout, 0, _maxHealth);
        List<Image> images = new List<Image>();
        _healthCountUI.GetComponentsInChildren<Image>(images);
        images.Remove(_healthCountUI.GetComponent<Image>());
        if (Mathf.Sign(amout) == 1)
        {
            GaugeChangeUI(images, amout);
        }
        else
        {
            images.Reverse();
            GaugeChangeUI(images, amout);
        }
        if (_health == 0)
        {
            Die();
        }
    }

    public void DashChange(int amout)
    {
        _dashCount = Mathf.Clamp(_dashCount + amout, 0, _maxDash);
        List<Image> images = new List<Image>();
        _dashCountUI.GetComponentsInChildren<Image>(images);
        images.Remove(_dashCountUI.GetComponent<Image>());
        if (Mathf.Sign(amout) == 1)
        {
            GaugeChangeUI(images, amout);
        }
        else
        {
            images.Reverse();
            GaugeChangeUI(images, amout);
        }
        if (_dashCount == 0 && _canDash)
        {
            _canDash = false;
        }
        else if (_dashCount != 0 && !_canDash)
        {
            _canDash = true;
        }
    }

    public void GaugeChangeUI(List<Image> list, int amout)
    {
        int i = Mathf.Abs(amout);
        int goalValue = Mathf.Clamp(amout, 0, 1);
        foreach (var item in list)
        {
            if (item.color.a == Mathf.Abs(goalValue - 1) && i > 0)
            {
                Color color = new Color(item.color.r, item.color.g, item.color.b, goalValue);
                item.color = color;
                i--;
            }
        }
    }

    public void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

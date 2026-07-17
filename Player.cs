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
    public List<GameObject> enemies = new List<GameObject>();

    [Header("health")]
    public int _health;
    public int _maxHealth;
    public GameObject _prefabHealthUI;
    public GameObject _healthCountUI;

    [Header("Weapon")]
    public Weapon _weapon;
    public bool _isShooting = false;
    public float _range;
    public GameObject _laserWeaponUI;
    public GameObject _missileWeaponUI;

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
    public Vector2 _worldMousePosition;
    public enum CurrentDevice {Gamepad, Keyboard, Mobile}
    public CurrentDevice _currentDevice;
    private InputAction _mousePosition, _direction, _dash, _drift, _shoot, _changeWeapon;

    void Awake()
    {
        _player = this;
        _mousePosition = InputSystem.actions.FindAction("Mouse Position");
        _direction = InputSystem.actions.FindAction("Direction");
        _dash = InputSystem.actions.FindAction("Dash");
        _drift = InputSystem.actions.FindAction("Drift");
        _shoot = InputSystem.actions.FindAction("Shoot");
        _changeWeapon = InputSystem.actions.FindAction("Change Weapon");
        for (int i = 0; i < _maxHealth; i++)
        {
            Instantiate(_prefabHealthUI, _healthCountUI.transform);
        }
        for (int i = 0; i < _maxDash; i++)
        {
            Instantiate(_prefabDashUI, _dashCountUI.transform);
        }
        WeaponChange(1, false);
    }

    void OnEnable()
    {
        _mousePosition.performed += InputHub;
        _mousePosition.Enable();
        _direction.performed += InputHub;
        _direction.Enable();
        _dash.performed += InputHub;
        _dash.Enable();
        _shoot.performed += InputHub;
        _shoot.canceled += InputHub;
        _shoot.Enable();
        _changeWeapon.performed += InputHub;
        _changeWeapon.Enable();
    }

    void OnDisable()
    {
        _mousePosition.performed -= InputHub;
        _mousePosition.Disable();
        _direction.performed -= InputHub;
        _direction.Disable();
        _dash.performed -= InputHub;
        _dash.Disable();
        _shoot.performed -= InputHub;
        _shoot.canceled -= InputHub;
        _shoot.Disable();
        _changeWeapon.performed -= InputHub;
        _changeWeapon.Disable();
    }

    void FixedUpdate()
    {
        _movement = Vector2.zero;
        _body.linearVelocity = Vector2.ClampMagnitude(_body.linearVelocity, _maxSpeed);
        UpdateCamPos(0.3f);
        UpdateSpeed();
        if (!_drift.IsPressed())
        {
            _movement = Vector2.up * _acceleration;
        }
        if (_isShooting)
        {
            UpdateAim();
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
        if (context.action == _shoot)
        {
            if (context.performed)
            {
                _isShooting = true;
            }
            if (context.canceled)
            {
                _isShooting = false;
                Shoot();
            }
        }
        if (context.action == _changeWeapon)
        {
            if (int.TryParse(context.control.name, out int value))
            {
                WeaponChange(value);
            }
            else
            {
                WeaponChange((int)_weapon._currentWeapon);
            }
        }
    }

    public void UpdateSpeed()
    {
        _speed = _body.linearVelocity.magnitude;
        _speedUI.text = (Mathf.Round(_speed * 10) * 0.1f) + " m/s";
    }

    public void UpdateCamPos(float coef)
    {
        _cam.transform.position = new Vector3(transform.position.x * coef, transform.position.y * coef, _cam.transform.position.z);
    }

    public void UpdateDirection()
    {
        if (_currentDevice == CurrentDevice.Gamepad)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, DirectionToAngle(_direction.ReadValue<Vector2>()));
        }
        if (_currentDevice == CurrentDevice.Keyboard || _currentDevice == CurrentDevice.Mobile)
        {
            _worldMousePosition = (Vector2)_cam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(_mousePosition.ReadValue<Vector2>().x, _mousePosition.ReadValue<Vector2>().y));
            transform.rotation = Quaternion.Euler(0f, 0f, DirectionToAngle(_worldMousePosition - (Vector2)transform.position));
        }
    }

    public void UpdateAim()
    {
        if (_weapon._currentWeaponState == Weapon.CurrentState.Working)
        {
            if (_currentDevice == CurrentDevice.Gamepad)
            {
                switch (_weapon._currentWeapon)
                {
                    case Weapon.CurrentWeapon.Laser:
                        _weapon._laser.AimCheck(_direction.ReadValue<Vector2>() + (Vector2)_weapon.transform.position, Color.red);
                        break;
                    case Weapon.CurrentWeapon.Rocket:
                        _weapon._rocket.AimCheck(DirectionToAngle(_direction.ReadValue<Vector2>()), Color.red);
                        break;
                }
            }
            if (_currentDevice == CurrentDevice.Keyboard || _currentDevice == CurrentDevice.Mobile)
            {
                switch (_weapon._currentWeapon)
                {
                    case Weapon.CurrentWeapon.Laser:
                        _weapon._laser.AimCheck(_worldMousePosition - (Vector2)transform.position, Color.red);
                        break;
                    case Weapon.CurrentWeapon.Rocket:
                        _weapon._rocket.AimCheck(_worldMousePosition, Color.red);
                        break;
                }
            }
        }
    }

    public void Shoot()
    {
        if (_weapon._currentWeaponState == Weapon.CurrentState.Working)
        {
            if (_currentDevice == CurrentDevice.Gamepad)
            {
                switch (_weapon._currentWeapon)
                {
                    case Weapon.CurrentWeapon.Laser:
                        _weapon._laser.AimCheck(_direction.ReadValue<Vector2>() + (Vector2)_weapon.transform.position, Color.white);
                        _laserWeaponUI.GetComponentInChildren<Animator>().Play("cooldownUI");
                        _laserWeaponUI.GetComponentInChildren<Animator>().SetFloat("speed", 1 / _weapon._laser.cooldownDuration);
                        break;
                    case Weapon.CurrentWeapon.Rocket:
                        _weapon._rocket.AimCheck(DirectionToAngle(_direction.ReadValue<Vector2>()), Color.red);
                        _missileWeaponUI.GetComponentInChildren<Animator>().Play("cooldownUI");
                        _missileWeaponUI.GetComponentInChildren<Animator>().SetFloat("speed", 1 / _weapon._rocket.cooldownDuration);
                        break;
                }
            }
            if (_currentDevice == CurrentDevice.Keyboard || _currentDevice == CurrentDevice.Mobile)
            {
                switch (_weapon._currentWeapon)
                {
                    case Weapon.CurrentWeapon.Laser:
                        _weapon._laser.Shoot(_worldMousePosition - (Vector2)transform.position, Color.white);
                        _laserWeaponUI.GetComponentInChildren<Animator>().Play("cooldownUI");
                        _laserWeaponUI.GetComponentInChildren<Animator>().SetFloat("speed", 1 / _weapon._laser.cooldownDuration);
                        break;
                    case Weapon.CurrentWeapon.Rocket:
                        _weapon._rocket.Shoot(_worldMousePosition, Color.red);
                        _missileWeaponUI.GetComponentInChildren<Animator>().Play("cooldownUI");
                        _missileWeaponUI.GetComponentInChildren<Animator>().SetFloat("speed", 1 / _weapon._rocket.cooldownDuration);
                        break;
                }
            }
        }
    }

    public void WeaponChange(int value, bool isInput = true)
    {
        if (_currentDevice == CurrentDevice.Gamepad && isInput)
        {
            _weapon._currentWeapon = (Weapon.CurrentWeapon)Mathf.Clamp((int)_weapon._currentWeapon + Mathf.Round(_changeWeapon.ReadValue<float>()), 1, 2);
        }
        else if (_currentDevice == CurrentDevice.Keyboard || _currentDevice == CurrentDevice.Mobile || !isInput)
        {
            _weapon._currentWeapon = (Weapon.CurrentWeapon)Mathf.Clamp(value, 1, 2);
        }
        switch (_weapon._currentWeapon)
        {
            case Weapon.CurrentWeapon.Laser:
                _laserWeaponUI.GetComponent<Image>().color = new Color(1f, 0, 0, 1f);
                _missileWeaponUI.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);
                break;
            case Weapon.CurrentWeapon.Rocket:
                _laserWeaponUI.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);
                _missileWeaponUI.GetComponent<Image>().color = new Color(1f, 0f, 0f, 1f);
                break;
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
                    _body.linearVelocity = (_worldMousePosition - (Vector2)transform.position).normalized * _speed;
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

    public float DirectionToAngle(Vector2 direction)
    {
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
    }
}

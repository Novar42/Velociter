using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Missile : MonoBehaviour
{
    public GameObject _target, _explosionEffect, _explosionParticle;
    public Rigidbody2D _body;
    public CircleCollider2D _explosionCollider;
    public Vector2 _direction;
    public EnnemiHealth _healthSystem;
    public Weapon _launcher;
    public bool _canExplode = true;
    public float _speed;
    public List<GameObject> _objectsInExplosion = new List<GameObject>();

    public void Start()
    {
        Player._player.enemies.Add(gameObject);
        _launcher._rocket.rotationSpeed = Mathf.Clamp(_launcher._rocket.rotationSpeed, 0f, 1f);
        _body.linearDamping = _launcher._rocket.precision;
        _explosionEffect.transform.localScale = new Vector3(_launcher._rocket.explosionRadius, _launcher._rocket.explosionRadius, 1f);
        if (_launcher._mainParent.TryGetComponent<Rigidbody2D>(out Rigidbody2D parentBody))
        {
            _body.linearVelocity = parentBody.linearVelocity;
        }
        StartCoroutine(ILifetime(_launcher._rocket.lifetime));
    }

    void FixedUpdate()
    {
        if (_target != null)
        {
            _direction = (_target.transform.position - transform.position).normalized;
        }
        //transform.rotation = Quaternion.Euler(0f, 0f, ((Player._player.DirectionToAngle(_direction) - transform.rotation.eulerAngles.z) * _rocketStats.rotationSpeed * Time.fixedDeltaTime) + transform.rotation.eulerAngles.z) * transform.rotation;
        transform.rotation = Quaternion.Euler(0f, 0f, Player._player.DirectionToAngle(_direction));
        _body.linearVelocity = Vector2.ClampMagnitude(_body.linearVelocity, _launcher._rocket.maxSpeed);
        _speed = _body.linearVelocity.magnitude;
        _body.AddRelativeForce(Vector2.up * _launcher._rocket.acceleration);
    }

    public IEnumerator ILifetime(float duration)
    {
        yield return new WaitForSeconds(duration);
        Explode();
    }

    public void Explode()
    {
        if (_canExplode)
        {
            _canExplode = false;
            ParticleEffect();
            foreach(var item in _objectsInExplosion)
            {
                if (item.TryGetComponent<EnnemiHealth>(out EnnemiHealth ennemiHealth))
                {
                    ennemiHealth.HealthChange(-_launcher._rocket.damages);
                }
                else if (item.TryGetComponent<Player>(out Player player))
                {
                    player.HealthChange(-_launcher._rocket.damages);
                }
            }
            _healthSystem.HealthChange(-_launcher._rocket.damages);
        }
    }

    public void ParticleEffect()
    {
        GameObject explosion = Instantiate(_explosionParticle);
        explosion.transform.position = transform.position;
        var main = explosion.GetComponent<ParticleSystem>().main;
        main.startSize = new ParticleSystem.MinMaxCurve(_explosionParticle.GetComponent<ParticleSystem>().main.startSize.constantMin * _launcher._rocket.explosionRadius, _explosionParticle.GetComponent<ParticleSystem>().main.startSize.constantMax * _launcher._rocket.explosionRadius);
        main.startSpeed = new ParticleSystem.MinMaxCurve(_explosionParticle.GetComponent<ParticleSystem>().main.startSpeed.constantMin * _launcher._rocket.explosionRadius, _explosionParticle.GetComponent<ParticleSystem>().main.startSpeed.constantMax * _launcher._rocket.explosionRadius);
        var shape = explosion.GetComponent<ParticleSystem>().shape;
        if (transform.localScale.x == transform.localScale.y)
        {
            shape.radius = transform.localScale.x;
            shape.scale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            shape.radius = 1f;
            shape.scale = new Vector3(transform.localScale.x, transform.localScale.y, 1f);
        }
        var emission = explosion.GetComponent<ParticleSystem>().emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(_explosionParticle.GetComponent<ParticleSystem>().emission.rateOverTime.constant * _launcher._rocket.explosionRadius * _launcher._rocket.damages);
    }
    
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_launcher._parents.Contains(collision.gameObject))
        {
            Explode();
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        _objectsInExplosion.Add(collider.gameObject);
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        _objectsInExplosion.Remove(collider.gameObject);
    }
}

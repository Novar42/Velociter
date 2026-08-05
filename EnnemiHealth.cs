using UnityEngine;
using System;

public class EnnemiHealth : MonoBehaviour
{
    public float _health, _maxHealth;
    public bool _canHealthChange = true;
    public GameObject _hitParticle;

    public void Awake()
    {
        _health = _maxHealth;
    }

    public void HealthChange(float value)
    {
        if (_canHealthChange)
        {
            _health += value;
            if (_health <= 0)
            {
                GameManager._gameManager._enemiesInScene.Remove(gameObject);
                if (TryGetComponent<Shooter>(out Shooter shooter))
                {
                    Player._player.DashChange(shooter._difficulty);
                }
                if (TryGetComponent<Missile>(out Missile missile))
                {
                    missile.Explode();
                }
                Destroy(gameObject, 0.01f);

            }
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (_hitParticle != null)
        {
            GameObject particle = Instantiate(_hitParticle);
            particle.transform.position = collision.GetContact(0).point;
            var main = particle.GetComponent<ParticleSystem>().main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(_hitParticle.GetComponent<ParticleSystem>().main.startSpeed.constantMin * collision.GetContact(0).normalImpulse, _hitParticle.GetComponent<ParticleSystem>().main.startSpeed.constantMax * collision.GetContact(0).normalImpulse);
            var emission = particle.GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(_hitParticle.GetComponent<ParticleSystem>().emission.rateOverTime.constant * collision.GetContact(0).normalImpulse);
        }
    }
}

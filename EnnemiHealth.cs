using UnityEngine;
using System;

public class EnnemiHealth : MonoBehaviour
{
    public float _health, _maxHealth;
    public bool _canHealthChange = true;
    public Animator _animator;

    public void Awake()
    {
        _health = _maxHealth;
    }

    public void HealthChange(float value)
    {
        if (_canHealthChange)
        {
            _health += value;
            if (Mathf.Sign(value) == -1 && _animator != null)
            {
                _animator.Play("damageTaken");
            }
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
        float impactPower = collision.GetContact(0).relativeVelocity.magnitude;
        GameObject particle = Instantiate(GameManager._gameManager._hitParticle);
        particle.transform.position = collision.GetContact(0).point;
        var main = particle.GetComponent<ParticleSystem>().main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(GameManager._gameManager._hitParticle.GetComponent<ParticleSystem>().main.startSpeed.constantMin * impactPower, GameManager._gameManager._hitParticle.GetComponent<ParticleSystem>().main.startSpeed.constantMax * impactPower);
        var emission = particle.GetComponent<ParticleSystem>().emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(GameManager._gameManager._hitParticle.GetComponent<ParticleSystem>().emission.rateOverTime.constant * impactPower);
    }
}

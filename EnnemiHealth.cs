using UnityEngine;

public class EnnemiHealth : MonoBehaviour
{
    public float _health, _maxHealth;
    public bool _canHealthChange = true;

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
                Player._player.enemies.Remove(gameObject);
                Destroy(gameObject, 0.01f);
            }
        }
    }
}

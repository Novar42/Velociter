using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour
{
    public LineRenderer _line;
    public LayerMask _shootInteract;
    public GameObject _viseur;
    [Header("Parents")]
    public GameObject _mainParent;
    public List<GameObject> _parents = new List<GameObject>();
    [Header("Weapon States")]
    public CurrentWeapon _currentWeapon;
    public CurrentState _currentWeaponState;
    public Laser _laser;
    public Rocket _rocket;
    public enum CurrentWeapon {Laser = 1, Rocket = 2}
    public enum CurrentState {Working, Cooldown, Disabled}

    void FixedUpdate()
    {
        switch (_currentWeapon)
        {
            case CurrentWeapon.Laser:
                _laser.equiped = true;
                _rocket.equiped = false;
                _currentWeaponState = _laser.currentState;
                break;
            case CurrentWeapon.Rocket:
                _laser.equiped = false;
                _rocket.equiped = true;
                _currentWeaponState = _rocket.currentState;
                break;
        }
    }

    [Serializable]
    public class Laser
    {
        public Weapon script;
        public int damages;
        public float range;
        public float cooldownDuration;
        public bool equiped;
        public GameObject particleEffect;
        public CurrentState currentState;

        public void Shoot(Vector2 target, Color color)
        {
            if (currentState == CurrentState.Working)
            {
                RaycastHit2D hit = AimCheck(target, color);
                switch (hit.transform?.gameObject.layer)
                {
                    case 6:
                        Player._player.HealthChange(-damages);;
                        break;
                    case 7:
                        hit.transform?.gameObject.GetComponent<EnnemiHealth>()?.HealthChange(-damages);
                        break;
                    case 8:
                        if (hit.transform?.gameObject.GetComponent<EnnemiHealth>()?._health <= damages)
                        {
                            hit.transform?.gameObject.GetComponent<Missile>()?.Explode();
                        }
                        hit.transform?.gameObject.GetComponent<EnnemiHealth>()?.HealthChange(-damages);
                        break;
                }
                if (hit.transform != null && particleEffect != null)
                {
                    GameObject particle = Instantiate(particleEffect);
                    particle.transform.position = hit.point;
                    var main = particle.GetComponent<ParticleSystem>().main;
                    main.startSpeed = new ParticleSystem.MinMaxCurve(particleEffect.GetComponent<ParticleSystem>().main.startSpeed.constantMin * this.damages, particleEffect.GetComponent<ParticleSystem>().main.startSpeed.constantMax * this.damages);
                    var emission = particle.GetComponent<ParticleSystem>().emission;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(particleEffect.GetComponent<ParticleSystem>().emission.rateOverTime.constant * this.damages);
                }
                script.StartCoroutine(this.ICooldown(this.cooldownDuration));
            }
        }

        public RaycastHit2D AimCheck(Vector2 target, Color color)
        {
            RaycastHit2D hit = Physics2D.Raycast((Vector2)script.gameObject.transform.position, target, range, script._shootInteract);
            if (currentState == CurrentState.Working)
            {
                script._line.enabled = true;
            }
            script._line.SetPosition(0, script.gameObject.transform.position);
            script._line.material.SetColor("_Color", color);
            script._line.material.SetColor("_EmissionColor", color);
            if (hit)
            {
                script._line.SetPosition(1, hit.point);
            }
            else
            {
                script._line.SetPosition(1, ((Vector3)target - script.gameObject.transform.position).normalized * range + script.gameObject.transform.position);
            } 
            return hit;
        }

        public IEnumerator ICooldown(float duration)
        {
            currentState = CurrentState.Cooldown;
            script._line.enabled = false;
            yield return new WaitForSeconds(duration);
            currentState = CurrentState.Working;
        }
    }

    [Serializable]
    public class Rocket
    {
        public Weapon script;
        public GameObject prefabMissile;
        public int damages;
        public float lifetime, acceleration, maxSpeed, rotationSpeed, explosionRadius, cooldownDuration, precision;
        public bool showViseur, equiped;
        public CurrentState currentState;

        public void Shoot(Vector2 pos, Color color)
        {
            if (currentState == CurrentState.Working)
            {
                GameObject missile = Instantiate(prefabMissile);
                missile.transform.position = script.gameObject.transform.TransformPoint(Vector3.up);
                missile.transform.rotation = script.gameObject.transform.rotation;
                missile.GetComponent<Missile>()._launcher = script;
                missile.GetComponent<Missile>()._target = AimCheck(pos, color);
                script.StartCoroutine(this.ICooldown(this.cooldownDuration));
            }
        }

        public void Shoot(float angle, Color color)
        {
            if (GameManager._gameManager._enemiesInScene.Count > 0 && currentState == CurrentState.Working)
            {
                GameObject missile = Instantiate(prefabMissile);
                missile.transform.position = script.gameObject.transform.TransformPoint(Vector3.up);
                missile.transform.rotation = script.gameObject.transform.rotation;
                missile.GetComponent<Missile>()._launcher = script;
                missile.GetComponent<Missile>()._target = AimCheck(angle, color);
                script.StartCoroutine(this.ICooldown(this.cooldownDuration));
            }
        }

        public void Shoot(GameObject target)
        {
            if (currentState == CurrentState.Working)
            {
                GameObject missile = Instantiate(prefabMissile);
                missile.transform.position = script.gameObject.transform.TransformPoint(Vector3.up);
                missile.transform.rotation = script.gameObject.transform.rotation;
                missile.GetComponent<Missile>()._launcher = script;
                missile.GetComponent<Missile>()._target = target;
                script.StartCoroutine(this.ICooldown(this.cooldownDuration));
            }
        }

        public GameObject AimCheck(Vector2 pos, Color color)
        {
            if (GameManager._gameManager._enemiesInScene.Count > 0 && currentState == CurrentState.Working)
            {
                float distance = Vector2.Distance(pos, (Vector2)GameManager._gameManager._enemiesInScene[0].transform.position);
                GameObject closestEnemy = null;
                foreach (var item in GameManager._gameManager._enemiesInScene)
                {
                    if (Vector2.Distance(pos, (Vector2)item.transform.position) < distance || closestEnemy == null)
                    {
                        distance = Vector2.Distance(pos, (Vector2)item.transform.position);
                        closestEnemy = item;
                    }
                }
                script._viseur.GetComponent<SpriteRenderer>().color = color;
                script._viseur.SetActive(this.showViseur);
                script._viseur.transform.position = closestEnemy.transform.position;
                return closestEnemy;
            }
            else
            {
                script._viseur.SetActive(false);
                return null;
            }
        }

        public GameObject AimCheck(float angle, Color color)
        {
            if (GameManager._gameManager._enemiesInScene.Count > 0 && currentState == CurrentState.Working)
            {
                float distance = Mathf.Abs(angle - Vector2.Angle(Vector2.right, (Vector2)GameManager._gameManager._enemiesInScene[0].transform.position - (Vector2)script.gameObject.transform.position));
                GameObject closestEnemy = null;
                foreach (var item in GameManager._gameManager._enemiesInScene)
                {
                    if (Mathf.Abs(angle - Vector2.Angle(Vector2.right, (Vector2)item.transform.position - (Vector2)script.gameObject.transform.position)) < distance || closestEnemy == null)
                    {
                        distance = Mathf.Abs(angle - Vector2.Angle(Vector2.right, (Vector2)item.transform.position - (Vector2)script.gameObject.transform.position));
                        closestEnemy = item;
                    }
                }
                script._viseur.GetComponent<SpriteRenderer>().color = color;
                script._viseur.SetActive(this.showViseur);
                script._viseur.transform.position = closestEnemy.transform.position;
                return closestEnemy;
            }
            else
            {
                script._viseur.SetActive(false);
                return null;
            }
        }

        public IEnumerator ICooldown(float duration)
        {
            currentState = CurrentState.Cooldown;
            if (script._viseur != null)
            {
                script._viseur.SetActive(false);
            }
            yield return new WaitForSeconds(duration);
            currentState = CurrentState.Working;
        }
    }
}

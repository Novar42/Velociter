using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("General")]
    public static GameManager _gameManager;
    public enum GameState {InMenu = 0, InWaveGame = 1, InPause = 2}
    public GameState _gameState;
    public List<GameObject> _enemiesPrefabs = new List<GameObject>();
    public List<GameObject> _enemiesInScene = new List<GameObject>();

    [Header("Menu")]
    public GameObject firstSelected;

    [Header("WaveMode")]
    public WaveManager _waveManager;
    public bool inWave = true;

    [Header("Particles")]
    public GameObject _explosionParticle;
    public GameObject _hitParticle;
    public GameObject _dashParticle;

    [Serializable]
    public class WaveManager
    {
        public GameManager script;
        public int _waveNumber;
        public float _waveChangeDuration;
        public List<GameObject> _enemiesToSpawn = new List<GameObject>();

        public IEnumerator IWaveChange()
        {
            script.inWave = false;
            int enemyNumber = Mathf.RoundToInt(_waveNumber / 2);
            _waveNumber++;
            _enemiesToSpawn.Clear();
            for (int restDifficulty = _waveNumber; restDifficulty > 0;)
            {
                GameObject enemyToAdd = script._enemiesPrefabs[0];
                foreach(var item in script._enemiesPrefabs)
                {
                    if (item.GetComponent<Shooter>()._difficulty <= restDifficulty && item.GetComponent<Shooter>()._difficulty > enemyToAdd.GetComponent<Shooter>()._difficulty)
                    {
                        enemyToAdd = item;
                    }
                }
                restDifficulty -= enemyToAdd.GetComponent<Shooter>()._difficulty;
                _enemiesToSpawn.Add(enemyToAdd);
            }
            yield return new WaitForSeconds(_waveChangeDuration);
            Player._player.HealthChange(1);
            Player._player._waveUI.text = "Wave " + _waveNumber;
            foreach(var item in _enemiesToSpawn)
            {
                SpawnEnemy(item, 7.5f);
            }
            script.inWave = true;
        }

        public void SpawnEnemy(GameObject enemy, float distance = 5f)
        {
            for (int i = 1; i < 100; i++)
            {
                Vector2 spawnPos = new Vector2(Random.Range(-i, i), Random.Range(-i, i));
                bool canSpawn = true;
                if (Vector2.Distance(spawnPos, Player._player.gameObject.transform.position) < distance)
                {
                    canSpawn = false;
                }
                if (script._enemiesInScene.Count > 0)
                {
                    foreach (var item in script._enemiesInScene)
                    {
                        if (Vector2.Distance(spawnPos, item.transform.position) < distance)
                        {
                            canSpawn = false;
                            break;
                        }
                    }
                }
                if (canSpawn == true)
                {
                    GameObject enemyInstance = Instantiate(enemy);
                    // enemyInstance.transform.parent = script._enemyGroup.transform;
                    enemyInstance.transform.position = spawnPos;
                    script._enemiesInScene.Add(enemyInstance);
                    break;
                }
            }
        }

        public WaveManager(GameManager script, int waveNumber, float waveChangeDuration)
        {
            this.script = script;
            this._waveNumber = waveNumber;
            this._waveChangeDuration = waveChangeDuration;
        }
    }

    public void Awake()
    {
        if (_gameManager == null)
        {
            _gameManager = this;
            _gameState = GameState.InMenu;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(_gameManager.gameObject);
            _gameManager = this;
            _gameState = GameState.InMenu;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void FixedUpdate()
    {
        if (_gameState == GameState.InWaveGame && _enemiesInScene.Count <= 0 && inWave)
        {
            StartCoroutine(_waveManager.IWaveChange());
        }
    }

    public void CameraShake(int duration, float magnitude)
    {
        IEnumerator ICameraShake()
        {
            for (int i = 0; i < duration; i++)
            {
                Camera.main.transform.position += (Vector3)Random.insideUnitCircle * magnitude;
                yield return new WaitForSeconds(0.05f);
            }
        }
        StartCoroutine(ICameraShake());
    }

    public bool IsOnUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void OnDisable()
    {
        _gameManager = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Menu":
                _gameState = GameState.InMenu;
                if (firstSelected != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstSelected);
                }
                break;
            case "WaveGame":
                _gameState = GameState.InWaveGame;
                _waveManager = new WaveManager(this, _waveManager._waveNumber, _waveManager._waveChangeDuration);
                StartCoroutine(_waveManager.IWaveChange());
                break;
        }
    }

    public void GameStateChange(GameState state)
    {
        _gameState = state;
    }

    public void GameStateChange(int state)
    {
        _gameState = (GameState)state;
    }

    public void SceneChange(string scene)
    {
        SceneManager.LoadScene(scene);
        StopAllCoroutines();
        _enemiesInScene.Clear();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

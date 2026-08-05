using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager _gameManager;
    public enum GameState {InMenu = 0, InWaveGame = 1, InPause = 2}
    public GameState _gameState;
    public List<GameObject> _enemiesPrefabs = new List<GameObject>();
    public List<GameObject> _enemiesInScene = new List<GameObject>();
    public WaveManager _waveManager;
    public bool inWave = true;

    [Serializable]
    public class WaveManager
    {
        public GameManager script;
        public int _waveNumber = 0;
        public float _waveChangeDuration = 2f;
        public List<GameObject> _enemiesToSpawn = new List<GameObject>();

        public IEnumerator IWaveChange()
        {
            script.inWave = false;
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
                SpawnEnemy(item);
            }
            script.inWave = true;
        }

        public void SpawnEnemy(GameObject enemy)
        {
            for (int i = 1; i < 100; i++)
            {
                Vector2 spawnPos = new Vector2(Random.Range(-i, i), Random.Range(-i, i));
                bool canSpawn = true;
                if (Vector2.Distance(spawnPos, Player._player.gameObject.transform.position) < 5f)
                {
                    canSpawn = false;
                }
                if (script._enemiesInScene.Count > 0)
                {
                    foreach (var item in script._enemiesInScene)
                    {
                        if (Vector2.Distance(spawnPos, item.transform.position) < 5f)
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

        public WaveManager(GameManager script)
        {
            this.script = script;
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
                break;
            case "WaveGame":
                _gameState = GameState.InWaveGame;
                _waveManager = new WaveManager(this);
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

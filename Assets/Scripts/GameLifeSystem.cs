using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLifeSystem : MonoBehaviour
{
    public static GameLifeSystem instance;
    public static Action OnLifeLost;
    public static Action OnGameOver;

    [SerializeField] private int _totalLives = 5;
    [SerializeField] private GameObject _targetToDestroy;
    [SerializeField] private GameObject lifeSpritesParent;
    [SerializeField] private List<GameObject> _lifeSpritesGO = new List<GameObject>();

    public GameObject LifeSpritesParent => lifeSpritesParent;

    public int TotalLives => _totalLives;
    public int CurrentLives => _currentLives;

    private int _currentLives;

    private void Awake()
    {
        _currentLives = _totalLives;
        DontDestroyOnLoad(gameObject);

        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);


        for (int i = 0; i < _totalLives; i++)
        {
            _lifeSpritesGO[i].SetActive(true);
        }


        OnLifeLost += OnGameLifeLost;
    }

    private void OnDestroy()
    {
        OnLifeLost -= OnGameLifeLost;
    }


    public void OnGameLifeLost()
    {
        _currentLives--;
        _lifeSpritesGO[_currentLives].SetActive(false);

        if (_currentLives <= 0)
        {
            OnGameOver?.Invoke();
        }

    }

    public void BackToScene(string sceneName)
    {
        StartCoroutine(DelaySceneLoad(sceneName));
    }

    IEnumerator DelaySceneLoad(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
        Destroy(_targetToDestroy);
    }

}

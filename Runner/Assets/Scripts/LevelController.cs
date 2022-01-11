using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public static LevelController Current; //Di�er s�n�flar�n bu objeye eri�mesi i�in
    public bool gameActive = false; //levelin aktif olup olmad��� s�yler

    public GameObject startMenu, gameMenu, gameOverMenu, finishMenu; //men�leri tutar
    public Text scoreText, finishScoreText, currentLevelText, nextLevelText, startingMenuMoneyText, gameOverMenuMoneyText, finishGameMenuMoneyText; //Oyun ekran�ndaki text metinlerini tutar
    public Slider levelProgressBar; //Karakterin oyun i�indeki ilerlemesini tutar
    public float maxDistance; //Karakterin biti� �izgisine olan uzakl���n� tutar
    public GameObject finishLine; //Biti� �izgisini tutar
    public AudioSource gameMusicAudioSource;
    public AudioClip victoryAudioClip, gameOverAudioClip; //Kazanma ve kaybetme sesleri
    public DailyReward dailyReward;

    int currentLevel; //G�ncel leveli tutar
    int score; //Skoru tutar

    void Start()
    {
        Current = this; //Curent'i levelconrollerin kendisine e�itlenir
        currentLevel = PlayerPrefs.GetInt("currentLevel"); //Oyuncunun ka��nc� levelde kald���n� tutar
        PlayerController.Current = GameObject.FindObjectOfType<PlayerController>();
        GameObject.FindObjectOfType<MarketController>().InitializeMarketController();
        dailyReward.InitializeDailyReward();
        currentLevelText.text = (currentLevel + 1).ToString(); //Oyun ekran�ndaki leveli textini karakterin leveliyle de�i�tirir.
        nextLevelText.text = (currentLevel + 2).ToString(); //Sonraki b�l�m textini bulundu�u level textine g�re ayarlar
        UpdateMoneyTexts();
        gameMusicAudioSource = Camera.main.GetComponent<AudioSource>(); //Ana kameaya ula��p kamera �zerindeki m�zi�i �almaya ba�lar
    }

    void Update()
    {
        if (gameActive)
        {
            PlayerController player = PlayerController.Current;
            float distance = finishLine.transform.position.z - PlayerController.Current.transform.position.z;
            levelProgressBar.value = 1 - (distance / maxDistance);
        }

    }

    public void StartLevel()
    {
        maxDistance = finishLine.transform.position.z - PlayerController.Current.transform.position.z; //Karakte ile biti� �izgisi aras�ndaki mesafeyi bulur

        PlayerController.Current.ChangeSpeed(PlayerController.Current.runningSpeed); //playercontorllerin �u anki objesine eri� ve h�z�n� ayn� objenin maksimum h�z� kadar artt�r
        startMenu.SetActive(false);
        gameMenu.SetActive(true);
        PlayerController.Current.animator.SetBool("running", true); //Ekrana bas�ld���nda karakter ko�maya ba�lar.
        gameActive = true;
    }

    public void RestartLevel()
    {
        LevelLoader.Current.ChangeLevel(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevet()
    {
        LevelLoader.Current.ChangeLevel("Level " + (currentLevel + 1));
    }

    public void GameOver()
    {
        UpdateMoneyTexts();
        gameMusicAudioSource.Stop();
        gameMusicAudioSource.PlayOneShot(gameOverAudioClip);
        gameMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        gameActive = false;
    }

    public void FinishGame()
    {
        GiveMoneyToPlayer(score);
        gameMusicAudioSource.Stop();
        gameMusicAudioSource.PlayOneShot(victoryAudioClip);
        PlayerPrefs.SetInt("currentLevel", currentLevel + 1);
        finishScoreText.text = score.ToString();
        gameMenu.SetActive(false);
        finishMenu.SetActive(true);
        gameActive = false;
    }

    public void ChangeScore(int increment)
    {
        score += increment;
        scoreText.text = score.ToString();
    }

    public void UpdateMoneyTexts() //Oyuncunun paras�n� g�nceller
    {
        int money = PlayerPrefs.GetInt("money");
        startingMenuMoneyText.text = money.ToString();
        gameOverMenuMoneyText.text = money.ToString();
        finishGameMenuMoneyText.text = money.ToString();
    }

    public void GiveMoneyToPlayer(int increment)
    {
        int money = PlayerPrefs.GetInt("money");
        money = Mathf.Max(0, money + increment); //Money + increment toplam� s�f�rkan k���k bir de�er olursa para 0 olacak b�y�k bir de�er olursa diek money d�nd�recek
        PlayerPrefs.SetInt("money", money);
        UpdateMoneyTexts();
    }
}

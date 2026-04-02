using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LunaManager : MonoBehaviour
{
    public static LunaManager ins;
    public int countDrop = 0;

    [LunaPlaygroundField("Số lần bắn ra Store")] public int countDropFinal;
    [LunaPlaygroundField("Time")] public int timeEndCreative = 30;
    [LunaPlaygroundField("Lightning")] public float Lightning = 5;
    [LunaPlaygroundField("Color Light")] public Color colorLight = Color.black;
    [LunaPlaygroundAsset("Music")] public AudioClip bgMusic;

    [Header("Card Manager")]
    public MultiCardStackManager cardStackManager;

    [Header("Card 1")]
    [LunaPlaygroundAsset("Card 1 Photo")] public Texture2D card1Photo;
    [LunaPlaygroundField("Card 1 Name")] public string card1Name = "Anna";
    [LunaPlaygroundField("Card 1 Age/Location")] public string card1AgeLocation = "23 / Hà Nội";
    [LunaPlaygroundField("Card 1 Distance")] public string card1Distance = "2 km away";
    [LunaPlaygroundField("Card 1 Tag")] public string card1Tag = "Coffee";

    [Header("Card 2")]
    [LunaPlaygroundAsset("Card 2 Photo")] public Texture2D card2Photo;
    [LunaPlaygroundField("Card 2 Name")] public string card2Name = "Linh";
    [LunaPlaygroundField("Card 2 Age/Location")] public string card2AgeLocation = "21 / Đà Nẵng";
    [LunaPlaygroundField("Card 2 Distance")] public string card2Distance = "5 km away";
    [LunaPlaygroundField("Card 2 Tag")] public string card2Tag = "Travel";

    [Header("Card 3")]
    [LunaPlaygroundAsset("Card 3 Photo")] public Texture2D card3Photo;
    [LunaPlaygroundField("Card 3 Name")] public string card3Name = "Mia";
    [LunaPlaygroundField("Card 3 Age/Location")] public string card3AgeLocation = "24 / TP.HCM";
    [LunaPlaygroundField("Card 3 Distance")] public string card3Distance = "1 km away";
    [LunaPlaygroundField("Card 3 Tag")] public string card3Tag = "Music";

    public Light directionalLight;
    public bool isCretivePause;
    [LunaPlaygroundAsset("End Card Photo")] public Texture2D endCardPhoto;
    public Image endCardImage;
    [LunaPlaygroundAsset("Logo Top")] public Texture2D logoTopTexture;
        public Image logoTopImage;
        [LunaPlaygroundAsset("Logo Left")] public Texture2D logoLeftTexture;
            public Image logoLeftImage;
    public Button[] lstBtnInstall;
    public GameObject EndCard, EndCardEmpty;
    public GameObject WinCard;

    private void Awake()
    {
        ins = this;
    }

    void Start()
    {
        directionalLight.intensity = Lightning;
        directionalLight.color = colorLight;

        ApplyCardDataToStack();

        Luna.Unity.LifeCycle.OnPause += PauseGameplay;
        Luna.Unity.LifeCycle.OnResume += ResumeGameplay;

        foreach (var button in lstBtnInstall)
        {
            button.onClick.AddListener(OnClickEndCard);
        }

        EndCard.SetActive(false);
        EndCardEmpty.SetActive(false);
        WinCard.SetActive(false);
        endCardImage.sprite = CreateSprite(endCardPhoto);
        logoTopImage.sprite = CreateSprite(logoTopTexture);
        logoLeftImage.sprite = CreateSprite(logoLeftTexture);
        Invoke(nameof(ShowEndCardEmpty), timeEndCreative);
    }

    private void ApplyCardDataToStack()
    {
        if (cardStackManager == null) return;

        cardStackManager.allCards = new List<CardData>
        {
            new CardData
            {
                photo = CreateSprite(card1Photo),
                userName = card1Name,
                ageLocation = card1AgeLocation,
                distance = card1Distance,
                tag = card1Tag
            },
            new CardData
            {
                photo = CreateSprite(card2Photo),
                userName = card2Name,
                ageLocation = card2AgeLocation,
                distance = card2Distance,
                tag = card2Tag
            },
            new CardData
            {
                photo = CreateSprite(card3Photo),
                userName = card3Name,
                ageLocation = card3AgeLocation,
                distance = card3Distance,
                tag = card3Tag
            }
        };

        cardStackManager.InitStack();
    }

    private Sprite CreateSprite(Texture2D texture)
    {
        if (texture == null) return null;

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    public void CheckClickShowEndCard()
    {
        countDrop++;
        if (countDrop >= countDropFinal && isCretivePause == false)
        {
            ShowEndCardEmpty();
        }
    }

    public void PauseGameplay()
    {
        Debug.Log("Pause game");
        Time.timeScale = 0;
    }

    public void ResumeGameplay()
    {
        Debug.Log("Load game");
        Time.timeScale = 1;
    }

    public void ShowEndCard()
    {
        if (isCretivePause) return;
        isCretivePause = true;
        AudioManager.ins.StopMusic();
        EndCard.SetActive(true);
        Debug.Log("Show end card");
        Luna.Unity.LifeCycle.GameEnded();
    }

    public void ShowEndCardEmpty()
    {
        if (isCretivePause) return;
        isCretivePause = true;
        EndCardEmpty.SetActive(true);
        Debug.Log("ShowEndCardEmpty");
        Luna.Unity.LifeCycle.GameEnded();
    }

    public void ShowWinCard()
    {
        if (isCretivePause) return;
        isCretivePause = true;
        WinCard.SetActive(true);
        Debug.Log("Show win card");
        Luna.Unity.LifeCycle.GameEnded();
    }

    public void OnClickEndCard()
    {
        Debug.Log("Click end card");
        Luna.Unity.Playable.InstallFullGame();
    }
}
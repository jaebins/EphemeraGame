
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum JsonPath
{
    Games,
    Players,
    Enemys,
    Skills
}

[System.Serializable]
public class Games
{
    public int playerIndex;
    public int health;
    public int money;
    public int level;
    public int skillPoint;
    public float exp;
    public float[] maxExps;
    public List<int> myPlayer;
    public float backSound;
    public float effectSound;
}

[System.Serializable]
public class PlayerInfors
{
    public string name;
    public int price;
    public float attackRangeX;
    public float attackRangeY;
    public float attack;
    public float attackSpeed;
    public float speed;
    public float health;
    public float exp;
    public float money;
    public float score;
}

[System.Serializable]
public class EnemyInfors
{
    public float health;
    public int money;
}

[System.Serializable]
public class Skills
{
    public string name;
    public string priceType;
    public int price;
}

[System.Serializable]
public class LoadJsonResult
{
    public PlayerInfors[] playerInfors;
    public Games games;
    public EnemyInfors[] enemyInfors;
    public Skills[] skills;
}

public class GameManager : MonoBehaviour
{
    // 외부 스크립트
    public ObjectManager objectManager;
    public Player player;
    public AudioSource backAudioSource;
    
    // 내부 스크립트
    public AudioSource audioSource;

    // 리소스
    public Games games; // 게임 데이터
    public PlayerInfors[] playerInfors;
    public EnemyInfors[] enemyInfors;
    public Skills[] skills;
    public AnimatorOverrideController[] player_animatorC; // 캐릭터들 애니메이션
    public Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip> ();

    // UI 설정
    public GameObject canvas;
    GameObject panel_infor;
    public GameObject[] panel_inforItems = new GameObject[5];
    public GameObject panel_die;

    public Dictionary<string, Text> text_stats = new Dictionary<string, Text>();
    public Scrollbar expBar;
    public TMP_Text text_expBar;
    public TMP_Text text_skillPoint;
    public TMP_Text text_money;
    public Scrollbar healthBar;
    public TMP_Text text_healthBar;

    public GameObject[] change_playerSp;
    public GameObject[,] shop_playerSp;
    public Scrollbar backSoundScroll;
    public Scrollbar effectSoundScroll;

    // 게임 설정
    public GameObject nowOnInfor;

    // 게임 진행 관련 변수
    public Stack<GameObject> now_Enemys = new Stack<GameObject>();
    public float spawnEnemyDelay;
    public float flowSpawnEnemyDelay;

    // 시간이 지날수록 몬스터 체력 높이기
    void Awake()
    {
        LoadGameResource();
        LoadJsonData();
        GetUIItems();

        // 해상도 대응
        Vector3 screen = canvas.GetComponent<RectTransform>().sizeDelta;
        panel_infor.GetComponent<RectTransform>().sizeDelta = new Vector2(0, screen.y / 3f);
        healthBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, panel_infor.GetComponent<RectTransform>().sizeDelta.y + 100 + 30);
        panel_die.GetComponent<RectTransform>().sizeDelta = new Vector2(0, screen.y - (screen.y / 3f) - 100);
    }

    void Start()
    {
        SetGame();
    }

    private void Update()
    {
        EnemySpawn();
    }

    void EnemySpawn()
    {
        flowSpawnEnemyDelay += Time.deltaTime;
        if (flowSpawnEnemyDelay > spawnEnemyDelay)
        {
            GameObject enemy = objectManager.getObj("enemy_0");
            enemy.transform.position = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-4.5f, 4.5f), 0);
            now_Enemys.Push(enemy);

            flowSpawnEnemyDelay = 0;
        }
    }

    void LoadJsonData()
    {
        // 열거형 때문에 순서대로 설정해야함
        int jsonCnt = Resources.LoadAll<TextAsset>("Datas").Length;
        string[] texts = new string[jsonCnt];
        string[] jsonName = getCombineJsonPath();

        // 처음으로 키는거라면 리소스 폴더안에 기본 데이터들을 불러옴
        if (!File.Exists(jsonName[0]))
        {
            int[] jsons = (int[])Enum.GetValues(typeof(JsonPath));
            foreach(int i in jsons)
            {
                TextAsset json = Resources.Load<TextAsset>($"Datas/{Enum.GetName(typeof(JsonPath), i)}");
                texts[i] = json.text;
            }
        }
        // 처음으로 키는게 아니라면 로컬 디렉토리에서 데이터들을 불러옴
        else
        {
            for (int i = 0; i < jsonCnt; i++) texts[i] = File.ReadAllText(jsonName[i]);
        }
        games = JsonUtility.FromJson<LoadJsonResult>(texts[0]).games;

        playerInfors = JsonUtility.FromJson<LoadJsonResult>(texts[1]).playerInfors;
        player.nowPlayerInfors = playerInfors[games.playerIndex];

        enemyInfors = JsonUtility.FromJson<LoadJsonResult>(texts[2]).enemyInfors;

        skills = JsonUtility.FromJson<LoadJsonResult>(texts[3]).skills;
    }

    void LoadGameResource()
    {
        flowSpawnEnemyDelay = spawnEnemyDelay;

        objectManager = GameObject.Find("ObjectManager").GetComponent<ObjectManager>();
        player = GameObject.Find("Player").GetComponent<Player>();
        audioSource = GetComponent<AudioSource>();
        backAudioSource = GameObject.Find("MusicManager").GetComponent<AudioSource>();
        player_animatorC = Resources.LoadAll<AnimatorOverrideController>("Animations/Player");

        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("Sounds");
        foreach(AudioClip clip in audioClips)
            sounds.Add(clip.name, clip);
    }

    void GetUIItems()
    {
        canvas = GameObject.Find("Canvas");
        panel_die = canvas.transform.Find("DiePanel").gameObject;
        // 메뉴들 가져옴
        panel_infor = canvas.transform.Find("Infor").gameObject;
        for (int i = 0; i < panel_infor.transform.childCount; i++)
        {
            panel_inforItems[i] = panel_infor.transform.GetChild(i).gameObject;
        }

        // 강화 창 UI 가져옴
        GameObject panel_upgradeItems = panel_inforItems[0].transform.Find("Viewport").Find("Content").gameObject;
        for (int i = 0; i < panel_upgradeItems.transform.childCount; i++)
        {
            // 강화 창 텍스트 가져오고 리스트에 넣음
            Text text = panel_upgradeItems.transform.GetChild(i).GetChild(2).GetComponent<Text>();
            string type = panel_upgradeItems.transform.GetChild(i).GetChild(3).name;

            text_stats.Add(type.Substring(8, type.Length - 8), text);
        }

        // 캐릭터 창 UI 가져옴
        change_playerSp = new GameObject[playerInfors.Length];
        shop_playerSp = new GameObject[playerInfors.Length, 2];
        GameObject panel_changeItems = panel_inforItems[1].transform.Find("Viewport").Find("Content").gameObject;
        for (int i = 0; i < panel_changeItems.transform.childCount; i++)
        {
            // 가로 세로 구분을 위해 2중 반복문을 돌림
            GameObject hInfor = panel_changeItems.transform.GetChild(i).gameObject;
            for (int j = 0; j < hInfor.transform.childCount; j++)
            {
                // 내 데이터에 이 플레이어가 구매되어있다면
                GameObject player = hInfor.transform.GetChild(j).gameObject;
                change_playerSp[(panel_changeItems.transform.childCount * i) + j] = player;

                if (Array.FindIndex(games.myPlayer.ToArray(), x => x == Int32.Parse(player.name[^1].ToString())) != -1)
                {
                    // 이미 구매된 플레이어 활성화
                    player.SetActive(true);
                }
            }
        }

        GameObject panel_shopItems = panel_inforItems[3].transform.Find("Viewport").Find("Content").gameObject;
        for (int i = 0; i < panel_shopItems.transform.childCount; i++)
        {
            // 가로 세로 구분을 위해 2중 반복문을 돌림
            GameObject hInfor = panel_shopItems.transform.GetChild(i).gameObject;
            for (int j = 0; j < hInfor.transform.childCount; j++)
            {
                // 내 데이터에 이 플레이어가 구매되어있다면
                GameObject player = hInfor.transform.GetChild(j).gameObject;
                GameObject icon = player.transform.GetChild(0).gameObject;
                GameObject text = player.transform.GetChild(1).gameObject;

                shop_playerSp[(panel_shopItems.transform.childCount * i) + j, 0] = icon;
                shop_playerSp[(panel_shopItems.transform.childCount * i) + j, 1] = text;

                if (Array.FindIndex(games.myPlayer.ToArray(), x => x == Int32.Parse(player.name[^1].ToString())) != -1)
                {
                    // 이미 구매된 플레이어는 투명 이미지로 돌리고, 텍스트를 없앰
                    icon.GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);
                    text.SetActive(false);
                }
            }
        }

        backSoundScroll = panel_inforItems[4].transform.Find("Horizon").Find("OptionBackSoundPanel").Find("SettingBackSoundScroll").GetComponent<Scrollbar>();
        effectSoundScroll = panel_inforItems[4].transform.Find("Horizon").Find("OptionEffectSoundPanel").Find("SettingEffectSoundScroll").GetComponent<Scrollbar>();

        expBar = canvas.transform.Find("Expbar").GetComponent<Scrollbar>();
        text_expBar = expBar.transform.Find("ExpbarText").GetComponent<TMP_Text>();
        text_skillPoint = expBar.transform.Find("SkillPointText").GetComponent<TMP_Text>();
        text_money = expBar.transform.Find("MoneyText").GetComponent<TMP_Text>();

        healthBar = canvas.transform.Find("Healthbar").GetComponent<Scrollbar>();
        text_healthBar = healthBar.transform.Find("HealthbarText").GetComponent<TMP_Text>();

        nowOnInfor = panel_inforItems[0].gameObject; // 첫 메뉴창
    }

    void SetGame()
    {
        Time.timeScale = 1;

        // 초기 사운드 설정
        backAudioSource.volume = games.backSound;
        audioSource.volume = games.effectSound;
        player.audioSource.volume = games.effectSound;

        backSoundScroll.value = games.backSound;
        effectSoundScroll.value = games.effectSound;
    }

    public void ChangeMenu(GameObject menu)
    {
        if (nowOnInfor == null || !menu.name.Equals(nowOnInfor.name))
        {
            AudioClip audioClip = getDicValue<string, AudioClip>("click", sounds);
            SoundPlay(audioSource, audioClip, false);

            menu.SetActive(true);
            if (nowOnInfor != null) nowOnInfor.SetActive(false);
            nowOnInfor = menu;
        }
    }

    public void SkillLevelUp(string levelUpType)
    {
        // 스킬포인트가 있을때만 스텟 업그레이드
        if (games.skillPoint > 0)
        {
            AudioClip audioClip = getDicValue<string, AudioClip>("click", sounds);
            SoundPlay(audioSource, audioClip, false);

            PlayerInfors p = player.nowPlayerInfors;

            switch (levelUpType)
            {
                case "attack":
                    p.attack = player.UpgradeStat(levelUpType, p.attack, 0.2f);
                    break;
                case "attackSpeed":
                    p.attackSpeed = player.UpgradeStat(levelUpType, p.attackSpeed, 0.2f);
                    break;
                case "speed":
                    p.speed = player.UpgradeStat(levelUpType, p.speed, 0.2f);
                    break;
                case "health":
                    p.health = player.UpgradeStat(levelUpType, p.health, 3f);
                    break;
                case "exp":
                    p.exp = player.UpgradeStat(levelUpType, p.exp, 0.2f);
                    break;
                case "money":
                    p.money = player.UpgradeStat(levelUpType, p.money, 0.2f);
                    break;
                case "score":
                    p.score = player.UpgradeStat(levelUpType, p.score, 1f);
                    break;
            }

            text_skillPoint.text = $"POINT : {--games.skillPoint}";
            player.SetStats();
        }
    }

    public void ChangePlayer(int playerIndex)
    {
        AudioClip audioClip = getDicValue<string, AudioClip>("click", sounds);
        SoundPlay(audioSource, audioClip, false);

        player.ChangePlayer(playerIndex);
    }

    public void UseSkill(Skills nowSkill)
    {
        if (games.money >= nowSkill.price && nowSkill.priceType.Equals("money"))
            player.SetMoeny(-nowSkill.price);
        else if (games.skillPoint >= nowSkill.price && nowSkill.priceType.Equals("skillPoint"))
        {
            games.skillPoint -= nowSkill.price;
            player.SetExp(0);
        }
        else return;

        AudioClip audioClip = getDicValue<string, AudioClip>("click", sounds);
        SoundPlay(audioSource, audioClip, false);

        switch (nowSkill.name)
        {
            case "heal":
                player.SkillHeal();
                break;
            case "getExp":
                player.SkillGetExp();
                break;
        }
    }

    public void BuyPlayer(int buyType, int needMoney)
    {
        // 없는 캐릭터만 구매 가능
        if (games.money >= needMoney && Array.FindIndex(games.myPlayer.ToArray(), x => x == buyType) == -1)
        {
            AudioClip audioClip = getDicValue<string, AudioClip>("buy", sounds);
            SoundPlay(audioSource, audioClip, false);

            games.myPlayer.Add(buyType);

            change_playerSp[buyType].SetActive(true);

            shop_playerSp[buyType, 0].GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);
            shop_playerSp[buyType, 1].SetActive(false);

            player.SetMoeny(-needMoney);
        }
    }

    public void RetryGame()
    {
        AudioClip audioClip = getDicValue<string, AudioClip>("click", sounds);
        SoundPlay(audioSource, audioClip, false);

        // class -> json으로 변환 (배열을 가진 경우 LoadJsonResult() 를 통해 저장)
        string[] texts = new string[4];

        GamesJson gamesInforsJson = new GamesJson();
        gamesInforsJson.games = games;
        texts[0] = JsonUtility.ToJson(gamesInforsJson);

        PlayerInforsJson playerInforsJson = new PlayerInforsJson();
        playerInforsJson.playerInfors = playerInfors;
        texts[1] = JsonUtility.ToJson(playerInforsJson);

        EnemyInforsJson enemyInforsJson = new EnemyInforsJson();
        enemyInforsJson.enemyInfors = enemyInfors;
        texts[2] = JsonUtility.ToJson(enemyInforsJson);

        SkillsJson skillsJson = new SkillsJson();
        skillsJson.skills = skills;
        texts[3] = JsonUtility.ToJson(skillsJson);

        // json으로 파일 저장
        string[] jsonPath = getCombineJsonPath();
        for (int i = 0; i < jsonPath.Length; i++)
        {
            File.Delete(jsonPath[i]);
            using (FileStream fs = File.Create(jsonPath[i]))
            {
                fs.Dispose();
                File.WriteAllText(jsonPath[i], texts[i]);
            }
        }

        SceneManager.LoadScene(1);
    }

    public void ResetGameData()
    {
        string[] jsonPath = getCombineJsonPath();
        for (int i = 0; i < jsonPath.Length; i++)
        {
            File.Delete(jsonPath[i]);
        }
        SceneManager.LoadScene(0);
    }

    public void SoundPlay(AudioSource audio, AudioClip audioClip, bool isLoop)
    {
        audio.clip = audioClip;
        audio.Play();
        audio.loop = isLoop;
    }

    public void SetSoundEmploy(string type)
    {
        switch(type)
        {
            case "SettingBackSoundScroll":
                backAudioSource.volume = backSoundScroll.value;
                games.backSound = backSoundScroll.value;
                break;
            case "SettingEffectSoundScroll":
                audioSource.volume = effectSoundScroll.value;
                player.audioSource.volume = effectSoundScroll.value;
                games.effectSound = effectSoundScroll.value;
                break;
        }
    }

    public static bool isEndAni(Animator animator, string name, float time)
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= time && animator.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    public static IEnumerator OffDamageText(GameObject damageText)
    {
        yield return new WaitForSeconds(1f);
        damageText.transform.SetParent(null);
        damageText.GetComponent<Animator>().SetBool("isDamage", false);
        damageText.SetActive(false);
    }

    public static V getDicValue<K, V>(K type, Dictionary<K, V> dic)
    {
        V v;
        dic.TryGetValue(type, out v);
        return v;
    }

    public static string[] getCombineJsonPath()
    {
        string[] jsonName = Enum.GetNames(typeof(JsonPath));
        for (int i = 0; i < jsonName.Length; i++)
        {
            // 저장되는 위치 수정
            jsonName[i] = Path.Combine(Application.persistentDataPath, jsonName[i] + ".json");
        }
        return jsonName;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // 외부 스크립트
    GameManager gameManager;
    GameObject myCamera;
    public BoxCollider2D findEnemyBoxCollider;
    public Enemy target;

    // 내부 스크립트
    Animator animator;
    public BoxCollider2D boxCollider;
    public AudioSource audioSource;

    // 플레이어 설정
    public PlayerInfors nowPlayerInfors;
    Games games;
    public Vector2 colliderSize;

    // 오브젝트
    public GameObject bullet;

    // 게임 진행 관련 변수
    public float walkDelay;
    public float attackDelay;
    public float flowWalkDelay;
    public float flowAttackDelay;
    float flowHealthDown;
    int healthRefreshCnt = 1;
    bool onAttackSound;
    bool onWalkSound;
    bool onceAttack;

    // 애니메이션 상태 && 게임 진행 관련 변수
    public bool isWalk;
    public bool isAttack;
    public bool isDie;

    void Awake()
    {
        // 초반에는 쿨타임 없음
        LoadGameResource();
        StatsOnText();
        SetStats();
    }

    void Update()
    {
        flowWalkDelay += flowWalkDelay <= walkDelay ? Time.deltaTime : 0; // 적과 마주치지 않을때만 시간 흐르게함
        flowAttackDelay += flowAttackDelay <= attackDelay ? Time.deltaTime : 0; // 공격이 아닌 타이밍에만 딜레이 흐름

        if (!isDie)
        {
            AttackEnemy();
            FindEnemy();
            DownHealth();
        }
        AniState();
    }

    void AniState()
    {
        animator.SetBool("isWalk", isWalk);
        animator.SetBool("isAttack", isAttack);
        animator.SetBool("isDie", isDie);

        // 죽었다면
        if (GameManager.isEndAni(animator, "player_die", 1.0f) && Time.timeScale == 1)
        {
            Time.timeScale = 0;
            gameManager.panel_die.SetActive(true);

            gameManager.ChangeMenu(gameManager.panel_inforItems[4]);
        }
    }

    void FindEnemy()
    {
        // 필드에 적이 있고, 그 적이 싸우는 상태가 아니라면
        if (gameManager.now_Enemys.Count > 0 && !isWalk && !isAttack && target == null &&
            flowWalkDelay > walkDelay)
        {
            target = gameManager.now_Enemys.Pop().GetComponent<Enemy>();

            isWalk = true;
        }

        // 타겟이 정해졌다면
        if (isWalk)
        {
            if (!onWalkSound)
            {
                onWalkSound = true;
                AudioClip audioClip = GameManager.getDicValue<string, AudioClip>("walk", gameManager.sounds);
                gameManager.SoundPlay(audioSource, audioClip, true);
            }

            transform.position = Vector2.MoveTowards(transform.position, target.transform.position, nowPlayerInfors.speed * Time.deltaTime);
            myCamera.transform.position = (transform.position + Vector3.back * 10) + (Vector3.down * 1.2f);
            // 좌우 반전
            if (target.transform.position.x < transform.position.x)
                transform.eulerAngles = Vector2.up * 180;
            else
                transform.eulerAngles = Vector2.up * 0;

            // 콜라이더 오류 발생시
            if(transform.position == target.transform.position)
            {
                findEnemyBoxCollider.enabled = false;
                findEnemyBoxCollider.enabled = true;
            }
        }
        else onWalkSound = false;
    }

    void AttackEnemy()
    {
        // state를 바꿈으로써 공격 딜레이를 줌
        if (GameManager.isEndAni(animator, "player_attack", 1.0f))
        {
            onceAttack = false;
            onAttackSound = false;
            isAttack = false;
            boxCollider.enabled = false;
            flowAttackDelay = 0;
            flowWalkDelay = 0;
        }
        else if (!onAttackSound && GameManager.isEndAni(animator, "player_attack", 0.4f))
        {
            // 캐릭터 이름에 맞게 공격소리 재생
            onAttackSound = true;
            AudioClip audioClip = GameManager.getDicValue(nowPlayerInfors.name, gameManager.sounds);
            gameManager.SoundPlay(audioSource, audioClip, false);
        }

        // 적을 발견하고, 일정시간이 지나면 데미지를 넣음
        if (target != null && !isWalk && flowAttackDelay > attackDelay)
            isAttack = true;

        // 근거리 공격이라면
        if (!onceAttack && isAttack && games.playerIndex != 2)
        {
            onceAttack = true;

            boxCollider.enabled = true;
        }
        // 원거리 공격이라면
        else if (!onceAttack && isAttack && games.playerIndex == 2 && bullet == null)
        {
            onceAttack = true;

            bullet = gameManager.objectManager.getObj("player2_bullet");
            bullet.transform.position = transform.position;
            // 총알 각도
            Vector2 dis = target.transform.position - bullet.transform.position;
            float angle = Mathf.Atan2(dis.y, dis.x) * Mathf.Rad2Deg;
            bullet.transform.eulerAngles = Vector3.forward * angle;
        }

        if (bullet != null && target != null)
        {
            // 선형 보간으로 적에게 총알 발사
            Vector2 a = Vector2.Lerp(bullet.transform.position, target.transform.position, 5 * Time.deltaTime);
            bullet.transform.position = a;
        }
    }

    void DownHealth()
    {
        flowHealthDown += Time.deltaTime;
        // 1초 간격으로 체력 깎임
        if (flowHealthDown > healthRefreshCnt)
        {
            SetHealth(-1);
            healthRefreshCnt++;
        }
    }

    public void ChangePlayer(int playerIndex)
    {
        // 플레이어 변경
        games.playerIndex = playerIndex;
        nowPlayerInfors = gameManager.playerInfors[games.playerIndex];
        if(target != null) isWalk = true;

        // 공격 범위 (콜라이더 초기화)
        boxCollider.enabled = false;
        findEnemyBoxCollider.enabled = false;

        StatsOnText();
        // 체력이 0이면서 플레이어 바뀔때 체력 풀로 차는거 나중에 고쳐야함
        SetStats();
    }

    public void SkillHeal()
    {
        // 회복된 피가 최대 체력을 넘어간다면
        SetHealth(10);
    }

    public void SkillGetExp()
    {
        SetExp(50);
    }

    void SetHealth(int value)
    {
        games.health += value;

        if (games.health > nowPlayerInfors.health)
            games.health = (int)nowPlayerInfors.health;

        if(games.health <= 0)
        {
            isDie = true;
            OffAllAniState();
        }

        gameManager.healthBar.size = games.health / nowPlayerInfors.health;
        gameManager.text_healthBar.text = $"{games.health} HP / {nowPlayerInfors.health} HP"; // 체력
    }

    public void SetExp(float value)
    {
        // 최종 레벨 도달 전이라면
        if (gameManager.games.level < games.maxExps.Length - 1)
        {
            games.exp += value;

            if (games.exp >= gameManager.games.maxExps[gameManager.games.level])
            {
                // 스킬포인트 추가
                gameManager.games.skillPoint += 2;

                // 경험치 초기화, 레벨업
                games.exp = games.exp - gameManager.games.maxExps[gameManager.games.level];
                gameManager.games.level++;
            }

            gameManager.expBar.size = games.exp / gameManager.games.maxExps[gameManager.games.level];
            gameManager.text_expBar.text = $"{gameManager.games.level} Level / {(Mathf.Floor(((games.exp / gameManager.games.maxExps[gameManager.games.level] * 100) * 100)) / 100).ToString()}%";
            gameManager.text_skillPoint.text = $"POINT : {games.skillPoint}";
        }
    }

    public void SetMoeny(int value)
    {
        games.money += value;
        gameManager.text_money.text = $"Money : ${games.money}";
    }

    public float UpgradeStat(string type, float target, float value)
    {
        target = Mathf.Floor((target + value) * 10) / 10f; // 소수점은 두자리까지

        Text text;
        gameManager.text_stats.TryGetValue(type, out text);
        text.text = $"x {target}";

        return target;
    }

    void LoadGameResource()
    {
        flowWalkDelay = walkDelay;
        flowAttackDelay = attackDelay;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        myCamera = GameObject.Find("Main Camera");
        findEnemyBoxCollider = transform.Find("FindEnemyCollider").GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        games = gameManager.games;
    }

    public void SetStats()
    {
        // 애니메이션 변경
        animator.runtimeAnimatorController = gameManager.player_animatorC[games.playerIndex];

        // 공격 범위
        colliderSize = new Vector2(nowPlayerInfors.attackRangeX, nowPlayerInfors.attackRangeY);
        boxCollider.size = colliderSize;
        findEnemyBoxCollider.size = colliderSize;
        findEnemyBoxCollider.enabled = true;

        // 공격은 Enemy.OnTriggerEnter2D() 에서 계속 적용중

        // 공격 속도
        animator.SetFloat("attackSpeed", nowPlayerInfors.attackSpeed);

        // 스피드는 FindEnemy() 에서 계속 적용중
        animator.SetFloat("speed", nowPlayerInfors.speed);

        // 체력
        games.health = games.health <= 0 ? (int)nowPlayerInfors.health : games.health;
        SetHealth(0);

        // 경험치
        SetExp(0);

        // 돈
        SetMoeny(0);

        // 점수
    }

    void StatsOnText()
    {
        // 저장되있던 스텟들 설정
        GameManager.getDicValue<string, Text>("attack", gameManager.text_stats).text = $"x {nowPlayerInfors.attack.ToString()}";
        GameManager.getDicValue<string, Text>("attackSpeed", gameManager.text_stats).text = $"x {nowPlayerInfors.attackSpeed.ToString()}";
        GameManager.getDicValue<string, Text>("speed", gameManager.text_stats).text = $"x {nowPlayerInfors.speed.ToString()}";
        GameManager.getDicValue<string, Text>("health", gameManager.text_stats).text = $"x {nowPlayerInfors.health.ToString()}";
        GameManager.getDicValue<string, Text>("exp", gameManager.text_stats).text = $"x {nowPlayerInfors.exp.ToString()}";
        GameManager.getDicValue<string, Text>("money", gameManager.text_stats).text = $"x {nowPlayerInfors.money.ToString()}";

        SetExp(0);
    }

    void OffAllAniState()
    {
        isWalk = false;
        isAttack = false;
    }
}


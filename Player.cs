using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // �ܺ� ��ũ��Ʈ
    GameManager gameManager;
    GameObject myCamera;
    public BoxCollider2D findEnemyBoxCollider;
    public Enemy target;

    // ���� ��ũ��Ʈ
    Animator animator;
    public BoxCollider2D boxCollider;
    public AudioSource audioSource;

    // �÷��̾� ����
    public PlayerInfors nowPlayerInfors;
    Games games;
    public Vector2 colliderSize;

    // ������Ʈ
    public GameObject bullet;

    // ���� ���� ���� ����
    public float walkDelay;
    public float attackDelay;
    public float flowWalkDelay;
    public float flowAttackDelay;
    float flowHealthDown;
    int healthRefreshCnt = 1;
    bool onAttackSound;
    bool onWalkSound;
    bool onceAttack;

    // �ִϸ��̼� ���� && ���� ���� ���� ����
    public bool isWalk;
    public bool isAttack;
    public bool isDie;

    void Awake()
    {
        // �ʹݿ��� ��Ÿ�� ����
        LoadGameResource();
        StatsOnText();
        SetStats();
    }

    void Update()
    {
        flowWalkDelay += flowWalkDelay <= walkDelay ? Time.deltaTime : 0; // ���� ����ġ�� �������� �ð� �帣����
        flowAttackDelay += flowAttackDelay <= attackDelay ? Time.deltaTime : 0; // ������ �ƴ� Ÿ�ֿ̹��� ������ �帧

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

        // �׾��ٸ�
        if (GameManager.isEndAni(animator, "player_die", 1.0f) && Time.timeScale == 1)
        {
            Time.timeScale = 0;
            gameManager.panel_die.SetActive(true);

            gameManager.ChangeMenu(gameManager.panel_inforItems[4]);
        }
    }

    void FindEnemy()
    {
        // �ʵ忡 ���� �ְ�, �� ���� �ο�� ���°� �ƴ϶��
        if (gameManager.now_Enemys.Count > 0 && !isWalk && !isAttack && target == null &&
            flowWalkDelay > walkDelay)
        {
            target = gameManager.now_Enemys.Pop().GetComponent<Enemy>();

            isWalk = true;
        }

        // Ÿ���� �������ٸ�
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
            // �¿� ����
            if (target.transform.position.x < transform.position.x)
                transform.eulerAngles = Vector2.up * 180;
            else
                transform.eulerAngles = Vector2.up * 0;

            // �ݶ��̴� ���� �߻���
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
        // state�� �ٲ����ν� ���� �����̸� ��
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
            // ĳ���� �̸��� �°� ���ݼҸ� ���
            onAttackSound = true;
            AudioClip audioClip = GameManager.getDicValue(nowPlayerInfors.name, gameManager.sounds);
            gameManager.SoundPlay(audioSource, audioClip, false);
        }

        // ���� �߰��ϰ�, �����ð��� ������ �������� ����
        if (target != null && !isWalk && flowAttackDelay > attackDelay)
            isAttack = true;

        // �ٰŸ� �����̶��
        if (!onceAttack && isAttack && games.playerIndex != 2)
        {
            onceAttack = true;

            boxCollider.enabled = true;
        }
        // ���Ÿ� �����̶��
        else if (!onceAttack && isAttack && games.playerIndex == 2 && bullet == null)
        {
            onceAttack = true;

            bullet = gameManager.objectManager.getObj("player2_bullet");
            bullet.transform.position = transform.position;
            // �Ѿ� ����
            Vector2 dis = target.transform.position - bullet.transform.position;
            float angle = Mathf.Atan2(dis.y, dis.x) * Mathf.Rad2Deg;
            bullet.transform.eulerAngles = Vector3.forward * angle;
        }

        if (bullet != null && target != null)
        {
            // ���� �������� ������ �Ѿ� �߻�
            Vector2 a = Vector2.Lerp(bullet.transform.position, target.transform.position, 5 * Time.deltaTime);
            bullet.transform.position = a;
        }
    }

    void DownHealth()
    {
        flowHealthDown += Time.deltaTime;
        // 1�� �������� ü�� ����
        if (flowHealthDown > healthRefreshCnt)
        {
            SetHealth(-1);
            healthRefreshCnt++;
        }
    }

    public void ChangePlayer(int playerIndex)
    {
        // �÷��̾� ����
        games.playerIndex = playerIndex;
        nowPlayerInfors = gameManager.playerInfors[games.playerIndex];
        if(target != null) isWalk = true;

        // ���� ���� (�ݶ��̴� �ʱ�ȭ)
        boxCollider.enabled = false;
        findEnemyBoxCollider.enabled = false;

        StatsOnText();
        // ü���� 0�̸鼭 �÷��̾� �ٲ� ü�� Ǯ�� ���°� ���߿� ���ľ���
        SetStats();
    }

    public void SkillHeal()
    {
        // ȸ���� �ǰ� �ִ� ü���� �Ѿ�ٸ�
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
        gameManager.text_healthBar.text = $"{games.health} HP / {nowPlayerInfors.health} HP"; // ü��
    }

    public void SetExp(float value)
    {
        // ���� ���� ���� ���̶��
        if (gameManager.games.level < games.maxExps.Length - 1)
        {
            games.exp += value;

            if (games.exp >= gameManager.games.maxExps[gameManager.games.level])
            {
                // ��ų����Ʈ �߰�
                gameManager.games.skillPoint += 2;

                // ����ġ �ʱ�ȭ, ������
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
        target = Mathf.Floor((target + value) * 10) / 10f; // �Ҽ����� ���ڸ�����

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
        // �ִϸ��̼� ����
        animator.runtimeAnimatorController = gameManager.player_animatorC[games.playerIndex];

        // ���� ����
        colliderSize = new Vector2(nowPlayerInfors.attackRangeX, nowPlayerInfors.attackRangeY);
        boxCollider.size = colliderSize;
        findEnemyBoxCollider.size = colliderSize;
        findEnemyBoxCollider.enabled = true;

        // ������ Enemy.OnTriggerEnter2D() ���� ��� ������

        // ���� �ӵ�
        animator.SetFloat("attackSpeed", nowPlayerInfors.attackSpeed);

        // ���ǵ�� FindEnemy() ���� ��� ������
        animator.SetFloat("speed", nowPlayerInfors.speed);

        // ü��
        games.health = games.health <= 0 ? (int)nowPlayerInfors.health : games.health;
        SetHealth(0);

        // ����ġ
        SetExp(0);

        // ��
        SetMoeny(0);

        // ����
    }

    void StatsOnText()
    {
        // ������ִ� ���ݵ� ����
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


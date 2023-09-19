using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // 외부 스크립트
    GameManager gameManager;
    Player player;

    // 내부 스크립트
    Animator animator;

    // 적 관련 설정
    public EnemyInfors nowEnemyInfors;
    public int enemyIndex;
    GameObject healthBar;

    // 게임 진행 관련 변수
    float nowHealth;

    // 애니메이션 상태 && 게임 진행 관련 변수
    bool isDie;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        player = gameManager.player;
        animator = GetComponent<Animator>();
        nowEnemyInfors = gameManager.enemyInfors[enemyIndex];

        nowHealth = gameManager.enemyInfors[enemyIndex].health;
        healthBar = transform.Find("healthBar_white").Find("healthBar_red").gameObject;
    }

    private void Update()
    {
        AniState();
    }

    void AniState()
    {
        animator.SetBool("isDie", isDie);

        if(GameManager.isEndAni(animator, "enemy_die", 1.0f))
        {
            isDie = false;
            gameObject.SetActive(false);
            player.findEnemyBoxCollider.enabled = true;

            // 체력바 초기화
            nowHealth = nowEnemyInfors.health;
            healthBar.transform.localPosition = Vector3.zero;
            healthBar.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어의 탐지 콜라이더에 닿였고, 플레이어의 타겟이 나라면
        if(player.target != null && player.target.name.Equals(gameObject.name) && !player.isDie)
        {
            if (collision.name.Equals("FindEnemyCollider"))
            {
                // 범위에 들어오면 싸움 모드 온, 
                player.isWalk = false;
            }
            // 플레이어의 공격 콜라이더에 닿였다면
            else if (collision.tag.Equals("Player") || collision.tag.Equals("Bullet"))
            {
                // 체력바 설정
                nowHealth -= player.nowPlayerInfors.attack;
                float healthBarSize = nowHealth > 0 ? nowHealth / nowEnemyInfors.health : 0;
                healthBar.transform.localPosition = Vector2.left * (1 - healthBarSize) / 2;
                healthBar.transform.localScale = new Vector3(healthBarSize, 1, 1);

                // 총알에 닿였다면 추가 이벤트
                if (collision.tag.Equals("Bullet"))
                {
                    collision.gameObject.SetActive(false);
                    player.bullet = null;
                }

                if (nowHealth <= 0)
                    KillEnemy();
                else
                {
                    // 데미지 텍스트창
                    GameObject damageText = gameManager.objectManager.getObj("text_damage");
                    damageText.transform.SetParent(gameObject.transform);
                    damageText.transform.position = gameObject.transform.position;
                    damageText.GetComponent<TextMeshPro>().text = player.nowPlayerInfors.attack.ToString();
                    damageText.GetComponent<Animator>().SetBool("isDamage", true);

                    StartCoroutine(GameManager.OffDamageText(damageText));
                }
            }
        }
    }

    public void KillEnemy()
    {
        // 콜라이더 재설정(이미 들어와있던 오브젝트들 전부 초기화)
        player.boxCollider.enabled = false;
        player.findEnemyBoxCollider.enabled = false;

        isDie = true;
        player.target = null;
        player.isWalk = false;

        // 경험치, 돈 이벤트
        player.SetExp(player.nowPlayerInfors.exp);
        player.SetMoeny(nowEnemyInfors.money + (int)player.nowPlayerInfors.money);
    }
}

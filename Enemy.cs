using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // �ܺ� ��ũ��Ʈ
    GameManager gameManager;
    Player player;

    // ���� ��ũ��Ʈ
    Animator animator;

    // �� ���� ����
    public EnemyInfors nowEnemyInfors;
    public int enemyIndex;
    GameObject healthBar;

    // ���� ���� ���� ����
    float nowHealth;

    // �ִϸ��̼� ���� && ���� ���� ���� ����
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

            // ü�¹� �ʱ�ȭ
            nowHealth = nowEnemyInfors.health;
            healthBar.transform.localPosition = Vector3.zero;
            healthBar.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �÷��̾��� Ž�� �ݶ��̴��� �꿴��, �÷��̾��� Ÿ���� �����
        if(player.target != null && player.target.name.Equals(gameObject.name) && !player.isDie)
        {
            if (collision.name.Equals("FindEnemyCollider"))
            {
                // ������ ������ �ο� ��� ��, 
                player.isWalk = false;
            }
            // �÷��̾��� ���� �ݶ��̴��� �꿴�ٸ�
            else if (collision.tag.Equals("Player") || collision.tag.Equals("Bullet"))
            {
                // ü�¹� ����
                nowHealth -= player.nowPlayerInfors.attack;
                float healthBarSize = nowHealth > 0 ? nowHealth / nowEnemyInfors.health : 0;
                healthBar.transform.localPosition = Vector2.left * (1 - healthBarSize) / 2;
                healthBar.transform.localScale = new Vector3(healthBarSize, 1, 1);

                // �Ѿ˿� �꿴�ٸ� �߰� �̺�Ʈ
                if (collision.tag.Equals("Bullet"))
                {
                    collision.gameObject.SetActive(false);
                    player.bullet = null;
                }

                if (nowHealth <= 0)
                    KillEnemy();
                else
                {
                    // ������ �ؽ�Ʈâ
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
        // �ݶ��̴� �缳��(�̹� �����ִ� ������Ʈ�� ���� �ʱ�ȭ)
        player.boxCollider.enabled = false;
        player.findEnemyBoxCollider.enabled = false;

        isDie = true;
        player.target = null;
        player.isWalk = false;

        // ����ġ, �� �̺�Ʈ
        player.SetExp(player.nowPlayerInfors.exp);
        player.SetMoeny(nowEnemyInfors.money + (int)player.nowPlayerInfors.money);
    }
}

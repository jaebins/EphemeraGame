using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public GameObject enemy_0Prefeb;
    public GameObject player2_bulletPrefeb;
    public GameObject text_damagePrefeb;

    GameObject[] enemy_0 = new GameObject[30];
    GameObject[] player2_bullet = new GameObject[10];
    GameObject[] text_damage = new GameObject[5];

    void Awake()
    {
        FillArray(enemy_0Prefeb, enemy_0, true);
        FillArray(player2_bulletPrefeb, player2_bullet, false);
        FillArray(text_damagePrefeb, text_damage, false);
    }

    void FillArray(GameObject tar, GameObject[] tarArr, bool isIncName)
    {
        for(int i = 0; i < tarArr.Length; i++)
        {
            GameObject obj = Instantiate(tar);
            obj.SetActive(false);
            if (isIncName) obj.name = obj.name + i;
            tarArr[i] = obj;
        }
    }

    public GameObject getObj(string name)
    {
        GameObject[] objs = null;

        switch(name)
        {
            case "enemy_0":
                objs = enemy_0;
                break;
            case "player2_bullet":
                objs = player2_bullet;
                break;
            case "text_damage":
                objs = text_damage;
                break;
        }

        for(int i = 0; i < objs.Length; i++)
        {
            if (!objs[i].activeSelf)
            {
                objs[i].SetActive(true);
                return objs[i];
            }
        }

        return null;
    }
}

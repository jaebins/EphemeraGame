using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Events : MonoBehaviour
{
    // �ܺ� ��ũ��Ʈ
    GameManager gameManager;

    // ���� ��ũ��Ʈ
    EventTrigger eventTrigger;
    Scrollbar scrollbar;

    // ���� ����

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        eventTrigger = GetComponent<EventTrigger>();

        EventTrigger.Entry ev = new EventTrigger.Entry();
        ev.eventID = EventTriggerType.PointerClick;

        // ��ȭ �̺�Ʈ
        if (gameObject.name.Contains("LevelUp"))
        {
            string SkillLevelUpType = gameObject.name.Substring(8, gameObject.name.Length - 8);

            ev.callback.AddListener((e) => gameManager.SkillLevelUp(SkillLevelUpType));
            eventTrigger.triggers.Add(ev);
        }
        // �÷��� ���� �̺�Ʈ
        else if (gameObject.name.Contains("Player"))
        {
            int playerIndex = Int32.Parse(gameObject.name[^1].ToString());

            ev.callback.AddListener((e) => gameManager.ChangePlayer(playerIndex));
            eventTrigger.triggers.Add(ev);
        }
        // ��ų �̺�Ʈ
        else if (gameObject.name.Contains("Use"))
        {
            int skillType = Int32.Parse(gameObject.name[^1].ToString());
            Skills nowSkill = gameManager.skills[skillType];

            Text priceText = transform.GetChild(0).GetComponent<Text>();
            priceText.text = nowSkill.price.ToString();
            switch (nowSkill.priceType)
            {
                case "skillPoint":
                    priceText.color = new Color(0.3f, 0.9f, 0.9f, 1f);
                    break;
                case "money":
                    priceText.color = new Color(1f, 1f, 0f, 1f);
                    break;
            }

            ev.callback.AddListener((e) => gameManager.UseSkill(nowSkill));
            eventTrigger.triggers.Add(ev);
        }
        // ���� �̺�Ʈ
        else if (gameObject.name.Contains("Buy"))
        {
            int buyType = Int32.Parse(gameObject.name[^1].ToString());

            int needMoney = gameManager.playerInfors[buyType].price;
            transform.GetChild(2).GetComponent<Text>().text = needMoney.ToString();

            ev.callback.AddListener((e) => gameManager.BuyPlayer(buyType, needMoney));
            eventTrigger.triggers.Add(ev);
        }
        // �ɼ� �̺�Ʈ
        else if(gameObject.name.Contains("Setting"))
        {
            if (gameObject.name.Equals("SettingRetryBut"))
            {
                ev.callback.AddListener((e) => gameManager.RetryGame());
                eventTrigger.triggers.Add(ev);
            }
            else if (gameObject.name.Equals("SettingResetDataBut"))
            {
                ev.callback.AddListener((e) => gameManager.ResetGameData());
                eventTrigger.triggers.Add(ev);
            }
            else if (gameObject.name.Contains("SoundScroll"))
            {
                scrollbar = GetComponent<Scrollbar>();
                scrollbar.onValueChanged.AddListener((e) => gameManager.SetSoundEmploy(gameObject.name));
            }
        }

        // �޴�â �ٲٱ�
        switch (gameObject.name)
        {
            case "UpgradeBut":
                ev.callback.AddListener((e) => gameManager.ChangeMenu(gameManager.panel_inforItems[0]));
                eventTrigger.triggers.Add(ev);
                break;
            case "ChangeBut":
                ev.callback.AddListener((e) => gameManager.ChangeMenu(gameManager.panel_inforItems[1]));
                eventTrigger.triggers.Add(ev);
                break;
            case "SkillBut":
                ev.callback.AddListener((e) => gameManager.ChangeMenu(gameManager.panel_inforItems[2]));
                eventTrigger.triggers.Add(ev);
                break;
            case "ShopBut":
                ev.callback.AddListener((e) => gameManager.ChangeMenu(gameManager.panel_inforItems[3]));
                eventTrigger.triggers.Add(ev);
                break;
            case "OptionBut":
                ev.callback.AddListener((e) => gameManager.ChangeMenu(gameManager.panel_inforItems[4]));
                eventTrigger.triggers.Add(ev);
                break;
        }
    }

}

class GamesJson
{
    public Games games;
}

class PlayerInforsJson
{
    public PlayerInfors[] playerInfors;
}

class EnemyInforsJson
{
    public EnemyInfors[] enemyInfors;
}

class SkillsJson
{
    public Skills[] skills;
}

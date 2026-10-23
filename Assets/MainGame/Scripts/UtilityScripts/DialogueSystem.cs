using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DialogueSystem : MonoBehaviour
{
    [Header("Text Replacers")]
    [SerializeField] string playerNameReplacer = "{PlayerName}";

    [Header("Objects")]
    [SerializeField] GameObject ArrowImage;
    [SerializeField] TextMeshProUGUI TextObject;
    [SerializeField] GameObject NameTextObject;
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] Image NPC_Image;
    [SerializeField] List<string> textBreakdownList = new List<string>();
    [SerializeField] int maxWords;
    [SerializeField] float characterAppearSpeed;
    public string CharacterName;
    public UnityEvent events;
    int currentChar;
    int currentLine = 0;
    float timer;
    bool pushButton;
    public bool disableText;
    float typeSpeed;
    //TODO
    //trigger event after all dialogue appears

    public void TriggerEvent()
    {
        if (events != null)
        {
            events?.Invoke();
        }
    }
    private void Start()
    {
        typeSpeed = characterAppearSpeed;
        timer = characterAppearSpeed;
        //setUpArrayWithBrokenDownText();
        TextObject.text = "";
        NameText.text = CharacterName;
        GetNameVisibility();

         disableText = true;
    }
    private void Update()
    {
        
        if (Input.GetButtonDown("Interact"))
        {
            OnWordButtonClick();
            if (isAnimating == true && currentChar > 0)//used to instantly make text appear (skip text appear animation)
            {
                for (int i = currentChar; i < textBreakdownList[currentLine].Length; i++)
                {
                    TextObject.text += textBreakdownList[currentLine][i];
                }
                isAnimating = false;
                pushButton = true;
                ArrowImage.SetActive(true);
            }

           
        }
    }
    private void FixedUpdate()
    {
        AnimateText();
    }
    public void GetNameVisibility()
    {
        if (CharacterName == "")
        {
            NameTextObject.SetActive(false);
        }
        else
        {
            NameTextObject.SetActive(true);
        }
    }

    public void ReplaceTexts(ref List<string> text)
    {
        for (int i = 0; i < text.Count; i++)
        {
            text[i] = text[i].Replace(playerNameReplacer, PlayerStats.i.PlayerName);
        }
    }

    public void InitialiseNewText(List<string> text,string name, List<UnityAction> eventToTrigger)
    {
        if (disableText == false)
            return;

        disableText = false;
        CharacterName = name;
        ReplaceTexts(ref text);
        textBreakdownList = text;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        NPC_Image.gameObject.SetActive(false);
        events.RemoveAllListeners();
        for (int i = 0; i < eventToTrigger.Count; i++)
        {
            events.AddListener(eventToTrigger[i]);
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        ArrowImage.SetActive(false);

        NameText.text = CharacterName;
        GetNameVisibility();
    }
    
    public void InitialiseNewText(List<string> text,string name, UnityEvent eventToTrigger)
    {
        if (disableText == false)
            return;

        disableText = false;
        CharacterName = name;
        ReplaceTexts(ref text);
        textBreakdownList = text;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        NPC_Image.gameObject.SetActive(false);
        events.RemoveAllListeners();
        events.AddListener(eventToTrigger.Invoke);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        ArrowImage.SetActive(false);

        NameText.text = CharacterName;
        GetNameVisibility();
    }

    public void InitialiseNewTextSingle(string text, string name, List<UnityAction> eventToTrigger)
    {
        if (disableText == false)
            return;

        disableText = false;
        CharacterName = name;
        List<string> txt = new List<string>();
        txt.Add(text);
        ReplaceTexts(ref txt);
        textBreakdownList = txt;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        NPC_Image.gameObject.SetActive(false);
        events.RemoveAllListeners();
        for (int i = 0; i < eventToTrigger.Count; i++)
        {
            events.AddListener(eventToTrigger[i]);
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        ArrowImage.SetActive(false);

        NameText.text = CharacterName;
        GetNameVisibility();
    }
    public void InitialiseNewTextSingleAction(List<string> text, string name, UnityAction eventToTrigger)
    {
        if (disableText == false)
            return;

        disableText = false;
        CharacterName = name;
        ReplaceTexts(ref text);
        textBreakdownList = text;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        NPC_Image.gameObject.SetActive(false);
        events.RemoveAllListeners();
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        events.AddListener(eventToTrigger);
        events.AddListener(ResetTypingSpeed);
        ArrowImage.SetActive(false);

        NameText.text = CharacterName;
        GetNameVisibility();
    }
    public void InitialiseNewTextSingle(string text,float typingSpeed, UnityAction eventToTrigger)
    {
        if (disableText == false)
            return;

        characterAppearSpeed = typingSpeed;
        disableText = false;
        CharacterName = name;
        List<string> txt = new List<string>();
        txt.Add(text);
        ReplaceTexts(ref txt);
        textBreakdownList = txt;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        NPC_Image.gameObject.SetActive(false);
        events.RemoveAllListeners();
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        events.AddListener(eventToTrigger);
        events.AddListener(ResetTypingSpeed);
        ArrowImage.SetActive(false);

        NameText.text = CharacterName;
        GetNameVisibility();
    }

    void ResetTypingSpeed()
    {
        characterAppearSpeed = typeSpeed;
    }
    public void InitialiseNewTextNPC(List<string> text, string name,Sprite npc_sprite, UnityEvent eventToTrigger)
    {
        if (disableText == false)
            return;

        disableText = false;
        CharacterName = name;
        ReplaceTexts(ref text);
        textBreakdownList = text;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        if (npc_sprite != null)
        {
            NPC_Image.gameObject.SetActive(true);
            NPC_Image.sprite = npc_sprite;
        }

        events.RemoveAllListeners();
        events.AddListener(eventToTrigger.Invoke);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        ArrowImage.SetActive(false);

        NameText.text = CharacterName;
        GetNameVisibility();
    }
    public void InitialiseNewTextNPC(List<string> text, string name, Sprite npc_sprite, UnityAction eventToTrigger)
    {
        if (disableText == false)
            return;

        disableText = false;
        CharacterName = name;
        ReplaceTexts(ref text);
        textBreakdownList = text;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        if (npc_sprite != null)
        {
            NPC_Image.gameObject.SetActive(true);
            NPC_Image.sprite = npc_sprite;
        }

        events.RemoveAllListeners();
        events.AddListener(eventToTrigger);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        ArrowImage.SetActive(false);

        NameText.text = CharacterName;
        GetNameVisibility();
    }
    public void InitialiseNewFoundItemText(string itemName, UnityAction eventToTrigger)
    {
        if (disableText == false)
            return;
        CharacterName = "";
        List<string> txt = new List<string>();
        txt.Add("You found " + itemName+"!");
        ReplaceTexts(ref txt);
        textBreakdownList = txt;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        NPC_Image.gameObject.SetActive(false);
        events.RemoveAllListeners();
        events.AddListener(eventToTrigger);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        ArrowImage.SetActive(false);
        disableText = false;
        TextObject.text = "";
        NameText.text = CharacterName;
        GetNameVisibility();
    }
    public void InitialiseNewDefeatedText(string enemyName, UnityEvent eventToTrigger)
    {
        if (disableText == false)
            return;

        CharacterName = "";
        List<string> txt = new List<string>();
        txt.Add(enemyName + " was defeated!");
        ReplaceTexts(ref txt);
        textBreakdownList = txt;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        NPC_Image.gameObject.SetActive(false);
        events.RemoveAllListeners();
        events.AddListener(eventToTrigger.Invoke);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        ArrowImage.SetActive(false);
        disableText = false;
        TextObject.text = "";
        NameText.text = CharacterName;
        GetNameVisibility();
    }
    public void InitialiseNewUsedItemOnMonster(string ItemName, string monsterName, UnityAction eventToTrigger)
    {
        if (disableText == false)
            return;

        CharacterName = "";
        List<string> txt = new List<string>();
        txt.Add("Used "+ItemName + " on " + monsterName + "!");
        ReplaceTexts(ref txt);
        textBreakdownList = txt;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        NPC_Image.gameObject.SetActive(false);
        events.RemoveAllListeners();
        events.AddListener(eventToTrigger);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        ArrowImage.SetActive(false);
        disableText = false;
        TextObject.text = "";
        NameText.text = CharacterName;
        GetNameVisibility();
    }
    public void InitialiseNewLearnMove(string moveName, string monsterName, UnityAction eventToTrigger)
    {
        if (disableText == false)
            return;

        CharacterName = "";
        List<string> txt = new List<string>();
        txt.Add(monsterName + " learned " + moveName + "!");
        ReplaceTexts(ref txt);
        textBreakdownList = txt;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        NPC_Image.gameObject.SetActive(false);
        events.RemoveAllListeners();
        events.AddListener(eventToTrigger);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        ArrowImage.SetActive(false);
        disableText = false;
        TextObject.text = "";
        NameText.text = CharacterName;
        GetNameVisibility();
    }
    public void InitialiseNewCaughtText(string enemyName, UnityEvent eventToTrigger)
    {
        if (disableText == false)
            return;

        CharacterName = "";
        List<string> txt = new List<string>();
        txt.Add(enemyName + " was caught!");
        ReplaceTexts(ref txt);
        textBreakdownList = txt;
        currentChar = 0;
        currentLine = 0;
        pushButton = false;
        NPC_Image.gameObject.SetActive(false);
        events.RemoveAllListeners();
        events.AddListener(eventToTrigger.Invoke);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        ArrowImage.SetActive(false);
        disableText = false;
        TextObject.text = "";
        NameText.text = CharacterName;
        GetNameVisibility();
    }
    bool isAnimating;
    void AnimateText()
    {
        if (disableText == false)
        {

            if (timer <= 0 && pushButton == false)
            {
                if (currentChar < textBreakdownList[currentLine].Length )
                {
                    TextObject.text += textBreakdownList[currentLine][currentChar];
                    currentChar++;
                    isAnimating = true;
                }
                else
                {
                    pushButton = true;
                    ArrowImage.SetActive(true);
                    isAnimating = false;
                }
                
                timer = characterAppearSpeed;
            }
            else
            {
                timer -= Time.deltaTime;
            }
        }
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
       
    }
    public void OnWordButtonClick()
    {
        if (pushButton == true)
        {
            AudioSystem.i.PlaySoundClip(AudioSystem.i.UIClickSound, AudioMaster.UI);
            pushButton = false;
            ArrowImage.SetActive(false);
            if (currentLine < textBreakdownList.Count - 1)
            {
                TextObject.text = "";
                currentLine++;
                currentChar = 0;
            }
            else
            {
                TextObject.text = "";
                disableText = true;
                TriggerEvent();
            }
        }
    }

    int getWordCount()
    {
        int words = 0;
        for (int i = 0; i < TextObject.text.Length; i++)
        {
            if (TextObject.text[i] == ' ')
            {
                words++;
            }
        }
        return words;
    }
    void setUpArrayWithBrokenDownText()
    {
        int words = 0;
        string tempString = "";
        for (int i = 0; i < TextObject.text.Length; i++)
        {
            if (TextObject.text[i] == ' ')
            {
                words++;
            }
            tempString = tempString + TextObject.text[i];
            if (words == maxWords || i == TextObject.text.Length - 1)
            {
                textBreakdownList.Add(tempString);
                words = 0;
                tempString = "";
            }
        }
    }

}

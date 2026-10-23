using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class PartySelectionScript : MonoBehaviour
{
    [SerializeField] List<GameObject> menuContent;
    public List<GameObject> tempMenuContent = new List<GameObject>();
    //[SerializeField] GameObject selectedImage;
    [SerializeField] Material selectedShader;
    [SerializeField] UnityEvent closeMenuEvent;
    [SerializeField] bool verticalButtons;
    [SerializeField] bool HorizontalButtons;
    [SerializeField] int horizontalColumns;
    int current;
    [SerializeField] float cdtime;
    [SerializeField] bool cd;
    //Menu layout
    //0 1 2
    // 3 4
    private void OnEnable()
    {
        cd = false;
        if (UIManager.i.currentUI.Contains(gameObject))
            return;

        UIManager.i.currentUI.Push(gameObject);
        activateTempMenuContent();
    }
    private void OnDisable()
    {
        //if (UIManager.i.currentUI.Peek() == gameObject)
         //   selectedImage.SetActive(true);
        //else
         //   selectedImage.SetActive(false);

        if (UIManager.i.IsTopUI(gameObject))
        {
            UIManager.i.currentUI.Pop();
        }
    }

    public List<GameObject> GetMenuContent()
    {
        return menuContent;
    }
    public bool activateTempMenuContent()
    {
        tempMenuContent = menuContent;
        bool found = false;
        if (tempMenuContent.Count >= 1)
        {
            int i = 0;
            while (i < tempMenuContent.Count )//&& found == false)     4
            {
                if (i >= PlayerStats.i.PlayerParty.Count )//  i = 4        c = 3
                {
                    found = true;
                    tempMenuContent.RemoveAt(i);
                    i--;
                }
                i++;
            }
        }
        return found;
    }
    public void CreateNewPartyMember()
    {
        int nextAvailable = tempMenuContent.Count;
        UIManager.i.PartyScene.GetComponent<PartyScript>().members[nextAvailable].SetActive(true);
        //tempMenuContent.Add(UIManager.i.PartyScene.GetComponent<PartyScript>().members[nextAvailable]);
        menuContent.Add(UIManager.i.PartyScene.GetComponent<PartyScript>().members[nextAvailable]);
    }
    public bool RemoveMonsterFromSelection(BattleMonster monster)
    {
        bool found = false;
        int i = 0;
        while (i < tempMenuContent.Count && found == false)
        {
            if (found == false)
            {
                if (PlayerStats.i.PlayerParty[tempMenuContent[i].GetComponent<PartyMember>().GetMemberPos()] == monster)
                {
                    found = true;
                    tempMenuContent.RemoveAt(i);
                    //menuContent.RemoveAt(i);
                }
            }
            i++;
        }
        return found;
    }
    //this one is used much more simplier :/
    public void RemoveMonsterFromSelection2()
    {
        tempMenuContent.RemoveAt(tempMenuContent.Count -1);
    }

    private void Update()
    {
        //if (UIManager.i.currentUI.Peek() == gameObject)
        //    selectedImage.SetActive(true);
        //else
        //    selectedImage.SetActive(false);


        if (!UIManager.i.IsTopUI(gameObject))
            return;

        if (GameUtils.i.ControllerConnected() == false)
            return;

        if (cd == false)
        {
            if (CheckMonsterDragging() == true)
                return;

            StartCoroutine(ChangeSelection());
        }


        if (Input.GetButtonDown("Interact") )
        {
        
            tempMenuContent[current].GetComponent<Button>().onClick.Invoke();
           
        }else if (Input.GetButtonDown("StatsHolderMenu"))
        {
            GetComponent<PartyScript>().EnableStatsHolderMenu();
        }
        CloseMenu();
    }

    void CloseMenu()
    {
        if (Input.GetButtonDown("CloseMenu"))
        {
            closeMenuEvent.Invoke();
        }
    }

    IEnumerator ChangeSelection()
    {
      


        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        if (x !=0 || y != 0)
        {
            cd = true;
            if (verticalButtons == true && HorizontalButtons == true)
            {
                if (current < horizontalColumns && y < 0)
                {
                    current = horizontalColumns;
                }
                if (current >= horizontalColumns && y > 0)
                {
                    current = 0;
                }
            }
            if (verticalButtons == true && HorizontalButtons == false)
            {
                if (current < tempMenuContent.Count - 1 && y < 0)
                {
                    current += 1;
                }
                if (current > 0 && y > 0)
                {
                    current -= 1;
                }
            }
            if (HorizontalButtons == true)
            {
                if (current < tempMenuContent.Count - 1 && x > 0)
                {
                    current += 1;
                }
                if (current > 0 && x < 0)
                {
                    current -= 1;
                }
            }
            SelectOB(current);

            //selectedImage.transform.SetParent(tempMenuContent[current].transform);
            //selectedImage.transform.position = new Vector2(tempMenuContent[current].transform.position.x, tempMenuContent[current].transform.position.y);
            //selectedImage.transform.localScale = new Vector3(1, 1, 1);
            if (tempMenuContent[current].GetComponent<PartyMember>() != null)
            {
                for (int i = 0; i < tempMenuContent.Count; i++)
                {
                    tempMenuContent[i].GetComponent<PartyMember>().SetSelected(false);
                }
                tempMenuContent[current].GetComponent<PartyMember>().SetSelected(true);
            }
            yield return new WaitForSeconds(cdtime);
            cd = false;
        }
        
    }

    public void SelectOB(int member)
    {
        for (int i = 0; i < tempMenuContent.Count; i++)
        {
            tempMenuContent[i].GetComponent<Image>().material = null;
        }
        tempMenuContent[member].GetComponent<Image>().material = selectedShader;
    }

    bool CheckMonsterDragging()
    {
        bool drag = false;
        for (int i = 0; i < tempMenuContent.Count; i++)
        {
            if (tempMenuContent[i].GetComponent<PartyMember>().IsDragging() == true)
            {
                drag = true;
            }
        }
        return drag;
    }

}

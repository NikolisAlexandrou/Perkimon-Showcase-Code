using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
public class SelectionMenuScript : MonoBehaviour
{
    public List<GameObject> menuContent = new List<GameObject>();
    public List<GameObject> tempMenuContent = new List<GameObject>();
    //public GameObject selectedImage;
    //[SerializeField] Material selectedShader;
    [SerializeField] UnityEvent closeMenuEvent;
    [SerializeField] bool verticalButtons;
    [SerializeField] bool HorizontalButtons;
    [SerializeField] bool isSelectionWheel;
    [SerializeField] int horizontalColumns;
    public int current;
    [SerializeField] float cdtime;
    [SerializeField] bool cd;

    [SerializeField] bool onSelectItem;
    [SerializeField] UnityEvent OnSelectEvent;

    [SerializeField] bool secondaryMenu;
    [SerializeField] int memberForSecondaryMenu;
    [SerializeField] UnityEvent secondaryMenuEvent;

    //Menu layout
    //0 1 2
    // 3 4
    //Wheel menu layout
    //    2
    //  1   3
    // 0     4
    private void OnEnable()
    {
        cd = false;
       
        if (isSelectionWheel == true)
        {
            current = 2;
        }
        else
        {
            current = 0;
        }
       
        if (UIManager.i.currentUI.Contains(gameObject))
            return;

        UIManager.i.currentUI.Push(gameObject);
        activateTempMenuContent();
        SelectItem();

    }
    private void OnDisable()
    {
        if (UIManager.i.IsTopUI(gameObject))
        {
            //if (selectedImage != null)
            //{
            //    selectedImage?.SetActive(false);
            //}
            UIManager.i.currentUI.Pop();
        }
    }
    public bool activateTempMenuContent()
    {
        tempMenuContent = menuContent;
        bool found = false;
        if (tempMenuContent.Count >= 1)
        {
            int i = 0;
            while (i < tempMenuContent.Count && found == false)
            {
                if (tempMenuContent[i].activeSelf == false)
                {
                    found = true;
                    tempMenuContent.RemoveAt(i);
                }
                i++;
            }
        }
        return found;
    }

    private void Update()
    {
        // if (selectedImage != null)
        // {
        //    if (UIManager.i.currentUI.Peek() == gameObject && tempMenuContent.Count > 0)
        //        selectedImage?.SetActive(true);
        //    else
        //        selectedImage?.SetActive(false);
        //}
        if (!UIManager.i.IsTopUI(gameObject))
            return;
        if (GameUtils.i.ControllerConnected() == false)
            return;

        if (cd == false)
        {
            if (isSelectionWheel == false)
            {
                StartCoroutine(ChangeSelection());
            }
            else
            {
                StartCoroutine(ChangeSelectionWheel());
            }
        }
       

        if (Input.GetButtonDown("Interact"))
        {
            if (tempMenuContent[current].GetComponent<Button>() != null)
            {
                tempMenuContent[current].GetComponent<Button>().onClick.Invoke();
            }
        }
        else if (Input.GetButtonDown("StatsHolderMenu") && secondaryMenu == true && current == memberForSecondaryMenu)
        {
            secondaryMenuEvent.Invoke();
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
    public void SelectItem()
    {
        if (onSelectItem == true)
        {
            OnSelectEvent.Invoke();
            if (tempMenuContent[current].GetComponent<Button>() != null)
            {
                tempMenuContent[current].GetComponent<Button>().Select();
            }
           
        }
    }
    IEnumerator ChangeSelection()
    {
        if(UIManager.i.IsTopUI(UIManager.i.MonsterStorage))
        {
            if (MonsterStorageManager.i.currentUnit.IsDragging() == true)
            {
                yield break;
            }
        }


        cd = true;
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        if (verticalButtons == true && HorizontalButtons == true)
        {
            if (current + horizontalColumns < tempMenuContent.Count && y < 0)
            {

                current += horizontalColumns;
            }
            else if (current >= horizontalColumns && y > 0)
            {
                current -= horizontalColumns;
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

       // selectedImage.transform.SetParent(tempMenuContent[current].transform);
        //selectedImage.transform.position = new Vector2(tempMenuContent[current].transform.position.x, tempMenuContent[current].transform.position.y);
        SelectItem();
        //selectedImage.transform.localScale = new Vector3(1, 1, 1);
        //for (int i = 0; i < tempMenuContent.Count; i++)
        //{
        //    tempMenuContent[i].GetComponent<Image>().material = null;
        //}
        //tempMenuContent[current].GetComponent<Image>().material = selectedShader;
        if (UIManager.i.IsTopUI(gameObject))
        {
            tempMenuContent[current].GetComponent<Button>().Select();
        }
        yield return new WaitForSeconds(cdtime);
        cd = false;
    }
    IEnumerator ChangeSelectionMonsterStorage()
    {
        if (UIManager.i.IsTopUI(UIManager.i.MonsterStorage))
        {
            if (MonsterStorageManager.i.currentUnit.IsDragging() == true)
            {
                yield break;
            }
        }


        cd = true;
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        if (verticalButtons == true && HorizontalButtons == true)
        {     
            if (current + horizontalColumns < tempMenuContent.Count && y < 0)
            {

                current += horizontalColumns;
            }
            else if(current >= horizontalColumns && y > 0)
            {
                current -= horizontalColumns;
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
        SelectItem();
        if (UIManager.i.IsTopUI(gameObject))
        {
            tempMenuContent[current].GetComponent<Button>().Select();
        }
        yield return new WaitForSeconds(cdtime);
        cd = false;
    }
    IEnumerator ChangeSelectionWheel()
    {
        cd = true;
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        if (x < 0 && y <= 0.2f)
        {
            current = 0;
        }else if (x > 0 && y <= 0.2f)
        {
            current = 4;
        }else if (x < 0 && y > 0.2f)
        {
            current = 1;
        }
        else if (x > 0 && y > 0.2f)
        {
            current = 3;
        }
        else if (x <= 0.1f && x >= -0.1f && y > 0)
        {
            current = 2;
        }
        for (int i = 0; i < tempMenuContent.Count; i++)
        {
            if (current != i)
            {
                tempMenuContent[i].transform.DOScaleY(1, 0.1f);
            }
        }
        tempMenuContent[current].transform.DOScaleY(1.2f, 0.1f);
        //selectedImage.transform.SetParent(tempMenuContent[current].transform);
        ////selectedImage.transform.position = new Vector2(tempMenuContent[current].transform.position.x, tempMenuContent[current].transform.position.y);
        //selectedImage.transform.rotation = tempMenuContent[current].transform.rotation;
        SelectItem();
        //selectedImage.transform.localScale = new Vector3(1, 1, 1);
        //for (int i = 0; i < tempMenuContent.Count; i++)
        //{
        //    tempMenuContent[i].GetComponent<Image>().material = null;
        //}
        //tempMenuContent[current].GetComponent<Image>().material = selectedShader;
        if (UIManager.i.IsTopUI(gameObject))
        {
            tempMenuContent[current].GetComponent<Button>().Select();
        }
       
        yield return new WaitForSeconds(cdtime);
        cd = false;
    }
}

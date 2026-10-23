using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class DynamicControllerUISetUp : MonoBehaviour
{
    [SerializeField] GameObject elementContainerUI;
    [SerializeField] int columnSize;
    [SerializeField] bool Initialise;
    [SerializeField] bool Horizontal;
    [SerializeField] List<Button> StandardLeftButton;

    private void OnEnable()
    {
        if (UIManager.i.currentUI.Contains(gameObject))
            return;

        UIManager.i.currentUI.Push(gameObject);

        if (Initialise == true)
        {
            if (Horizontal == true)
            {
                InitialiseCatchingListHorizontalUI(true);
            }
            else
            {
                InitialiseCatchingListVerticalUI(true);
            }
           
        }
    }
    public GameObject GetElementContainer()
    {
        return elementContainerUI;
    }
    public void SetSelected(GameObject ob)
    {
        EventSystem.current.SetSelectedGameObject(ob);
    }
    public void SetSelected(int childPos)
    {
        EventSystem.current.SetSelectedGameObject(elementContainerUI.transform.GetChild(childPos).gameObject);
    }
    public List<Button> GetStandardLeftButton()
    {
        return StandardLeftButton;
    }
    private void OnDisable()
    {
        if (UIManager.i.IsTopUI(gameObject))
        {
            UIManager.i.currentUI.Pop();
        }
    }
    public void InitialiseCatchingListVerticalUI(bool firstInit)
    {
        //assign button positions for controller support
        for (int i = 0; i < elementContainerUI.transform.childCount; i++)
        {
            if (i == 0)//first element
            {
                for (int j = 0; j < StandardLeftButton.Count; j++)
                {
                    SetNavigationRight(StandardLeftButton[j], elementContainerUI.transform.GetChild(i).GetComponent<Button>());
                }
                elementContainerUI.transform.GetChild(i).gameObject?.AddComponent<ActivateFirstButton>();
                if (firstInit == true)
                {
                    GameUtils.i.SetNewSelectedUIObject(elementContainerUI.transform.GetChild(0).gameObject);
                }
               

                Button first = elementContainerUI.transform.GetChild(i).GetComponent<Button>();
                Button Left = elementContainerUI.transform.GetChild(elementContainerUI.transform.childCount - 1).GetComponent<Button>();
                Button Right = first;
                if (elementContainerUI.transform.childCount > 1)
                {
                    Right = elementContainerUI.transform.GetChild(i + 1).GetComponent<Button>();
                }
                
                SetNavigation(first, null, null, Left, Right);
                if (elementContainerUI.transform.childCount > columnSize)
                {
                    Button Down = elementContainerUI.transform.GetChild(columnSize).GetComponent<Button>();
                    SetNavigation(first, null, Down, Left, Right);
                }
            }
            else if (i == elementContainerUI.transform.childCount - 1)//last element
            {
                Button first = elementContainerUI.transform.GetChild(i).GetComponent<Button>();
                Button Left = elementContainerUI.transform.GetChild(i - 1).GetComponent<Button>();
                Button Right = elementContainerUI.transform.GetChild(0).GetComponent<Button>();
                SetNavigation(first,  null, null, Left, Right);
            }
            else //any other element
            {
                Button Up = null;
                Button Down = null;
                Button first = elementContainerUI.transform.GetChild(i).GetComponent<Button>();
                Button Left = elementContainerUI.transform.GetChild(i - 1).GetComponent<Button>();
                Button Right = elementContainerUI.transform.GetChild(i + 1).GetComponent<Button>();

                if (i - columnSize > 0)
                {
                    Up = elementContainerUI.transform.GetChild(i - columnSize).GetComponent<Button>();
                }
                else
                {
                    Up = elementContainerUI.transform.GetChild(0).GetComponent<Button>();
                }
                if (i + columnSize < elementContainerUI.transform.childCount)
                {
                    Down = elementContainerUI.transform.GetChild(i + columnSize).GetComponent<Button>();
                }
                else
                {
                    Down = elementContainerUI.transform.GetChild(elementContainerUI.transform.childCount - 1).GetComponent<Button>();
                }
                SetNavigation(first, Up, Down, Left, Right);
            }
        }
        //selects first element
        if (elementContainerUI.transform.childCount >= 1 && firstInit == true)
        {
            GameUtils.i.SetNewSelectedUIObject(elementContainerUI.transform.GetChild(0).gameObject);
        }

    }
    public IEnumerator initDelay(bool f)
    {
        yield return new WaitForSeconds(.1f);
        InitialiseCatchingListHorizontalUI(f);
    }
    public IEnumerator initDelayVertical(bool f)
    {
        yield return new WaitForSeconds(.1f);
        InitialiseCatchingListVerticalUI(f);
    }
    public void InitialiseCatchingListHorizontalUI(bool firstInit)
    {
        //assign button positions for controller support
        for (int i = 0; i < elementContainerUI.transform.childCount; i++)
        {
            if (elementContainerUI.transform.childCount < 2)
                return;

            if (i == 0)//first element
            {
                
                elementContainerUI.transform.GetChild(i).gameObject?.AddComponent<ActivateFirstButton>();
                if (firstInit == true)
                {
                    EventSystem.current.SetSelectedGameObject(elementContainerUI.transform.GetChild(0).gameObject);
                }
                Button first = elementContainerUI.transform.GetChild(i).GetComponent<Button>();
                Button Left = elementContainerUI.transform.GetChild(elementContainerUI.transform.childCount - 1).GetComponent<Button>();
                Button Right = elementContainerUI.transform.GetChild(i + 1).GetComponent<Button>();
                SetNavigation(first, Left, Right, null, null);
                if (elementContainerUI.transform.childCount > columnSize)
                {
                    Button Down = elementContainerUI.transform.GetChild(columnSize).GetComponent<Button>();
                    SetNavigation(first, Left, Right, null, Down);
                }
            }
            else if (i == elementContainerUI.transform.childCount - 1)//last element
            {
                Button first = elementContainerUI.transform.GetChild(i).GetComponent<Button>();
                Button Left = elementContainerUI.transform.GetChild(i - 1).GetComponent<Button>();
                Button Right = elementContainerUI.transform.GetChild(0).GetComponent<Button>();
                SetNavigation(first, Left, Right, null, null);
            }
            else //any other element
            {
                Button Up = null;
                Button Down = null;
                Button first = elementContainerUI.transform.GetChild(i).GetComponent<Button>();
                Button Left = elementContainerUI.transform.GetChild(i - 1).GetComponent<Button>();
                Button Right = elementContainerUI.transform.GetChild(i + 1).GetComponent<Button>();

                if (i - columnSize > 0)
                {
                    Up = elementContainerUI.transform.GetChild(i - columnSize).GetComponent<Button>();
                }
                else
                {
                    Up = elementContainerUI.transform.GetChild(0).GetComponent<Button>();
                }
                if (i + columnSize < elementContainerUI.transform.childCount)
                {
                    Down = elementContainerUI.transform.GetChild(i + columnSize).GetComponent<Button>();
                }
                else
                {
                    Down = elementContainerUI.transform.GetChild(elementContainerUI.transform.childCount - 1).GetComponent<Button>();
                }
                SetNavigation(first, Left, Right, Up, Down);
            }
        }
        //selects first element
        if (elementContainerUI.transform.childCount >= 1 && firstInit == true)
        {
            EventSystem.current.SetSelectedGameObject(elementContainerUI.transform.GetChild(0).gameObject);
        }

    }

    bool IsMultiple(int number, int divisor)
    {
        return number % divisor == 0;
    }

    void SetNavigation(Selectable current, Selectable left, Selectable right, Selectable Up, Selectable Down)
    {
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.Explicit; // Set manual navigation mode
        if (StandardLeftButton.Count >= 1)
        {
            nav.selectOnLeft = StandardLeftButton[0];
        }
        else
        {
            nav.selectOnLeft = left;
        }
        
        nav.selectOnRight = right;
        nav.selectOnUp = Up;
        nav.selectOnDown = Down;

        current.navigation = nav; // Apply navigation to the button
    }
    void SetNavigationRight(Selectable current,Selectable right)
    {
        Navigation nav = current.navigation;
        nav.mode = Navigation.Mode.Explicit; // Set manual navigation mode
        nav.selectOnRight = right;
        current.navigation = nav; // Apply navigation to the button
    }
}

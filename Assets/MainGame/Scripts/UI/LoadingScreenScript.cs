using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LoadingScreenScript : MonoBehaviour
{
    [SerializeField] Image UpBackgroundImage;
    [SerializeField] Image DownBackgroundImage;
    [SerializeField] TextMeshProUGUI RouteNameText;
    [SerializeField] TextMeshProUGUI TimeText;
    [SerializeField] TextMeshProUGUI SeasonText;
    [SerializeField] Image WeatherImage;
    [SerializeField] Sprite SunImage;
    [SerializeField] Sprite MoonImage;
    [SerializeField] Sprite RainImage;
    [SerializeField] float LoadTime;
    public void ChangeSeasonName()
    {
        SeasonText.text = DaynightCycleScript.i.Season.ToString();
    }
    public void ChangeRouteName()
    {
        if (GameUtils.i.player.GetComponent<PlayerController>().CurrentArea == null)
            return;
        
        RouteNameText.text = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.AreaName;
    }

    public void ChangeTimeText()
    {
        TimeText.text = DaynightCycleScript.i.formatTime((int)DaynightCycleScript.i.TimeHours) + ":" + DaynightCycleScript.i.formatTime(DaynightCycleScript.i.TimeMinutes);
    }

    public void ChangeBackgrounds()
    {
        if (GameUtils.i.player.GetComponent<PlayerController>().CurrentArea == null)
            return;
        
        UpBackgroundImage.sprite = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.BackgroundUpLoading;
        DownBackgroundImage.sprite = GameUtils.i.player.GetComponent<PlayerController>().CurrentArea.BackgroundDownLoading;
    }

    public void ChangeWeatherImage()
    {
        if (DaynightCycleScript.i.HasWeather == true)
        {
            WeatherImage.sprite = RainImage;
            return;
        }

        if (DaynightCycleScript.i.IsNight == true)
            WeatherImage.sprite = MoonImage;
        else
            WeatherImage.sprite = SunImage;
    }
    private void OnEnable()
    {
        ChangeSeasonName();
        ChangeRouteName();
        ChangeTimeText();
        ChangeWeatherImage();
        ChangeBackgrounds();
        StartCoroutine(LoadScreen());
    }

    public void DisableOb()
    {
        gameObject.SetActive(false);
    }

    public IEnumerator LoadScreen()
    {
        GetComponent<Animator>().SetBool("ScreenLoaded",true);
        yield return new WaitForSeconds(LoadTime);
        GetComponent<Animator>().SetBool("ScreenLoaded", false);
    }
}

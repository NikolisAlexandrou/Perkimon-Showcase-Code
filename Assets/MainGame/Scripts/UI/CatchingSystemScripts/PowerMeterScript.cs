using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;
using TMPro;

public class PowerMeter : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnSuccessNextTrialEvent;
    public UnityEvent OnSuccessEvent;
    public UnityEvent OnFailEvent;

    [Header("Objects")]
    public RectTransform arrow;  // Assign the arrow UI element
    public RectTransform bar;    // Assign the full red bar (background)
    public RectTransform successZone; // Assign the green success area

    [Header("Default Values")]
    [SerializeField] const float minSizeDefault = 30f; // Minimum width
    [SerializeField] const float maxSizeDefault = 50f; // Maximum width
    public const float speedDefault = 175f;  // Arrow movement speed
    public const float trialSpeedIncreaseDefault = 1.15f;  // Arrow movement speed

    [Header("Partile Properties")]
    [SerializeField] float sizeIncrease = 0.05f;
    [SerializeField] float simulationSpeedIncrease = 0.3f;
    [SerializeField] float lifetimeIncrease = 0.1f;
    [SerializeField] float rateOverTimeIncrease = 5f;

    [Header("Settings")]
    [SerializeField] string stopKeyName;
    [SerializeField] int trials;
    [SerializeField] float minSize = 30f; // Minimum width
    [SerializeField] float maxSize = 50f; // Maximum width
    [SerializeField] float cooldownTime = 0.2f;
    public float speed = 175f;  // Arrow movement speed
    public float trialSpeedIncrease = 1.15f;  // Arrow movement speed
    private bool movingRight = true;
    private bool isStopped = false;
    private int currentTrial;
    bool canPress;
    bool finishedCatching;

    //void Start()
    //{
    //    SetRandomSuccessZone(); // Set initial position & size
    //}


    void IncreaseParticleStats(GameObject ob)
    {
        if (ob.TryGetComponent(out ParticleSystem particleSystem))
        {
            var main = particleSystem.main;
            var emission = particleSystem.emission;
            main.startSize = main.startSize.constant + sizeIncrease;
            main.simulationSpeed += simulationSpeedIncrease;
            main.startLifetime = main.startLifetime.constant + lifetimeIncrease;
            emission.rateOverTime = emission.rateOverTime.constant + rateOverTimeIncrease;
        }
    }

    public void InitialisePowerMeter(float scaleIncrease,float monsterDifficulty, float speedIncrease, int newTrials, float newTrialSpeedIncrease, int monsterLevel)
    {
        isStopped = false;
        finishedCatching = false;
        currentTrial = 0;
        canPress = false;
        minSize = (minSizeDefault / (1 + ((monsterDifficulty + monsterLevel) / 100))) * scaleIncrease;
        maxSize = (maxSizeDefault / (1 + ((monsterDifficulty + monsterLevel) / 100))) * scaleIncrease;
        speed = speedDefault * speedIncrease ;
        trials = newTrials;
        trialSpeedIncrease =  newTrialSpeedIncrease + ((monsterDifficulty + monsterLevel) / 1000);
        SetRandomSuccessZone();
    }

    void Update()
    {
        if (finishedCatching == true)
            return;


        if (!isStopped)
        {
            MoveArrow();
        }

        if (Input.GetButtonDown("Interact") && canPress == false)
        {
            canPress = true;
            StopArrow();
            CatchingManager.i.StartCoroutine(resetCooldown());
        }
    }
    IEnumerator resetCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        canPress = false;
    }
    void MoveArrow()
    {
        Vector3 position = arrow.localPosition;

        if (movingRight)
            position.x += speed * Time.deltaTime;
        else
            position.x -= speed * Time.deltaTime;

        // Reverse direction at edges
        float halfBarWidth = bar.rect.width / 2;
        if (position.x >= halfBarWidth - (arrow.rect.width / 2))
            movingRight = false;
        else if (position.x <= -halfBarWidth + (arrow.rect.width / 2))
            movingRight = true;

        arrow.localPosition = position;
    }

    float successL;
    float successR;


    void StopArrow()
    {
        isStopped = true;

        // Get arrow and success zone boundaries
        float arrowLeft = arrow.position.x;// - (arrow.rect.width / 2);
        //float arrowRight = arrow.localPosition.x + (arrow.rect.width / 2);

        RectTransform rt = successZone.GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        float successLeft = corners[0].x; // bottom-left corner
        float successRight = corners[2].x; // top-right corner

       // float successLeft = (successZone.localPosition.x - (successZone.rect.width / 2));
        //float successRight = (successZone.localPosition.x + (successZone.rect.width / 2));
        successL = successLeft;
        successR = successRight;

        // **NEW: Arrow must be FULLY inside the success zone**
        if (arrowLeft >= successLeft && arrowLeft <= successRight)
        {
           
            // Add success logic here (e.g., bonus, power-up)
            //if current trials is less than total trials, move to next trial
            if (currentTrial < trials)
            {
                OnSuccessNextTrialEvent?.Invoke();
                currentTrial++;
                CatchingManager.i.GetCatchingPowerMeterOb().transform.DOPunchScale(new Vector3(1f + (currentTrial/trials), 1f + (currentTrial / trials)), 0.2f);
                //ResetMeter();
                CatchingManager.i.GetTrialCounterOb().GetComponent<TextMeshProUGUI>().text = currentTrial+"/" + trials;
            }
            else//if current trials is bigger or equal to number of trials, finish and catch monster
            {
                OnSuccessEvent?.Invoke();
                finishedCatching = true;
            }
        }
        else
        {
            OnFailEvent?.Invoke();
            finishedCatching = true;
            //Debug.Log("Failed! Arrow is not fully inside the green area.");
            // Add failure logic here (e.g., weak attack, miss)
        }

        // Reset after a delay
        //Invoke("ResetMeter", 1.5f);
    }

    public void ResetMeter() 
    {
        isStopped = false;
        SetRandomSuccessZone();
    }
    public void NextTrialEvent()//added as event in OnSuccessNextTrialEvent
    {
        isStopped = false;
        SetRandomSuccessZone();
        speed *= trialSpeedIncrease;
        IncreaseParticleStats(CatchingManager.i.GetCurrentCatchOB().transform.GetChild(0).gameObject);
    }

    void SetRandomSuccessZone()
    {
        float halfBarWidth = bar.rect.width / 2;

        // **Set a random width for the success zone** (keep within limits)
      
        float randomWidth = Random.Range(minSize, maxSize);

        // **Calculate max allowed position** so it stays inside the bar
        float maxX = halfBarWidth - (randomWidth / 2);
        float randomX = Random.Range(-maxX, maxX);

        // **Apply new size and position**
        successZone.sizeDelta = new Vector2(randomWidth, successZone.sizeDelta.y);
        successZone.localPosition = new Vector3(randomX, successZone.localPosition.y, 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
public class GameUtils : MonoBehaviour
{
    public static GameUtils i;
    public GameObject InteractDisplayOb;
    public GameObject player;
    public GameObject mainCamera;
    public GameObject virtualCamera;
    public string AlwaysActiveScene;
    public string CurrentScene;
    [SerializeField] FootstepByTileDataMultiScene footstepData;
    [SerializeField] GameObject hitParticle;
    [SerializeField] List<GameObject> clashParticle;
    [SerializeField] GameObject DamageIndicator;
    [SerializeField] GameObject questionMarkOb;
    public Material defaultMaterial;

    [Header("Missing Monster")]//used for empty monster slots
    public BattleMonster missingMon;

    [Header("Cutscenes")]
    [SerializeField] bool isInCutscene;

    [Header("Player Party")]
    public BattleUnit playerUnit;
    public BattleSystem battleSystem;
    public DialogueSystem dialogueSystem;
    public TeleportScript currentCaveEntrance;
    public EnterNextZone currentNextZone;
    public Vector2 LastHealedNPCPosition;
    [Header("Status Conditions")]
    public Sprite _BurnIcon;
    public Sprite _DrownIcon;
    public Sprite _FreezeIcon;
    public Sprite _SleepIcon;
    [Header("Buttons")]
    [SerializeField] List<GameObject> ControllerButtonsDisplay = new List<GameObject>();
    public Sprite _Keyboard_A;
    public Sprite _Keyboard_W;
    public Sprite _Keyboard_S;
    public Sprite _Keyboard_D;
    public Sprite _Keyboard_E;
    public Sprite _Keyboard_Shift;
    public Sprite _Keyboard_Space;
    public Sprite _Controller_Square;
    public Sprite _Controller_Circle;
    public Sprite _Controller_Triangle;
    public Sprite _Controller_Cross;

    bool controllerConnected;
    bool controllerStateInitialised;

    public Color healthBarFullHPColor;
    public Color healthBarHalfHPColor;
    public Color healthBarQuarterHPColor;
    
    private Camera mainCameraComponent;
    public Camera MainCamera
    {
        get
        {
            if (mainCameraComponent == null)
                mainCameraComponent = Camera.main;

            return mainCameraComponent;
        }
    }
    private float controllerPollTimer;
    private const float ControllerPollInterval = 0.5f;

    public bool IsInCutscene()
    {
        return isInCutscene;
    }
    public void SetInCutscene(bool newc)
    {
        isInCutscene = newc;
    }
    public FootstepByTileDataMultiScene getFootstepData()
    {
        return footstepData;
    }
    public void DisplayInteractOb()
    {
        InteractDisplayOb.SetActive(true);
        if (controllerConnected == true)
            InteractDisplayOb.GetComponent<SpriteRenderer>().sprite = _Controller_Cross;
        else
            InteractDisplayOb.GetComponent<SpriteRenderer>().sprite = _Keyboard_E;
    }

    public bool IsSuperEffective(ElementType attack, ElementType defend)
    {
        bool found = false;
        if (TypeChart.GetEffectiveness(attack, defend) >= 2)
        {
            found = true;
        }
        return found;
    }

    public List<GameObject> GetControllerButtons()
    {
        return ControllerButtonsDisplay;
    }
    void FixedUpdate()
    {
        controllerPollTimer -= Time.fixedDeltaTime;
        if (controllerPollTimer > 0f)
            return;

        controllerPollTimer = ControllerPollInterval;
        UpdateControllerState();
    }
    public bool ControllerConnected()
    {
        return controllerConnected;
    }

    public bool HasControllerAimInput()
    {
        if (!controllerConnected)
            return false;

        Vector2 aim = new Vector2(Input.GetAxisRaw("Right Joystick X"), Input.GetAxisRaw("Right Joystick Y"));
        return aim.sqrMagnitude > 0.04f;
    }
    
    /* NOT USED
    private void UpdateControllerState()
    {
        bool connected = false;
        string[] names = Input.GetJoystickNames();
        for (int x = 0; x < names.Length; x++)
        {
            if (!string.IsNullOrEmpty(names[x]))
            {
                connected = true;
                break;
            }
        }

        if (controllerStateInitialised && controllerConnected == connected)
            return;

        controllerStateInitialised = true;
        controllerConnected = connected;
        foreach (GameObject item in ControllerButtonsDisplay)
        {
            if (item != null)
                item.SetActive(controllerConnected);
        }
    }
*/
    private void UpdateControllerState()
    {
        bool connected = false;
        string[] names = Input.GetJoystickNames();
        for (int x = 0; x < names.Length; x++)
        {
            if (!string.IsNullOrEmpty(names[x]))
            {
                connected = true;
                break;
            }
        }

        if (controllerStateInitialised && controllerConnected == connected)
            return;

        controllerStateInitialised = true;
        controllerConnected = connected;
        foreach (GameObject item in ControllerButtonsDisplay)
        {
            if (item != null)
                item.SetActive(controllerConnected);
        }
    }


    public void spawnExclamationMarkOB(Transform transform)
    {
        GameObject ob = Instantiate(questionMarkOb, new Vector2(transform.position.x, transform.position.y + 1.5f), Quaternion.identity, transform);
        AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(AudioSystem.i.TrainerAlertSounds), AudioMaster.SFX);
        Destroy(ob, .3f);
    }
    public GameObject getHitParticle()
    {
        return hitParticle;
    }
    public List<GameObject> getClashParticles()
    {
        return clashParticle;
    }
    public GameObject getDamageIndicator()
    {
        return DamageIndicator;
    }

    public GameObject SpawnDamageIndicator(Vector3 position, float damage, float destroyTime, float scale, Color color, bool shake)
    {
        GameObject ob = Instantiate(getDamageIndicator(), new Vector3(position.x, position.y + 0.7f, getDamageIndicator().transform.position.z), Quaternion.identity, UIManager.i.GetMiscObjectsParent().transform);
        Camera cam = battleSystem.BattleArena.GetComponentInChildren(typeof(Camera)) as Camera;
        ob.GetComponent<RectTransform>().position = cam.WorldToScreenPoint(new Vector3(position.x,position.y + 0.7f,position.z));
        ob.GetComponent<TextMeshProUGUI>().text = "-"+Mathf.RoundToInt(damage).ToString();
        ob.GetComponent<TextMeshProUGUI>().color = color;
        ob.transform.DOScale(scale, destroyTime);
        if (shake == true)
        {
            ob.transform.DOShakePosition(destroyTime, 5);
        }
        Destroy(ob, destroyTime);
        return ob;
    }
    public void SpawnStatChangeIndicator(Vector3 position, float stat, float destroyTime, float scale, Color color, bool shake, string prefix, string suffix)
    {
        GameObject ob = Instantiate(getDamageIndicator(), new Vector3(position.x, position.y, getDamageIndicator().transform.position.z), Quaternion.identity, UIManager.i.GetMiscObjectsParent().transform);
        Camera cam = battleSystem.BattleArena.GetComponentInChildren(typeof(Camera)) as Camera;
        ob.GetComponent<RectTransform>().position = cam.WorldToScreenPoint(position);
        ob.GetComponent<TextMeshProUGUI>().text = prefix + Mathf.RoundToInt(stat).ToString() + "% " + suffix;
        ob.GetComponent<TextMeshProUGUI>().color = color;
        ob.transform.DOScale(scale, destroyTime);
        if (shake == true)
        {
            ob.transform.DOShakePosition(destroyTime, 5);
        }
        Destroy(ob, destroyTime);
    }
    public void SpawnStatusConditionAppliedIndicator(Vector3 position, Status status, float destroyTime, float scale, Color color, bool shake, string prefix, string suffix)
    {
        GameObject ob = Instantiate(getDamageIndicator(), new Vector3(position.x, position.y, getDamageIndicator().transform.position.z), Quaternion.identity, UIManager.i.GetMiscObjectsParent().transform);
        Camera cam = battleSystem.BattleArena.GetComponentInChildren(typeof(Camera)) as Camera;
        ob.GetComponent<RectTransform>().position = cam.WorldToScreenPoint(position);
        ob.GetComponent<TextMeshProUGUI>().text = prefix + status.ToString() + " " + suffix;
        ob.GetComponent<TextMeshProUGUI>().color = color;
        ob.transform.DOScale(scale, destroyTime);
        if (shake == true)
        {
            ob.transform.DOShakePosition(destroyTime, 3);
        }
        Destroy(ob, destroyTime);
    }

    private void Start()
    {
       
        //initialise party
        for (int i = 0; i < PlayerStats.i.PlayerParty.Count; i++)
        {
            PlayerStats.i.PlayerParty[i].Initialise();
        }
        missingMon.Initialise();
        getAliveMonster();
        UIManager.i.PartyScene.GetComponent<PartyScript>().InitialiseParty();
        UIManager.i.PartyScene.GetComponent<PartySelectionScript>().activateTempMenuContent();
        
        //TODO remove later
        ChangeHealedNPCPosition();
    }
    
    public void ChangeHealedNPCPosition() => LastHealedNPCPosition = player.transform.position;

    public void HealTeam(List<BattleMonster> party1)
    {
        for (int i = 0; i < party1.Count; i++)
        {
            party1[i].HP = party1[i].MaxHP;
        }
    }
    public void HealPlayerTeam()
    {
        for (int i = 0; i < PlayerStats.i.PlayerParty.Count; i++)
        {
            PlayerStats.i.PlayerParty[i].HP = PlayerStats.i.PlayerParty[i].MaxHP;
        }
    }

    public void getAliveMonster()
    {
        BattleMonster temp = PlayerStats.i.PlayerParty[0];
        PlayerStats.i.PlayerParty[0] = battleSystem.NextAliveMonster(PlayerStats.i.PlayerParty, 0, false);
        PlayerStats.i.PlayerParty[PartyRules.GetNextAliveSlot(PlayerStats.i.PlayerParty, 0)] = temp;


        playerUnit.CurrentMonster = PlayerStats.i.PlayerParty[0];
       // playerUnit.GetComponent<SpriteRenderer>().sprite = party[0].Base.frontSprite;
    }
    public bool isNear(Vector2 currentPos, Vector2 targetPos, float distance)
    {
        bool near = false;
        if (Vector2.Distance(currentPos, targetPos) <= distance)
        {
            near = true;
        }
        return near;
    }
    public void SetGameState(GameState state) => battleSystem.SetState(state);

    public void SetNewSelectedUIObject(GameObject ob)
    {
        //StartCoroutine(SelectDelay(ob));
        EventSystem.current.SetSelectedGameObject(ob);
    }
    IEnumerator SelectDelay(GameObject ob)
    {
        yield return new WaitForSeconds(0.1f);
        EventSystem.current.SetSelectedGameObject(ob);
    }
    public void FreezeGame()
    {
        SetGameState(GameState.FREEZE);
        player.GetComponent<PlayerController>().SpawnedFootstep = false;
    }
    public void UnFreezeGame()
    {
        SetGameState(GameState.OVERWORLD);
    }
    public void HealMonster(BattleMonster mon, float hp)
    {
        float currentHP = mon.HP;
        if (currentHP + hp >= mon.MaxHP)
        {
            mon.HP = mon.MaxHP;
        }
        else
        {
            mon.HP += hp;
        }
    }
    public void SelectMonToHeal(Item item)
    {
        battleSystem.OnClickPartyButtonHEAL(item);
    }
    public void SelectMonToLearnMove(Item item)
    {
        battleSystem.OnClickPartyButtonLearnMove(item);
    }
    //returns on top element if there is another element on top
    //returns same element as caster if there is no element to raycast
    //TODO NEEDS FIX WITH HIERARCY SEMI DONE, SOUND NOT WORKING
    public GameObject IsUIElementOnTop(RectTransform element)
    {
        GameObject gb = element.gameObject;
        PointerEventData eventData = new PointerEventData(MonsterStorageManager.i.eventSystem);
        eventData.position = element.position;  // Center position

        List<RaycastResult> results = new List<RaycastResult>();
        UIManager.i.raycaster.Raycast(eventData, results);
        foreach (RaycastResult result in results)
        {
           
            //StorageUnit component = result.gameObject.GetComponent<StorageUnit>();
            // Mask component2 = result.gameObject.GetComponent<Mask>();
            if (result.gameObject != element.gameObject)
            {
                if (result.gameObject.CompareTag("UIDrop") == true)
                {
                    gb = result.gameObject;  // Return the first found component of type T
                }
            }
        }
        return gb;
        // If more than one UI element is detected, it's overlapping with another
        // return results.Count > 1 && results[0].gameObject == element.gameObject;
    }
    private void Awake()
    {
        if (i != null)
        {
            Destroy(i);
        }
        i = this;
        mainCameraComponent = Camera.main;
    }
    public Sprite IdentifyButton(ControllerButtonType type)
    {
        if (type == ControllerButtonType.SQUARE)
            return _Controller_Square;
        else if (type == ControllerButtonType.TRIANGLE)
            return _Controller_Triangle;
        else if (type == ControllerButtonType.CIRCLE)
            return _Controller_Circle;
        else if (type == ControllerButtonType.CROSS)
            return _Controller_Cross;
        else
            return _Controller_Cross;
    }

    public Sprite IdentifyStatusConditionSprite(Status condition)
    {
        Sprite sp = null;
        switch (condition)
        {
            case Status.NONE:
                sp = null;
                break;
            case Status.DROWN:
                sp = _DrownIcon;
                break;
            case Status.BURN:
                sp = _BurnIcon;
                break;
            case Status.SLEEP:
                sp = _SleepIcon;
                break;
            case Status.FREEZE:
                sp = _FreezeIcon;
                break;
        }
        return sp;
    }
    

    public void PlayAnimationFromMove(string moveName, GameObject moveAnimationHolder, BattleUnit AttackerUnit, BattleUnit TargetUnit, bool enemyAttacking)
    {
        PlayAnimationFromMove(AttackerUnit.CurrentMove, moveAnimationHolder, AttackerUnit, TargetUnit, enemyAttacking);
    }

    public void PlayAnimationFromMove(BattleMove move, GameObject moveAnimationHolder, BattleUnit AttackerUnit, BattleUnit TargetUnit, bool enemyAttacking)
    {
        if (move == null || move.Base == null || moveAnimationHolder == null || AttackerUnit == null || TargetUnit == null)
            return;

        GameObject ob = Instantiate(moveAnimationHolder, transform.position, Quaternion.identity);
        MoveAnimationHolder newAnim = ob.GetComponent<MoveAnimationHolder>();
        MoveContext context = new MoveContext(AttackerUnit, TargetUnit, move, enemyAttacking);

        newAnim.EnemyAttacking = enemyAttacking;
        newAnim.PlayerPosition = AttackerUnit.transform.position;
        newAnim.EnemyPosition = TargetUnit.gameObject;
        newAnim.CurrentMove = move;
        newAnim.Context = context;

        switch (GetCastPattern(move))
        {
            case MoveCastPattern.BurstProjectile:
                MoveAnimationManager.i.StartCoroutine(MoveAnimationManager.i.PlayShootMultipleParticlesAnimation(newAnim));
                break;
            case MoveCastPattern.SingleProjectileNoChildRotation:
                MoveAnimationManager.i.PlayShootSingleParticleNotRotateAnimation(newAnim);
                break;
            case MoveCastPattern.StatusProjectile:
                MoveAnimationManager.i.PlayShootSingleStatusParticleAnimation(newAnim);
                break;
            case MoveCastPattern.DashStrike:
                MoveAnimationManager.i.StartCoroutine(MoveAnimationManager.i.PlayStrikeForwardAnimation(newAnim, GetDashStrikeLifetime(move)));
                break;
            case MoveCastPattern.DoubleDashStrike:
                MoveAnimationManager.i.StartCoroutine(MoveAnimationManager.i.PlayStrikeDoubleAnimation(newAnim));
                break;
            case MoveCastPattern.Laser:
                MoveAnimationManager.i.PlayShootLaserAnimation(newAnim);
                break;
            case MoveCastPattern.SelfAura:
                MoveAnimationManager.i.StartCoroutine(MoveAnimationManager.i.PlayStatusMoveAnimation(newAnim));
                break;
            default:
                MoveAnimationManager.i.PlayShootSingleParticleAnimation(newAnim);
                break;
        }
    }

    private MoveCastPattern GetCastPattern(BattleMove move)
    {
        if (move.Base.CastPattern != MoveCastPattern.LegacyAuto)
            return move.Base.CastPattern;

        string moveName = move.Base.Name;

        if (IsLegacyBurstMove(moveName))
            return MoveCastPattern.BurstProjectile;
        if (IsSameMoveName(moveName, "Turbulence"))
            return MoveCastPattern.SingleProjectileNoChildRotation;
        if (IsLegacyStatusProjectileMove(moveName))
            return MoveCastPattern.StatusProjectile;
        if (IsLegacyDashStrikeMove(moveName))
            return MoveCastPattern.DashStrike;
        if (IsSameMoveName(moveName, "Volt Strike"))
            return MoveCastPattern.DoubleDashStrike;
        if (IsSameMoveName(moveName, "Electron Beam"))
            return MoveCastPattern.Laser;

        return MoveCastPattern.SingleProjectile;
    }

    private float GetDashStrikeLifetime(BattleMove move)
    {
        if (IsSameMoveName(move.Base.Name, "TakeDown"))
            return 0.3f;

        return 0.2f;
    }

    private bool IsLegacyBurstMove(string moveName)
    {
        return IsSameMoveName(moveName, "Flare")
            || IsSameMoveName(moveName, "Thundershock")
            || IsSameMoveName(moveName, "Thunder")
            || IsSameMoveName(moveName, "Tsunami")
            || IsSameMoveName(moveName, "Bubbles")
            || IsSameMoveName(moveName, "Flamethrower")
            || IsSameMoveName(moveName, "Sharp Leaf")
            || IsSameMoveName(moveName, "Ice Spikes")
            || IsSameMoveName(moveName, "Ice Spray")
            || IsSameMoveName(moveName, "Dark Pool")
            || IsSameMoveName(moveName, "Protect")
            || IsSameMoveName(moveName, "Power Up")
            || IsSameMoveName(moveName, "Meditate")
            || IsSameMoveName(moveName, "Rock Spikes")
            || IsSameMoveName(moveName, "Rocky Combo");
    }

    private bool IsLegacyStatusProjectileMove(string moveName)
    {
        return IsSameMoveName(moveName, "Ember")
            || IsSameMoveName(moveName, "Jasmine Flower")
            || IsSameMoveName(moveName, "Freezing Cloud");
    }

    private bool IsLegacyDashStrikeMove(string moveName)
    {
        return IsSameMoveName(moveName, "Tackle")
            || IsSameMoveName(moveName, "TakeDown");
    }

    private bool IsSameMoveName(string moveName, string expected)
    {
        return string.Equals(moveName, expected, System.StringComparison.OrdinalIgnoreCase);
    }

    public void ShakeCamera(float duration, float strength = 3, int vibrato = 10, bool changeFOV = false)
    {
        //Camera cam = GameUtils.i.battleSystem.BattleArena.GetComponentInChildren(typeof(Camera)) as Camera;
        Camera cam = battleSystem.BattleCameraBehaviour.BattleCamera;
        var sequence = DOTween.Sequence();
        cam.DOShakePosition(duration, strength, vibrato);
        if (changeFOV == true)
        {
            
            sequence.Append(MainCamera.DOOrthoSize(10f, duration/3));
            sequence.Append(MainCamera.DOOrthoSize(10f, duration / 3));
            sequence.Append(MainCamera.DOOrthoSize(12f, duration/3));
        }
    }
}

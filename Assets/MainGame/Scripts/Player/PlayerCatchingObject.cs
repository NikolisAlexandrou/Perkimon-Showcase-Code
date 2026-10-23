using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCatchingObject : MonoBehaviour
{

    [Header("General Properties")]
    [SerializeField] private GameObject _monsterObject;
    [SerializeField] private GameObject _scratchObject;
    [SerializeField] private GameObject _punchObject;
    [SerializeField] private GameObject _currentCatchingObject;
    [SerializeField] private float _despawnTime = 1f;

    [Header("Item Drops")]
    [SerializeField] private Item _itemWood;
    [SerializeField] private Item _itemRockDust;

    private float _currentDespawntimer;
    private bool _isFollowingPlayer;
    private bool _hasCollided;
    private PlayerController _player;
    private Sprite _currentCatchingObjectSprite;

    private void Start()
    {
        _currentCatchingObjectSprite = _currentCatchingObject.GetComponent<SpriteRenderer>().sprite;
    }
    
    private void FixedUpdate()
    {
        if (_isFollowingPlayer == true)
        {
            gameObject.transform.DOMove(new Vector2(_player.transform.position.x, _player.transform.position.y - 0.25f), .2f);
            if (GameUtils.i.isNear(gameObject.transform.position, _player.transform.position, 1f))
            {
                _player.FollowingMonster.GetComponent<MonsterFollowScript>().PlayThrowAnimation(_player.FollowingMonster);
                _player.StartCoroutine(_player.ResetThrowCooldown());
                Destroy(gameObject);
            }
        }
        if (_hasCollided == false)
        {
            _currentCatchingObject.transform.Rotate(Vector3.forward, 1200 * Time.deltaTime);
            GetComponent<Rigidbody2D>()?.AddForce(transform.right * _player.TemporaryThrowCatchingObjectSpeed, ForceMode2D.Impulse);
            if (_currentDespawntimer <= _despawnTime)
                _currentDespawntimer += Time.deltaTime;
            else
                _isFollowingPlayer = true;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Tree") && _currentDespawntimer < _despawnTime - 0.1f && _isFollowingPlayer == false)
        {
            _hasCollided = true;
            StartCoroutine(SpawnMonster(collision.gameObject));
            PlayMoveAnimation(collision.gameObject, _scratchObject, 1, 45, false);
            _itemWood.Initialise();
            BagManager.i.AddItemToBag(_itemWood);
            AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(AudioSystem.i.TreeBreakSounds), AudioMaster.SFX);
        }
        else if (collision.CompareTag("Rock") && _currentDespawntimer < _despawnTime - 0.1f && _isFollowingPlayer == false)
        {
            _hasCollided = true;
            StartCoroutine(SpawnMonster(collision.gameObject));
            PlayMoveAnimation(collision.gameObject, _punchObject, 0, 0, true);
            _itemRockDust.Initialise();
            BagManager.i.AddItemToBag(_itemRockDust);
            AudioSystem.i.PlaySoundClip(AudioSystem.i.PlayRandomSound(AudioSystem.i.BoulderBreakSounds), AudioMaster.SFX);
        }
        else if (collision.CompareTag("Monster") && _currentDespawntimer < _despawnTime - 0.1f && _isFollowingPlayer == false)
        {
            _hasCollided = true;
            _player.StartBattleMonster(collision);
            _isFollowingPlayer = true;
        }

    }
    public void setPlayer(PlayerController pl) => _player = pl;

    public void SetSprite(Sprite sp) => _currentCatchingObject.GetComponent<SpriteRenderer>().sprite = sp;
    
    private void PlayMoveAnimation(GameObject target, GameObject objectToSummon, float offset, float rotation, bool scale)
    {
        GameObject ob = Instantiate(objectToSummon, new Vector2(target.transform.position.x, target.transform.position.y + offset), Quaternion.Euler(0, 0, rotation), target.transform);
        ob.GetComponent<SpriteRenderer>().DOFade(0, 0.4f);
        if (scale == true)
            ob.transform.DOScale(new Vector3(1.2f, 1.2f, 0), 0.1f);


        target.GetComponent<SpriteRenderer>().DOFade(0, 0.7f);
        //StartCoroutine(playHurtAnimation(enemyPos.GetComponent<Image>(), 0.4f, Color.red));
        Destroy(ob, 0.4f);
    }

    private IEnumerator SpawnMonster(GameObject collider)
    {
        GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
        GameObject mon = Instantiate(_monsterObject, transform.position, Quaternion.identity);
        Instantiate(PlayerStats.i.PlayerParty[0].CapsuleCaught.Capsule_Particle, transform.position, Quaternion.identity);
        mon.GetComponent<SpriteRenderer>().DOFade(0, 0);
        mon.transform.localScale = new Vector3(0, 0, 0);
        mon.GetComponent<SpriteRenderer>().sprite = PlayerStats.i.PlayerParty[0].Base.overworldSprites[0];
        setSpriteFromRotation(mon);
        _player.FollowingMonster.GetComponent<MonsterFollowScript>().PlayThrowAnimation(mon);
        _currentCatchingObject.GetComponent<SpriteRenderer>().sprite = null;

        yield return new WaitForSeconds(0.7f);
        Destroy(collider);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(_player.FollowingMonster.GetComponent<MonsterFollowScript>().PlayDestroyAnimation(mon));
        yield return new WaitForSeconds(_player.FollowingMonster.GetComponent<MonsterFollowScript>().InDuration);
        Destroy(mon);
        _currentCatchingObject.GetComponent<SpriteRenderer>().sprite = _currentCatchingObjectSprite;
        _isFollowingPlayer = true;


    }
    private void setSpriteFromRotation(GameObject monster)
    {
        if (transform.eulerAngles.z >= -45 && transform.eulerAngles.z <= 45)
            monster.GetComponent<SpriteRenderer>().sprite = PlayerStats.i.PlayerParty[0].Base.overworldSprites[2];
        else if (transform.eulerAngles.z >= 135 || transform.eulerAngles.z <= -135)
            monster.GetComponent<SpriteRenderer>().sprite = PlayerStats.i.PlayerParty[0].Base.overworldSprites[1];
        else if (Mathf.Abs(transform.eulerAngles.z) > 45 && Mathf.Abs(transform.eulerAngles.z) < 135)
            monster.GetComponent<SpriteRenderer>().sprite = PlayerStats.i.PlayerParty[0].Base.overworldSprites[3];
        else
            monster.GetComponent<SpriteRenderer>().sprite = PlayerStats.i.PlayerParty[0].Base.overworldSprites[0];
    }
}

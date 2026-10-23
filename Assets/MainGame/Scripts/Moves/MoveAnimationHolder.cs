using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class MoveAnimationHolder : MonoBehaviour
{
    private BattleMove _currentMove; //  Current using mvoe
    private MoveContext _context;
    private GameObject _enemyPosition;    //  Holds the game object of the enemy to get its positions and rotations
    private Vector3 _playerPosition;  //  Get the player position

    //  For moves that shoot multiple projectiles, keeps the rotation of first projectile to add to all of them
    private Quaternion _previousRotation = Quaternion.Euler(0, 0, 0);
    private bool _enemyAttacking = false;    //  Get if enemy used the attack
    private int _projectileCount;  //  Get the number of projectiles used

    /// <summary>
    /// Get the list that contains all sounds that can be played when shooting the move
    /// </summary>
    public List<AudioClip> ShootSoundEffect => _currentMove.Base.ShootSoundEffect;

    /// <summary>
    /// Get the list that contains all sounds that can be played when move hits a target
    /// </summary>
    public List<AudioClip> HitSoundEffect => _currentMove.Base.HitSoundEffect;

    /// <summary>
    /// Get the list that contains all sounds that can be played when move hits a wall
    /// or is destroyed naturally
    /// </summary>
    public List<AudioClip> DestroySoundEffect => _currentMove.Base.DestroySoundEffect;

    /// <summary>
    /// Get or set if the attack comes from an enemy
    /// </summary>
    public bool EnemyAttacking { get => _enemyAttacking; set => _enemyAttacking = value; }

    /// <summary>
    /// Get or set the current move used
    /// </summary>
    public BattleMove CurrentMove { get => _currentMove; set => _currentMove = value; }

    public MoveContext Context { get => _context; set => _context = value; }

    /// <summary>
    /// Get the rotation of first projectile thrown in same move
    /// </summary>
    public Quaternion PreviousRotation { get => _previousRotation; set => _previousRotation = value; }

    /// <summary>
    /// Get or set the player position
    /// </summary>
    public Vector3 PlayerPosition { get => _playerPosition; set => _playerPosition = value; }

    /// <summary>
    /// Get or set the enemy game object to get its position
    /// </summary>
    public GameObject EnemyPosition { get => _enemyPosition; set => _enemyPosition = value; }

    /// <summary>
    /// Get or set the count of projectiles thrown from a single move use
    /// </summary>
    public int ProjectileCount { get => _projectileCount; set => _projectileCount = value; }
  
}

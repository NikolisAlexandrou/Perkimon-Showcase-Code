using System;
using UnityEngine;

public class BattleCameraBehaviour : MonoBehaviour
{
   [Tooltip("Holds reference to the battle system")]
   [SerializeField] private BattleSystem _battleSystem;

   [Header("Camera Settings")] 
   [SerializeField] private bool _useDynamicCamera = true;
   
   [SerializeField] private float _minDistanceZoom = 5;
   [SerializeField] private float _maxDistanceZoom = 15;
   
   [SerializeField] private float _minCameraZoom = 7;
   [SerializeField] private float _maxCameraZoom = 12;
   
   private Camera _battleCamera;
   private BattleUnit _playerUnit;
   private BattleUnit _enemyUnit;

   private bool _interruptCamera = false;

   /// <summary>
   /// True if dynamic camera calculations need to be interrupted for something else to finish first
   /// </summary>
   public bool InterruptCamera { get => _interruptCamera; set => _interruptCamera = value; }

   public Camera BattleCamera { get => _battleCamera; set => _battleCamera = value; }
   
   private void Start()
   {
      _interruptCamera = false;
      AssignBattleValues();

      if (_battleCamera == null)
         return;
      
      // reset the camera position to start position after each battle
      _battleCamera.transform.localPosition = new  Vector3(0, 0, _battleCamera.transform.localPosition.z);
   }

   private void LateUpdate()
   {
      if (_battleCamera == null)
         return;
      if (_useDynamicCamera == false)
         return;
      if (_interruptCamera == true)
         return;
      
      CalculateCameraPosition();
   }

   /// <summary>
   /// Assign the current camera used in battle to camera here
   /// </summary>
   public void AssignCamera()
   {
      _battleCamera = _battleSystem.BattleArena.GetComponentInChildren(typeof(Camera)) as Camera;
   }

   /// <summary>
   /// Assign the variables needed to calculate camera angles and zooms
   /// </summary>
   private void AssignBattleValues()
   {
      _playerUnit = _battleSystem.PlayerUnit;
      _enemyUnit = _battleSystem.EnemyUnit;
   }

   /// <summary>
   /// Sets the camera position to always be in the middle between
   /// the player and enemy unit
   /// </summary>
   private void CalculateCameraPosition()
   {
      Vector2 playerVector = _playerUnit.transform.position;
      Vector2 enemyVector = _enemyUnit.transform.position;

      // calculate midpoint between player and enemy and set it to camera position
      Vector2 differenceVector = playerVector + enemyVector;
      float distance = Vector2.Distance(playerVector, enemyVector);
      
      _battleCamera.transform.parent.position = new Vector3(differenceVector.x/2,differenceVector.y/2,_battleCamera.transform.parent.position.z);
      CalculateCameraZoom(distance);
   }

   private void CalculateCameraZoom(float distance)
   {
      float proportion = Mathf.InverseLerp(_minDistanceZoom, _maxDistanceZoom, distance);
      float x = Mathf.Lerp(_minCameraZoom, _maxCameraZoom, proportion);

      _battleCamera.orthographicSize = x;
   }
   
}

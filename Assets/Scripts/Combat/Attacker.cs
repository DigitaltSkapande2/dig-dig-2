using System.Collections.Generic;
using DigDig2.Debugging;
using DigDig2.Entity;

using UnityEngine;

namespace DigDig2.Combat
{
	[RequireComponent( typeof( EntityCharacterController ) )]
	public class Attacker : MonoBehaviour
	{
		public enum CombatState
		{
			Idle,
			Performing,
			Charging,
			FullyCharged,
			OnCooldown
		}

		[Tooltip( "The attack types that this entity has access to." )]
		[SerializeField] private List<AttackType> attackTypes;

		[Tooltip( "The time after the last attack was finished where you can attack again to trigger a chain." )]
		[SerializeField] private float chainTimeWindowAfterAttackEnd = 0.05f;

		[Tooltip( "The time before the last attack is going to finish where you can attack again to trigger a chain." )]
		[SerializeField] private float chainTimeWindowBeforeAttackEnd = 0.5f;

		[Tooltip( "The \"anchor points\" that an attack can use to create hitboxes." )]
		[SerializeField] private List<BindableAttackHitbox> bindableAttackHitboxes = new( );

		[Tooltip( "The groups of this entity's enemies" )]
		[SerializeField] private List<string> enemyGroups = new( );

		[SerializeField] private bool verboseLogging;

		public readonly Dictionary<string, BindableAttackHitbox> activeAttacks = new( );

		private Animator animator;
		private float attackCooldownTimer;
		private int attackRequestChain;
		private bool attackRequestProcessed = true;
		private TrailRenderer attackTrailRenderer;

		private float chargeStartTime = -1;

		private int currentAttackChain;
		private Attack currentPerformingAttack;

		private AttackType currentPerformingAttackType;
		private EntityCharacterController entityCharacterController;

		private Attackable focusedEnemy;

		private AttackType heldAttackType;
		private Attack lastPerformedAttack;
		private AttackType lastPerformedAttackType;
		private float performanceStartTime = -1;

		[SerializeField] CombatState combatState;

		public CombatState State { get; private set; } = CombatState.Idle;

		private void Awake( )
		{
			animator = GetComponentInChildren<Animator>( );
			if ( bindableAttackHitboxes[ 0 ] ) bindableAttackHitboxes[ 0 ].TryGetComponent( out attackTrailRenderer );
			TryGetComponent( out entityCharacterController );
		}

		private void Update( )
        {
			combatState = State;
			if ( ( State == CombatState.Idle || attackRequestChain > 0 ) && !attackRequestProcessed )
			{
				if ( heldAttackType )
				{
					currentAttackChain = attackRequestChain;

					if ( heldAttackType.chargeMode == AttackType.ChargeMode.NoCharge )
						PerformAttack( heldAttackType );
					else
						ChargeAttack( heldAttackType );
				}

				attackRequestProcessed = true;
				attackRequestChain = -1;
			}

			switch ( State )
			{
				case CombatState.Charging:
				{
					currentPerformingAttack.Charge( this, currentPerformingAttackType, Mathf.Clamp( Time.time - chargeStartTime, 0, currentPerformingAttackType.chargeDuration ) );
					if ( Time.time - chargeStartTime >= currentPerformingAttackType.chargeDuration && chargeStartTime > 0 )
					{
						State = CombatState.FullyCharged;
						currentPerformingAttack.ChargeFull( this, currentPerformingAttackType );
					}

					break;
				}
				case CombatState.Performing:
				{
					if ( Time.time - performanceStartTime >= currentPerformingAttack.AttackDuration ) 
					{
						EndAttack( true );
					}

					break;
				}
				case CombatState.Idle:
				case CombatState.FullyCharged:
				case CombatState.OnCooldown:
				default: break;
			}

			if ( attackCooldownTimer > 0 ) attackCooldownTimer -= Time.deltaTime;
			if ( State == CombatState.OnCooldown && attackCooldownTimer <= 0 ) State = CombatState.Idle;
		}

		private void FixedUpdate( )
		{
			foreach ( KeyValuePair<string, BindableAttackHitbox> activeAttack in activeAttacks ) { activeAttack.Value.Trigger( activeAttack.Key ); }
		}

		private void LogVerbose( string message )
		{
			if ( verboseLogging ) Debug.Log( message );
		}

		private void LogWarningVerbose( string message )
		{
			if ( verboseLogging ) Debug.LogWarning( message );
		}

		private AttackType GetAttackTypeFromIndex( int attackTypeIndex )
		{
			if ( !( attackTypes.Count > attackTypeIndex ) )
			{
				Debug.LogError( $"{name} does not have an attack type index of {attackTypeIndex}." );
				return null;
			}

			return attackTypes[ attackTypeIndex ];
		}

		public TrailRenderer GetattackTrailRenderer( ) => attackTrailRenderer;

		#region Chaining

		public bool HasMetChainRequirement( AttackType attackType )
		{
			LogVerbose( "Checking attack chain requirement:" );
			LogVerbose( $"> Is it the same as the last? {lastPerformedAttackType == attackType}" );
			if ( lastPerformedAttackType != attackType ) return false;

			float chainWindowMarginOfError = GetAttackPerformanceTime( ) - lastPerformedAttack.AttackDuration;
			LogVerbose( $"> Is it in the margin of error? {chainWindowMarginOfError <= chainTimeWindowAfterAttackEnd && chainWindowMarginOfError >= -chainTimeWindowBeforeAttackEnd}, MoE: {chainWindowMarginOfError}" );
			if ( !( chainWindowMarginOfError <= chainTimeWindowAfterAttackEnd && chainWindowMarginOfError >= -chainTimeWindowBeforeAttackEnd ) ) return false;

			LogVerbose( $"> Is it at the end of the chain? {currentAttackChain >= attackType.chain.Count - 1} ({currentAttackChain + 1}, {attackType.chain.Count})" );
			if ( currentAttackChain + 1 >= attackType.chain.Count ) return false;

			LogVerbose( "Chain is valid!" );

			return true;
		}

		#endregion

		#region Animation

		public void PlayAnimation( string animationStateName )
		{
			animator.CrossFadeInFixedTime( animationStateName, 0.1f, 0 );
			//animator.CrossFadeInFixedTime( animationStateName, 0.1f, 1 );
		}

		public void AnimationEvent( string animEventName )
		{
			if (!currentPerformingAttack)
			{
				Debug.LogError("There is no preforming attack somehow?");
				return;
			}

			currentPerformingAttack.AnimationEvent( this, currentPerformingAttackType, animEventName );
		}

		#endregion

		#region Input

		public void RequestAttackStart( int attackTypeIndex )
		{
			heldAttackType = GetAttackTypeFromIndex( attackTypeIndex );

			LogVerbose( $"Attack request started. Held Attack Type: {heldAttackType}" );
			bool isValidChain = HasMetChainRequirement( heldAttackType );
			if ( isValidChain )
			{
				attackRequestChain = currentAttackChain + 1;
				LogVerbose( "Incrementing chain!" );
				EndAttack( false, true );
			}
			else
			{
				LogVerbose( "Resetting chain!" );
				attackRequestChain = 0;
			}

			LogVerbose( $"Attack requested! Held Attack Type: {heldAttackType}, Is Chain: {isValidChain}, State: {State}" );
			attackRequestProcessed = false;
		}

		public void RequestAttackEnd( bool ignoreWarnings = false )
		{
			// Perform the Charged attack (if there is one and it's valid)
			if ( IsChargeValid( ) ) PerformAttack( heldAttackType );
			else EndAttackCharge( ignoreWarnings );

			heldAttackType = null;
			attackRequestProcessed = true;
			attackRequestChain = 0;

			LogVerbose( "Attack end requested!" );
		}

		#endregion

		#region Charging

		private void ChargeAttack( AttackType attackType )
		{
			if ( attackType.chargeMode == AttackType.ChargeMode.NoCharge )
			{
				Debug.LogError( $"Attack type \"{attackType.name}\" is not a chargable attack type." );
				return;
			}

			if ( attackType.chargeDuration <= 0 ) Debug.LogWarning( $"Attack type \"{attackType.name}\" is being charged, but has a charge duration of 0 or below" );

			Attack chargingAttack = attackType.GetAttackFromIndex( currentAttackChain );
			chargingAttack.ChargeStart( this, attackType );

			currentPerformingAttackType = attackType;
			currentPerformingAttack = chargingAttack;

			State = CombatState.Charging;
			chargeStartTime = Time.time;
		}

		public void EndAttackCharge( bool ignoreWarning = false )
		{
			if ( !IsChargingAttack( ) )
			{
                if ( !ignoreWarning ) BetterDebug.Log( "Could not cancel attack charge, there is no attack being charged.", LogSeverity.Error );
				return;
			}

			EndAttack( );
			chargeStartTime = -1;
		}

		public bool IsChargingAttack( ) => !Mathf.Approximately( chargeStartTime, -1 );

		public float GetAttackChargeTime( )
		{
			if ( IsChargingAttack( ) ) return Time.time - chargeStartTime;

			LogWarningVerbose( "There is no attack being charged." );
			return -1;
		}

		private bool IsChargeValid( ) => State == CombatState.Charging && currentPerformingAttackType.chargeMode != AttackType.ChargeMode.RequireFullCharge || State == CombatState.FullyCharged;

		#endregion

		#region Attacking

		private void PerformAttack( AttackType attackType = null )
		{
			if ( IsChargingAttack( ) && !attackType ) // Attack is charged, end charge and trigger attack
			{
				if ( IsChargeValid( ) )
					EndAttackCharge( );
				else
				{
					EndAttack( );
					return;
				}
			}
			else if ( attackType ) // Attack is not charged, trigger attack
			{
				LogVerbose( $"Attacking with chain {currentAttackChain}!" );
				currentPerformingAttackType = attackType;
				currentPerformingAttack = currentPerformingAttackType.GetAttackFromIndex( currentAttackChain );
			}
			else
				EndAttack( true );

			if ( !currentPerformingAttack )
			{
				EndAttack( false, true );
				Debug.LogWarning( $"Failed to get attack chain, currentAttackChain = {currentAttackChain}, currentPerformingAttackType.chain.Count = {currentPerformingAttackType.chain.Count}." );
				return;
			}

			currentPerformingAttack.Trigger( this, currentPerformingAttackType, Mathf.Clamp( Time.time - chargeStartTime, 0, currentPerformingAttackType.chargeDuration ) );
			State = CombatState.Performing;
			performanceStartTime = Time.time;

			lastPerformedAttackType = currentPerformingAttackType;
			lastPerformedAttack = currentPerformingAttack;
		}

		public void EndAttack( bool applyCooldown = false, bool ignoreWarning = false )
		{
			if ( !currentPerformingAttackType )
			{
				if ( !ignoreWarning ) BetterDebug.Log( "Can't end attack, no attack is being performed right now.", LogSeverity.Error );
				return;
			}

			if ( applyCooldown ) attackCooldownTimer = currentPerformingAttackType.endCooldown;

			currentPerformingAttackType.chain[ currentAttackChain ].Ended( this, currentPerformingAttackType );
			State = CombatState.OnCooldown;
			currentPerformingAttackType = null;
			currentPerformingAttack = null;

			LogVerbose( "Attack ended!" );
		}

		public bool IsPerformingAttack( ) => currentPerformingAttackType != null;

		public float GetAttackPerformanceTime( ) => Time.time - performanceStartTime;

        public bool CanAttackGroup( string group )
        {
            if ( enemyGroups.Count <= 0 ) return true;
            return enemyGroups.Contains( group );
        }

		#endregion

		#region Attack Hit Detection

		public void StartHitboxAttack( Attack attack, string id, BindableAttackHitbox bindableAttackHitbox )
		{
			bindableAttackHitbox.StartAttack( id, this, attack );
			activeAttacks[ id ] = bindableAttackHitbox;
		}

		public void EndHitboxAttack( string id )
		{
			if ( !activeAttacks.TryGetValue( id, out BindableAttackHitbox attackHitbox ) ) return;

			attackHitbox.EndAttack( id );
			activeAttacks.Remove( id );
		}

		public BindableAttackHitbox GetBindableAttackHitbox( int index ) => bindableAttackHitboxes[ index ];

		#endregion

		#region Character Controller Interfacing

		public void AddMoveSpeedDebuff( string debuffId, float debuff )
		{
			if ( entityCharacterController ) entityCharacterController.AddMoveSpeedDebuff( debuffId, debuff );
		}

		public void RemoveMoveSpeedDebuff( string debuffId )
		{
			if ( entityCharacterController ) entityCharacterController.RemoveMoveSpeedDebuff( debuffId );
		}

		public float GetBaseMoveSpeed( )
		{
			if ( entityCharacterController ) return entityCharacterController.MoveSpeed;

			return 0;
		}

		public void PushInDirection( Vector3 direction, float strength )
		{
			if ( entityCharacterController ) entityCharacterController.PushInDirection( direction, strength );
		}

		public void ApplyKnockback( Vector3 direction, float strength )
		{
			if ( entityCharacterController ) entityCharacterController.ApplyKnockback( direction, strength );
		}

		#endregion
	}
}

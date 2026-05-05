using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{

    public enum Team
    {
        Player,
        Enemy
    }

    public enum ActionState
    {
        Ready,
        Moved,
        Acted
    }

    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private Vector3Int currentCellPosition;

    [SerializeField] private int maxHp = 10;
    [SerializeField] private int currentHp = 10;
    [SerializeField] private int attackPower = 3;
    [SerializeField] private int moveRange = 3;
    [SerializeField] private int attackRange = 1;
    [SerializeField] private int defense = 1;
    [SerializeField] private string unitId;
    [Header("AP Costs")]
    [SerializeField] private int actionAPCost = 1;
    [SerializeField] private int costToSummon = 1;

    [SerializeField] private Team team;

    [SerializeField] private ElementType elementType;

    [SerializeField] private Sprite portrait;
    public Sprite Portrait => portrait;

    [Header("Movement Feedback")]
    [SerializeField] private float moveSpeed = 10f;

    private bool isMoving;
    public bool IsMoving => isMoving;

    [Header("Attack Feedback")]
    [SerializeField] private float lungeDistance = 0.3f;
    [SerializeField] private float lungeSpeed = 10f;

    [Header("Feedback")]
    [SerializeField] private float hitFlashDuration = 0.25f;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private DamageNumber damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 0.8f, 0f);

    [SerializeField] private GridPatternType movePattern = GridPatternType.Diamond;
    [SerializeField] private GridPatternType attackPattern = GridPatternType.Diamond;

    [Header("State Visuals")]
    [SerializeField] private Color actedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private GameObject selectionRing;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private ActionState actionState = ActionState.Ready;

    public string UnitId => unitId;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

    public event Action<Unit, int, int> OnDamageTaken;
    public event Action<Unit> OnUnitDefeated;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        currentHp = maxHp;
        SnapToCell(currentCellPosition);
    }

    public void SnapToCell(Vector3Int cellPosition)
    {
        currentCellPosition = cellPosition;

        Vector3 worldPosition = combatTilemap.GetCellCenterWorld(cellPosition);
        transform.position = worldPosition;
    }

    private IEnumerator MoveToCoroutine(Vector3Int targetCellPosition)
    {
        isMoving = true;

        currentCellPosition = targetCellPosition;

        Vector3 startPosition = transform.position;
        Vector3 targetWorldPosition = combatTilemap.GetCellCenterWorld(targetCellPosition);

        while (Vector3.Distance(transform.position, targetWorldPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWorldPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetWorldPosition;
        isMoving = false;
    }

    public void MoveTo(Vector3Int targetCellPosition)
    {
        StartCoroutine(MoveToCoroutine(targetCellPosition));
    }

    public bool CanMoveTo(Vector3Int targetCellPosition)
    {
        return GridPatternUtility.IsCellInPattern(
            currentCellPosition,
            targetCellPosition,
            moveRange,
            movePattern
        );
    }

    public Vector3Int GetCurrentCellPosition()
    {
        return currentCellPosition;
    }

    public int GetAttackPower()
    {
        return attackPower;
    }

    public int GetMoveRange()
    {
        return moveRange;
    }

    public int GetAttackRange()
    {
        return attackRange;
    }

    public Team GetTeam()
    {
        return team;
    }

    public ElementType GetElementType()
    {
        return elementType;
    }

    public int GetDefense()
    {
        return defense;
    }

    public GridPatternType GetMovePattern()
    {
        return movePattern;
    }

    public GridPatternType GetAttackPattern()
    {
        return attackPattern;
    }

    public int GetActionAPCost()
    {
        return actionAPCost;
    }

    public int GetCostToSummon()
    {
        return costToSummon;
    }

    public bool CanAct()
    {
        return actionState != ActionState.Acted;
    }

    public bool HasMovedThisTurn()
    {
        return actionState == ActionState.Moved;
    }

    public void MarkMoved()
    {
        actionState = ActionState.Moved;
    }

    public void MarkActed()
    {
        actionState = ActionState.Acted;
        UpdateActionStateVisual();
    }

    public void ResetTurnState()
    {
        actionState = ActionState.Ready;
        UpdateActionStateVisual();
    }

    public void SetCombatTilemap(Tilemap tilemap)
    {
        combatTilemap = tilemap;
    }

    public bool IsInAttackRange(Unit otherUnit)
    {
        return GridPatternUtility.IsCellInPattern(
            currentCellPosition,
            otherUnit.GetCurrentCellPosition(),
            attackRange,
            attackPattern
        );
    }

    public int CalculateDamageAgainst(Unit targetUnit)
    {
        float multiplier = ElementSystem.GetMultiplier(
            elementType,
            targetUnit.GetElementType()
        );

        int rawDamage = attackPower - targetUnit.GetDefense();
        int finalDamage = Mathf.RoundToInt(rawDamage * multiplier);

        return Mathf.Max(1, finalDamage);
    }

    public void TakeDamage(int damage)
    {
        int oldHp = currentHp;

        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayHitImpact();
        }

        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHp}/{maxHp}");

        OnDamageTaken?.Invoke(this, oldHp, currentHp);

        ShowDamageNumber(damage);
        StartCoroutine(HitFlash());
    }

    public void ResolveDeathIfNeeded()
    {
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        spriteRenderer.color = hitFlashColor;

        yield return new WaitForSeconds(hitFlashDuration);

        UpdateActionStateVisual();
    }

    private void ShowDamageNumber(int damage)
    {
        if (damageNumberPrefab == null)
        {
            Debug.Log("DamageNumberPrefab is NULL");
            return;
        }

        Vector3 spawnPosition = transform.position + damageNumberOffset;
        spawnPosition.z = 0f;

        DamageNumber damageNumber = Instantiate(
            damageNumberPrefab,
            spawnPosition,
            Quaternion.identity
        );

        damageNumber.Setup(damage);
    }

    public IEnumerator AttackLunge(Vector3 targetWorldPosition)
    {
        Vector3 startPosition = transform.position;

        Vector3 direction = (targetWorldPosition - startPosition).normalized;
        Vector3 lungeTarget = startPosition + direction * lungeDistance;

        // Move forward
        while (Vector3.Distance(transform.position, lungeTarget) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                lungeTarget,
                lungeSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Move back
        while (Vector3.Distance(transform.position, startPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                lungeSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = startPosition;
    }

    private void UpdateActionStateVisual()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        switch (actionState)
        {
            case ActionState.Ready:
                spriteRenderer.color = originalColor;
                break;

            case ActionState.Acted:
                spriteRenderer.color = actedColor;
                break;
        }
    }

    public void SetSelectedVisual(bool selected)
    {
        if (selectionRing != null)
        {
            selectionRing.SetActive(selected);
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} was defeated.");

        OnUnitDefeated?.Invoke(this);

        Destroy(gameObject);
    }

}
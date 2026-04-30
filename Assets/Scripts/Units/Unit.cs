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

    [SerializeField] private Tilemap combatTilemap;
    [SerializeField] private Vector3Int currentCellPosition;

    [SerializeField] private int maxHp = 10;
    [SerializeField] private int currentHp = 10;
    [SerializeField] private int attackPower = 3;
    [SerializeField] private int moveRange = 3;
    [SerializeField] private int attackRange = 1;
    [SerializeField] private int defense = 1;
    [SerializeField] private string unitId;

    [SerializeField] private Team team;

    [SerializeField] private ElementType elementType;

    [SerializeField] private Sprite portrait;
    public Sprite Portrait => portrait;

    [Header("Movement Feedback")]
    [SerializeField] private float moveSpeed = 10f;

    private bool isMoving;
    public bool IsMoving => isMoving;

    [Header("Feedback")]
    [SerializeField] private float hitFlashDuration = 0.25f;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private DamageNumber damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 0.8f, 0f);

    [SerializeField] private GridPatternType movePattern = GridPatternType.Diamond;
    [SerializeField] private GridPatternType attackPattern = GridPatternType.Diamond;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public string UnitId => unitId;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

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
        currentHp -= damage;

        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHp}/{maxHp}");

        ShowDamageNumber(damage);
        StartCoroutine(HitFlash());

        if (currentHp <= 0)
        {
            StartCoroutine(DieAfterDelay());
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

        spriteRenderer.color = originalColor;
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

    private void Die()
    {
        Debug.Log($"{gameObject.name} was defeated.");

        OnUnitDefeated?.Invoke(this);

        Destroy(gameObject);
    }

    private IEnumerator DieAfterDelay()
    {
        yield return new WaitForSeconds(hitFlashDuration);

        Die();
    }
}
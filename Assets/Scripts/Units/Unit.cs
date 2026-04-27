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
    [SerializeField] private string unitId;

    [SerializeField] private Team team;

    [SerializeField] private ElementType elementType;

    [SerializeField] private Sprite portrait;
    public Sprite Portrait => portrait;

    [Header("Feedback")]
    [SerializeField] private float hitFlashDuration = 0.25f;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private DamageNumber damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 0.8f, 0f);

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public string UnitId => unitId;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

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

    public void MoveTo(Vector3Int targetCellPosition)
    {
        SnapToCell(targetCellPosition);
    }

    public bool CanMoveTo(Vector3Int targetCellPosition)
    {
        int distanceX = Mathf.Abs(currentCellPosition.x - targetCellPosition.x);
        int distanceY = Mathf.Abs(currentCellPosition.y - targetCellPosition.y);

        int totalDistance = distanceX + distanceY;

        return totalDistance <= moveRange;
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

    public void SetCombatTilemap(Tilemap tilemap)
    {
        combatTilemap = tilemap;
    }

    // Potentially Obsolete
    public bool IsAdjacentTo(Unit otherUnit)
    {
        Vector3Int otherCell = otherUnit.GetCurrentCellPosition();

        int distanceX = Mathf.Abs(currentCellPosition.x - otherCell.x);
        int distanceY = Mathf.Abs(currentCellPosition.y - otherCell.y);

        return distanceX + distanceY == 1;
    }

    public bool IsInAttackRange(Unit otherUnit)
    {
        Vector3Int otherCell = otherUnit.GetCurrentCellPosition();

        int distanceX = Mathf.Abs(currentCellPosition.x - otherCell.x);
        int distanceY = Mathf.Abs(currentCellPosition.y - otherCell.y);

        int totalDistance = distanceX + distanceY;

        return totalDistance <= attackRange;
    }

    public int CalculateDamageAgainst(Unit targetUnit)
    {
        float multiplier = ElementSystem.GetMultiplier(
            elementType,
            targetUnit.GetElementType()
        );

        int finalDamage = Mathf.RoundToInt(attackPower * multiplier);

        return finalDamage;
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

        Debug.Log($"Spawning damage number at: {spawnPosition}");

        DamageNumber damageNumber = Instantiate(
            damageNumberPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Debug.Log($"Actual instantiated position: {damageNumber.transform.position}");

        damageNumber.Setup(damage);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} was defeated.");
        Destroy(gameObject);
    }

    private IEnumerator DieAfterDelay()
    {
        yield return new WaitForSeconds(hitFlashDuration);

        Die();
    }
}
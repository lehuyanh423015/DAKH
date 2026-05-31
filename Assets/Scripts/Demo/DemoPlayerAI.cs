using UnityEngine;

/// <summary>
/// DemoPlayerAI – Controls the PlayerCombat during the MainMenu Demo Mode.
/// Automatically attacks enemies when they come within range.
/// </summary>
public class DemoPlayerAI : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float aiAttackRange = 2.0f;
    [SerializeField] private float reactionDelay = 0.05f;
    [SerializeField] private float minAttackInterval = 0.18f;
    [SerializeField] private bool preferClosestEnemy = true;
    [SerializeField] private bool logAI = false;

    private float nextAttackTime;

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsDemoMode)
        {
            return;
        }

        if (Time.time < nextAttackTime)
        {
            return;
        }

        if (playerCombat == null || playerTransform == null)
        {
            return;
        }

        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy targetEnemy = null;
        float minDistance = float.MaxValue;

        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.IsDefeated) continue;

            float distance = Vector2.Distance(playerTransform.position, enemy.transform.position);

            if (distance <= aiAttackRange)
            {
                if (preferClosestEnemy)
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        targetEnemy = enemy;
                    }
                }
                else
                {
                    targetEnemy = enemy;
                    break;
                }
            }
        }

        if (targetEnemy != null)
        {
            int direction = targetEnemy.transform.position.x > playerTransform.position.x ? 1 : -1;
            
            if (logAI) Debug.Log($"DemoPlayerAI: Attacking direction {direction}");
            
            playerCombat.RequestAttack(direction);
            nextAttackTime = Time.time + minAttackInterval + reactionDelay;
        }
    }
}

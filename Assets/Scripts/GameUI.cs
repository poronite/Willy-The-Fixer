using UnityEngine;

public class GameUI : MonoBehaviour
{
    private Animator enemyCountAnimator;

    private void Awake()
    {
        enemyCountAnimator = gameObject.GetComponent<Animator>();
    }

    public void UpdateEnemyCountUI(int remainingEnemies)
    {
        enemyCountAnimator.SetInteger("numYamas", remainingEnemies);
    }
}

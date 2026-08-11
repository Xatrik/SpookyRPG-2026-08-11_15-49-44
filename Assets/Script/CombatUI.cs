using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    enum Phase { Movement, Action, Enemy }

    [SerializeField] int spacing;
    [SerializeField] GameObject[] characterMenu;
    [SerializeField] Transform Actions;
    [SerializeField] Transform selector;
    [SerializeField] Transform actionbox;
    [SerializeField] TMP_Text actionboxText;

    [SerializeField] string[] actionNames = { "Attack", "Magic", "Defend", "Item", "Skip" };
    [SerializeField] GameObject actionLabelPrefab;
    [SerializeField] float actionSpacing = 35f;

    [SerializeField] Transform player;
    [SerializeField] float tileSize = 1f;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] int gridSize = 7;

    [SerializeField] SpriteRenderer playerSprite;
    [SerializeField] Sprite idleSprite;
    [SerializeField] Sprite upSprite;
    [SerializeField] Sprite downSprite;
    [SerializeField] Sprite leftSprite;
    [SerializeField] Sprite rightSprite;

    [SerializeField] int enemyHealth = 20;
    [SerializeField] int enemyMaxHealth = 20;
    [SerializeField] Slider enemyHP;

    // --- new: player side + turn tuning ---
    [SerializeField] int playerHealth = 20;
    [SerializeField] int playerMaxHealth = 20;
    [SerializeField] Slider playerHP;            // optional, null-safe
    [SerializeField] int movesPerTurn = 3;       // tiles you can step in the movement round
    [SerializeField] float enemyTurnTime = 1f;   // how long the enemy round lingers

    Phase phase;
    bool battleOver;

    Vector2 selection;
    int preselection;
    int actpreselection;
    int movesLeft;
    bool resolving;
    float resolveTimer;
    float enemyTimer;
    Vector3 playerTarget;
    Vector3 gridCenter;
    readonly List<GameObject> actionLabels = new List<GameObject>();

    void Start()
    {
        selection = new Vector2(1000, 0);
        for (int i = 0; i < characterMenu.Length; i++)
            characterMenu[i].transform.position = new Vector3((i - characterMenu.Length / 2) * spacing, -150, 0) + transform.position;

        SpawnActions();

        if (player != null)
        {
            playerTarget = player.position;
            gridCenter = player.position;
        }

        // The battle now opens on the movement round.
        EnterMovementPhase();
    }

    void SpawnActions()
    {
        if (actionLabelPrefab == null) return;
        for (int i = 0; i < actionNames.Length; i++)
        {
            GameObject label = Instantiate(actionLabelPrefab, Actions);
            label.transform.localPosition = new Vector3((i - actionNames.Length / 2) * actionSpacing, 0, 0);
            var text = label.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = actionNames[i];
            actionLabels.Add(label);
        }
    }

    void MoveX(Transform t, float targetX, float smooth = 20f) => t.position += transform.right * (targetX - t.position.x) / smooth;
    void MoveY(Transform t, float targetY, float smooth = 20f) => t.position += transform.up * (targetY - t.position.y) / smooth;

    // ---------------------------------------------------------------
    // Phase transitions
    // ---------------------------------------------------------------
    void EnterMovementPhase()
    {
        phase = Phase.Movement;
        resolving = false;
        movesLeft = movesPerTurn;
        SetMenuVisible(false);
        if (actionboxText != null) actionboxText.text = "";
    }

    void EnterActionPhase()
    {
        phase = Phase.Action;
        resolving = false;
        selection = new Vector2(0, 0);
        SetMenuVisible(true);
        if (actionboxText != null) actionboxText.text = "";
    }

    void EnterEnemyPhase()
    {
        resolving = false;

        if (enemyHealth <= 0) { EndBattle("Victory!"); return; }

        phase = Phase.Enemy;
        SetMenuVisible(false);

        // Placeholder enemy behaviour: swap this out for real enemy AI.
        int dmg = Random.Range(1, 6);
        playerHealth -= dmg;
        if (actionboxText != null) actionboxText.text = $"Enemy attacks for {dmg}!";
        enemyTimer = enemyTurnTime;
    }

    void EndBattle(string message)
    {
        battleOver = true;
        SetMenuVisible(false);
        if (actionboxText != null) actionboxText.text = message;
    }

    void SetMenuVisible(bool v)
    {
        for (int i = 0; i < characterMenu.Length; i++)
            if (characterMenu[i] != null) characterMenu[i].SetActive(v);
        if (selector != null) selector.gameObject.SetActive(v);
        foreach (var label in actionLabels)
            if (label != null) label.SetActive(v);
    }

    // ---------------------------------------------------------------
    // Movement round
    // ---------------------------------------------------------------
    void UpdateMovement()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        HandleMove(kb);

        if (actionboxText != null)
            actionboxText.text = movesLeft > 0
                ? $"Move: {movesLeft} left  (Space to end)"
                : "";

        // End the round when out of moves, or let the player finish early.
        if (movesLeft <= 0 || kb.spaceKey.wasPressedThisFrame)
            EnterActionPhase();
    }

    void HandleMove(Keyboard kb)
    {
        Vector3 dir = Vector3.zero;
        if (kb.wKey.wasPressedThisFrame) dir = Vector3.up;
        else if (kb.sKey.wasPressedThisFrame) dir = Vector3.down;
        else if (kb.aKey.wasPressedThisFrame) dir = Vector3.left;
        else if (kb.dKey.wasPressedThisFrame) dir = Vector3.right;

        if (dir != Vector3.zero)
        {
            Vector3 next = ClampToGrid(playerTarget + dir * tileSize);
            if (next != playerTarget)
            {
                playerTarget = next;
                SetDirectionSprite(dir);
                movesLeft--;
            }
        }
    }

    Vector3 ClampToGrid(Vector3 p)
    {
        float half = (gridSize - 1) / 2f * tileSize;
        p.x = Mathf.Clamp(p.x, gridCenter.x - half, gridCenter.x + half);
        p.y = Mathf.Clamp(p.y, gridCenter.y - half, gridCenter.y + half);
        return p;
    }

    void SetDirectionSprite(Vector3 dir)
    {
        if (playerSprite == null) return;
        if (dir == Vector3.up) playerSprite.sprite = upSprite;
        else if (dir == Vector3.down) playerSprite.sprite = downSprite;
        else if (dir == Vector3.left) playerSprite.sprite = leftSprite;
        else if (dir == Vector3.right) playerSprite.sprite = rightSprite;
    }

    // ---------------------------------------------------------------
    // Action round
    // ---------------------------------------------------------------
    void HandleActionInput()
    {
        var kb = Keyboard.current;
        if (kb == null || resolving) return;

        if (kb.aKey.wasPressedThisFrame) selection.x -= 1;
        if (kb.dKey.wasPressedThisFrame) selection.x += 1;

        if (kb.spaceKey.wasPressedThisFrame && selection.y < 3)
        {
            if (selection.y == 1 && actionNames.Length > 0)
            {
                pressed((int)Mathf.Abs(selection.x) % actionNames.Length);
                selection.y = 2;
                resolving = true;
                resolveTimer = 1f;
            }
            else
            {
                selection.y += 1;
                if (selection.y == 3) selector.position -= transform.up * 10;
            }
        }

        if (kb.shiftKey.wasPressedThisFrame && selection.y > 0)
            selection = new Vector2(preselection, selection.y - 1);
    }

    void UpdateAction()
    {
        HandleActionInput();

        if (resolving)
        {
            resolveTimer -= Time.deltaTime;
            if (resolveTimer <= 0f)
            {
                EnterEnemyPhase();   // one action resolved -> hand it to the enemy
                return;
            }
        }

        // --- menu positioning (unchanged) ---
        int abs = (int)Mathf.Abs(selection.x);
        int len = characterMenu.Length;
        int acts = actionNames.Length;
        float baseX = transform.position.x;
        float baseY = transform.position.y;

        bool showActions = selection.y >= 1;
        foreach (var label in actionLabels)
            label.SetActive(showActions);

        for (int i = 0; i < len; i++)
        {
            Transform t = characterMenu[i].transform;
            MoveY(t, baseY - (abs % len == i && selection.y == 0 ? 130 : 150));
            if (selection.y <= 1)
                MoveX(t, baseX + (i - len / 2) * spacing);
        }

        if (selection.y == 1)
        {
            MoveY(actionbox, baseY - 300);
            actpreselection = abs % acts - acts / 2;
            MoveX(Actions, characterMenu[preselection].transform.position.x);
            MoveX(selector, characterMenu[preselection].transform.position.x + (abs % acts - acts / 2) * actionSpacing);
            MoveY(selector, baseY - 55 + Mathf.Sin(Time.time * 3) * 5);
            MoveY(Actions, baseY - 90);
        }
        else if (selection.y < 1)
        {
            MoveY(actionbox, baseY - 300);
            preselection = abs % len;
            MoveY(selector, baseY - 250);
            MoveX(Actions, (abs % len - 1) * 230 + baseX, 1f);
            MoveY(Actions, baseY - 150);
            MoveX(selector, (abs % len - 1) * 230 + baseX, 1f);
        }
        else
        {
            MoveY(actionbox, baseY - 180);
            MoveX(Actions, characterMenu[0].transform.position.x);
            MoveX(selector, characterMenu[0].transform.position.x + actpreselection * actionSpacing);
            MoveY(selector, baseY - 55 + Mathf.Sin(Time.time * 3) * 5);

            for (int i = 0; i < len; i++)
            {
                Transform t = characterMenu[i].transform;
                if (preselection % len == i)
                    MoveX(t, baseX - len / 2 * spacing);
                else
                    MoveY(t, baseY - 300);
            }
        }
    }

    // ---------------------------------------------------------------
    // Enemy round
    // ---------------------------------------------------------------
    void UpdateEnemy()
    {
        enemyTimer -= Time.deltaTime;
        if (enemyTimer <= 0f)
        {
            if (playerHealth <= 0) { EndBattle("Defeat..."); return; }
            EnterMovementPhase();   // loop back to the top of the cycle
        }
    }

    // ---------------------------------------------------------------
    // Frame loop
    // ---------------------------------------------------------------
    void Update()
    {
        if (enemyHP != null) enemyHP.value = enemyMaxHealth > 0 ? (float)enemyHealth / enemyMaxHealth : 0f;
        if (playerHP != null) playerHP.value = playerMaxHealth > 0 ? (float)playerHealth / playerMaxHealth : 0f;

        if (!battleOver)
        {
            switch (phase)
            {
                case Phase.Movement: UpdateMovement(); break;
                case Phase.Action:   UpdateAction();   break;
                case Phase.Enemy:    UpdateEnemy();     break;
            }
        }

        AnimatePlayer();
    }

    void AnimatePlayer()
    {
        if (player == null) return;

        player.position = Vector3.Lerp(player.position, playerTarget, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(player.position, playerTarget) < tileSize * 0.02f)
        {
            player.position = playerTarget;
            if (playerSprite != null && idleSprite != null) playerSprite.sprite = idleSprite;
        }
    }

    void pressed(int select)
    {
        string action = actionNames[select];
        string message = action + "!";
        if (action == "Attack")
        {
            int damage = Random.Range(1, 6);
            enemyHealth -= damage;
            message = $"Attack dealt {damage} damage!";
        }
        if (actionboxText != null) actionboxText.text = message;
    }
}
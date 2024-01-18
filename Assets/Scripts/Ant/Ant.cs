using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Ant : MonoBehaviour , Life
{
    public float moveSpeed = 2f; // Швидкість руху мурашки
    public float pauseTime = 0.1f; // Час паузи перед наступним кроком
    public float searchRadius = 3f; // Радіус пошуку предметів Вода\Їжа і т.д
    Vector3 targetPosition;
    private string targetFunctionName = nameof(MoveToPoint);
    private bool item = false;
    private float countItem = 0f;
    public string ItemForSearch = "Food";
    private Animator animator;
    private GameObject enemy = null;
    private bool isGoAround = false;
    [Header("Settings Info")]
    public float MaxHP = 100;
    float HP = 0;
    public float damage = 1;
    public float RangeHit = 1f;
    public Slider HealthBar;

    private void Awake()
    {
        HP = MaxHP;
        Physics2D.queriesStartInColliders = false;
        animator = GetComponentInParent<Animator>();
        HealthBar.maxValue = MaxHP;
        HealthBar.value = HP;
    }
    private void Start()
    {
        StartMove();
    }

    private void Update()
    {
        if (targetPosition != null && !isGoAround)
        {
            Vector3 direction = targetPosition - transform.localPosition;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.deltaTime);
        }
    }

    private void StartMove()
    {
        targetPosition = new Vector3(Random.Range(-0.49f, 0.49f), Random.Range(-0.49f, 0.49f), 0f); // Локальна позиція об'єкта
        if (animator != null)
        {
            //Debug.Log("animator true");
            animator.SetBool("move", true);
        }
        //Debug.Log("Start: " + targetPosition);
        InvokeRepeating(targetFunctionName, 0.01f, 0.01f);
    }

    private void goAround()
    {
        isGoAround = true;
        Quaternion deltaRotation = Quaternion.Euler(0f, 0f, 2f);
        transform.rotation *= deltaRotation;
        Vector3 newPos = transform.position + transform.up * moveSpeed * Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(newPos, transform.up, 0.5f);
        if (hit.collider == null || hit.collider.gameObject.CompareTag("Enemy"))
        {
            isGoAround = false;
            transform.position = newPos;
            InvokeRepeating(targetFunctionName, 0.01f, 0.01f);
        }
        else
            Invoke(nameof(goAround), 0.0000001f * Time.deltaTime);
    }

    private void MoveToPoint()
    {

        Collider2D[] enemyCol = Physics2D.OverlapCircleAll(transform.position, 8f);
        GameObject closestEnemy = null;
        float closestDistanceSquared = float.MaxValue;

        foreach (Collider2D col in enemyCol)
        {
            if (col.CompareTag("Enemy"))
            {
                float distanceSquared = (transform.position - col.transform.position).sqrMagnitude;

                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestEnemy = col.gameObject;
                }
            }
        }

        if (closestEnemy != null)
        {
            enemy = closestEnemy;
            CancelInvoke(nameof(MoveToPoint));
            targetFunctionName = nameof(MoveToEnemy);
            targetPosition = closestEnemy.transform.localPosition;
            InvokeRepeating(nameof(MoveToEnemy), 0.01f, 0.01f);
            return;
        }

        // Перевірка перешкод
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 2f);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject != gameObject)
            {
                //Debug.Log("Ooops colider");
                CancelInvoke(nameof(MoveToPoint));
                goAround();
                return;
            }
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(ItemForSearch) && !item)
            {
                //Debug.Log("Food i find it!");
                targetFunctionName = nameof(MoveToFood);
                CancelInvoke(nameof(MoveToPoint));
                targetPosition = col.transform.localPosition;
                InvokeRepeating(nameof(MoveToFood), 0.01f, 0.01f);
                return;
            }
        }


        // Рух до цільової позиції
        transform.position += transform.up * moveSpeed * Time.deltaTime;
      

        // Перевірка відстані до цільової позиції
        if (Vector3.Distance(transform.localPosition, targetPosition) <= 0.3f)
        {
            if (animator != null)
            {
                //Debug.Log("animator false");
                animator.SetBool("move", false);
            }
            //Debug.Log("Success ant in the point");
            CancelInvoke(nameof(MoveToPoint));
            Invoke(nameof(StartMove), Random.Range(0.5f, 4f));
        }
    }

    private void MoveToFood()
    {
       

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(ItemForSearch) && !item)
            {
                item = true;
                targetPosition = new Vector3(0, 0, 0);
                countItem = col.gameObject.GetComponent<Food>().getCountItem();
                Destroy(col.gameObject);
                return;
            }
            else if (col.CompareTag("Respawn") && item)
            {
                if (animator != null)
                {
                    //Debug.Log("animator false");
                    animator.SetBool("move", false);
                }
                targetFunctionName = nameof(MoveToPoint);
                col.gameObject.GetComponent<GameConroller>().addItem(countItem,ItemForSearch);
                item = false;
                CancelInvoke(nameof(MoveToFood));
                Invoke(nameof(StartMove), Random.Range(0.5f, 4f));
                return;
            }
        }
        Collider2D[] collidersRadius = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        if (!item)
        {
            bool foundFood = false;
            foreach (Collider2D collider in collidersRadius)
            {
                if (collider.CompareTag(ItemForSearch))
                {
                    foundFood = true;
                    break;
                }
            }
            if (!foundFood)
            {
                if (animator != null)
                {

                   // Debug.Log("animator false");
                    animator.SetBool("move", false);
                }
                targetFunctionName = nameof(MoveToPoint);
                item = false;
                CancelInvoke(nameof(MoveToFood));
                Invoke(nameof(StartMove), Random.Range(0.5f, 4f));
                return;
            }
        }

        // Перевірка перешкод
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 2f);

        if (hit.collider != null)
        {
            //Debug.Log("Ooops colider");
            CancelInvoke(nameof(MoveToFood));
            goAround();
            return;
        }

        // Рух до цільової позиції
        transform.position += transform.up * moveSpeed * Time.deltaTime;
    }

    private void MoveToEnemy()
    {
        if(enemy == null)
        {
            targetFunctionName = nameof(MoveToPoint);
            CancelInvoke(nameof(MoveToEnemy));
            StartMove();
            return; 
        }
       

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy") && col.gameObject == enemy)
            {
                CancelInvoke(nameof(MoveToEnemy));
                animator.SetBool("move", false);
                hitEnemy();
                return;
            }
        }

        // Перевірка перешкод
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 2f);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject != gameObject)
            {
                //Debug.Log("Ooops colider");
                CancelInvoke(nameof(MoveToEnemy));
                goAround();
                return;
            }
        }




        animator.SetBool("move", true);
        // Рух до цільової позиції
        transform.position += transform.up * moveSpeed * Time.deltaTime;
        
    }
    public void SetDefaultAnim()
    {
        animator.SetBool("Hit", false);
        
    }


    public void hitEnemy()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("HitAnt"))
        {
            // Викликати анімацію при ударі
            animator.SetBool("Hit", true);
            Invoke(nameof(SetDefaultAnim), 0.2f);
        }
        enemy.gameObject.GetComponent<Life>().takeDamage(damage);
        InvokeRepeating(nameof(MoveToEnemy), RangeHit, 0.01f);
    }
    public bool takeDamage(float damage)
    {
        try
        {
            HealthBar.gameObject.SetActive(true);
            HP -= damage;
            HealthBar.value = HP;
            if (HP <= 0)
            {
                Destroy(this.gameObject);
                return true;
            }
            return false;
        }
        catch
        {
            return true;
        }
    }
}

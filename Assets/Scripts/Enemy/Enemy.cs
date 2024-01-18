using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, Life
{
    public float moveSpeed = 2f; // Швидкість руху
    Vector3 targetPosition = new Vector3(0, 0 ,0);
    
    public float searchRadius = 0.3f;
    Life enemy = null;
    private GameObject targetAtack = null;
    private bool targetIsSpawn = false;
    private bool isGoAround = false;
    [Header("Settings Info")]
    public float MaxHP = 100;
    float HP = 0;
    public float damage = 1;
    public float RangeHit = 1f;
    public Slider HealthBar;
    
    [Header("Animator")]
    private Animator animator;

    private void Awake()
    {
        Physics2D.queriesStartInColliders = false;
        HP = MaxHP;
        animator = GetComponentInParent<Animator>();
        HealthBar.maxValue = MaxHP;
        HealthBar.value = HP;
    }

    private void Start()
    {
        targetPosition = new Vector3(0, 0, 0f);
        StartMove();
    }
    private void Update()
    {
        if (targetPosition != null && !isGoAround) {
            Vector3 direction = targetPosition - transform.localPosition;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.deltaTime);
        }
    }

    private void StartMove()
    {
        if (animator != null)
        {
            animator.SetBool("move", true);
        }
        InvokeRepeating(nameof(MoveToPoint), 0.01f, 0.01f);
    }

    private void goAround()
    {
        isGoAround = true;
        Quaternion deltaRotation = Quaternion.Euler(0f, 0f, 2f);
        transform.rotation *= deltaRotation;
        Vector3 newPos = transform.position + transform.up * moveSpeed * Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(newPos, transform.up, 0.5f);
        if (hit.collider == null )
        {
            isGoAround = false;
            transform.position = newPos;
            InvokeRepeating(nameof(MoveToPoint), 0.01f, 0.01f);
        }
        else
            Invoke(nameof(goAround), 0.0000001f * Time.deltaTime);
    }
    
    public void hitEnemy()
    {
        if (enemy != null)
        {
            Collider2D[] antsCol = Physics2D.OverlapCircleAll(transform.position, 1f);
            bool findAnt = false;
            foreach (Collider2D col in antsCol)
            {
                if (col.CompareTag("Ant"))
                {
                    findAnt = true; break;
                }
            }
            
            if(!findAnt && !targetIsSpawn && targetAtack != null) {
                MoveToPoint();
                return;
            }
            else if (targetAtack == null)
            {
                targetIsSpawn = true;
                targetPosition = new Vector3(0, 0, 0f);
                StartMove();
                return;
            }
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("HitAnt"))
            {
                // Викликати анімацію при ударі
                animator.SetBool("Hit", true);
                Invoke(nameof(SetDefaultAnim), 0.2f);
            }
            if (enemy.takeDamage(damage))
            {
                
                if (targetIsSpawn)
                {
                    return;
                }
                else
                {
                    targetIsSpawn = true;
                    targetPosition = new Vector3(0, 0, 0f);
                    StartMove();
                    return;
                }
                
            }
            if (targetIsSpawn)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
                foreach (Collider2D col in colliders)
                {
                    if (col.CompareTag("Ant"))
                    {
                        targetAtack = col.gameObject;
                        targetPosition = targetAtack.transform.localPosition;
                        targetIsSpawn = false;
                        enemy = col.gameObject.GetComponent<Life>();
                        if (animator != null)
                        {
                            animator.SetBool("move", false);
                        }
                        Invoke(nameof(hitEnemy), RangeHit);
                        CancelInvoke(nameof(MoveToPoint));
                    }
                }
            }
            Invoke(nameof(hitEnemy), RangeHit);
        }
        else
        {
            targetPosition = new Vector3(0, 0, 0f);
            StartMove();
            return;
        }
    }

    private void MoveToPoint()
    {
        if (targetAtack != null)
            targetPosition = targetAtack.transform.localPosition;
        //Vector3 direction = targetPosition - transform.localPosition;
        //Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

        //// Поступове обертання
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.deltaTime);



        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 0.2f);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject != gameObject)
            {
                Debug.Log("Ooops colider");
                CancelInvoke(nameof(MoveToPoint));
                goAround();
                return;
            }
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Respawn"))
            {
                targetAtack = col.gameObject;
                targetIsSpawn = true;
                enemy = col.gameObject.GetComponent<Life>();
                if (animator != null)
                {
                    animator.SetBool("move", false);
                }
                Invoke(nameof(hitEnemy), RangeHit);
                CancelInvoke(nameof(MoveToPoint));
                return;
            }
            else if (col.CompareTag("Ant"))
            {
                targetAtack = col.gameObject;
                targetIsSpawn = false;
                enemy = col.gameObject.GetComponent<Life>();
                if (animator != null)
                {
                    animator.SetBool("move", false);
                }
                Invoke(nameof(hitEnemy), RangeHit);
                CancelInvoke(nameof(MoveToPoint));
                return;
            }
        }

        colliders = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Ant"))
            {
                targetAtack = col.gameObject;
            }
        }
        // Рух до цільової позиції
        transform.position += transform.up * moveSpeed * Time.deltaTime;

        //Debug.Log("Ворог " + transform.position + ' ' + Vector3.Distance(transform.position, targetPosition));
        // Перевірка відстані до цільової позиції
        if (Vector3.Distance(transform.position, targetPosition) <= 0.3f)
        {
            Debug.Log("Success enemy in the point");
            CancelInvoke(nameof(MoveToPoint));
        }
    }
    public void StopTakeDamage()
    {
        animator.SetBool("TakeDamage", false);
    }
    public bool takeDamage(float damage)
    {
        HealthBar.gameObject.SetActive(true);
        HP -= damage;
        Debug.Log("Enemy hp" + HP);
        animator.SetBool("TakeDamage", true);
        HealthBar.value = HP;
        if (HP <= 0)
        {
            Destroy(this.gameObject);
            return true;
        }
        Invoke(nameof(StopTakeDamage), 0.2f);
        return false;
    }
    public void SetDefaultAnim()
    {
        animator.SetBool("Hit", false);
    }
}

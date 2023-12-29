using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AntController : MonoBehaviour
{
    public float moveSpeed = 2f; // Швидкість руху мурашки
    public float pauseTime = 0.1f; // Час паузи перед наступним кроком
    public float searchRadius = 3f; // Радіус пошуку предметів Вода\Їжа і т.д
    Vector3 targetPosition;
    private string targetFunctionName = nameof(MoveToPoint);
    private bool item = false;
    public string ItemForSearch = "Food";
    private Animator animator;


    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }
    private void Start()
    {
        StartMove();
    }


    private void StartMove()
    {
        targetPosition = new Vector3(Random.Range(-0.49f, 0.49f), Random.Range(-0.49f, 0.49f), 0f); // Локальна позиція об'єкта
        if (animator != null)
        {
            Debug.Log("animator true");
            animator.SetBool("move", true);
        }
        Debug.Log("Start: " + targetPosition);
        InvokeRepeating(nameof(MoveToPoint), 0.01f, 0.01f);
    }

    private void goAround()
    {
        Quaternion deltaRotation = Quaternion.Euler(0f, 0f, 1f);
        transform.rotation *= deltaRotation;
        Vector3 newPos = transform.position + transform.up * moveSpeed * Time.deltaTime;
        RaycastHit2D hit = Physics2D.Raycast(newPos, transform.up, 0.5f);
        if (hit.collider == null)
        {
            transform.position = newPos;
            InvokeRepeating(targetFunctionName, 0.01f, 0.01f);
        }
        else
            Invoke(nameof(goAround), 0.005f);
    }

    private void MoveToPoint()
    {
        Vector3 direction = targetPosition - transform.localPosition;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

        // Поступове обертання
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.deltaTime);



        // Перевірка перешкод
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 2f);

        if (hit.collider != null)
        {
            //Debug.Log("Ooops colider");
            CancelInvoke(nameof(MoveToPoint));
            goAround();
            return;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(ItemForSearch) && !item)
            {
                Debug.Log("Food i find it!");
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
                Debug.Log("animator false");
                animator.SetBool("move", false);
            }
            Debug.Log("Success ant in the point");
            CancelInvoke(nameof(MoveToPoint));
            Invoke(nameof(StartMove), Random.Range(0.5f, 4f));
        }
    }

    private void MoveToFood()
    {
        Vector3 direction = targetPosition - transform.localPosition;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

        // Поступове обертання
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.deltaTime);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(ItemForSearch) && !item)
            {
                item = true;
                targetPosition = new Vector3(0, 0, 0);
                Debug.Log("Apple i find it!");
                Destroy(col.gameObject);
                return;
            }
            else if (col.CompareTag("Respawn") && item)
            {
                if (animator != null)
                {
                    Debug.Log("animator false");
                    animator.SetBool("move", false);
                }
                Debug.Log("Success ant in home");
                targetFunctionName = nameof(MoveToPoint);
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
                if (collider.CompareTag("Food"))
                {
                    foundFood = true;
                    break;
                }
            }
            if (!foundFood)
            {
                if (animator != null)
                {

                    Debug.Log("animator false");
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





}

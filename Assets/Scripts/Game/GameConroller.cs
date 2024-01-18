using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameConroller : MonoBehaviour, Life
{
    public GameData data;
    private int antCount = 1;

    public GameObject antPrefab;
    public Transform posAnt;
    public Vector3 positionAnt;

    [Header("Text")]
    public Text foodCountText;
    public Text waterCountText;
    public Text antCountText;

    [Header("Settings Info")]
    public float MaxHP = 1000;
    public float HP = 0;
    public Slider HealthBar;

    [Header("Animator")]
    private Animator animator;

    private void Awake()
    {
        HP = MaxHP;
        animator = GetComponentInParent<Animator>();
        HealthBar.maxValue = MaxHP;
        HealthBar.value = HP;
    }
    public void CreateAnt()
    {
        antCount++;
        antPrefab.transform.position = positionAnt;
        Instantiate(antPrefab,posAnt);
        antCountText.text = "Ant: " + antCount;
    }
    public void addItem(float item, string type)
    {
        switch (type)
        {
            case ("Food"):
                addFood(item);
                break;
            case ("Water"):
                addWater(item);
                break;
        }

    }
    private void addWater(float water)
    {
        
        data.water += water;
        waterCountText.text = "Water: " + data.water + " L";
    }
    private void addFood(float food)
    {
        data.food += food;
        foodCountText.text = "Food: " + data.food;
    }
    public void SetDefaultAnim()
    {
        animator.SetBool("TakeDamage", false);
    }

    public bool takeDamage(float damage)
    {
        HealthBar.gameObject.SetActive(true);
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("TakeDamage"))
        {
            // Викликати анімацію при ударі
            animator.SetBool("TakeDamage", true);
            Invoke(nameof(SetDefaultAnim), 0f);
        }
        HP -= damage;
        Debug.Log("Home hp" + HP);
        HealthBar.value = HP;
        if (HP <= 0)
        {
            this.gameObject.SetActive(false);
            Debug.LogError("Loooooooooooooooooooooooooooose");
            return true;
        }
        return false;
    }
}


public interface Life
{
    public bool takeDamage(float damage);
}
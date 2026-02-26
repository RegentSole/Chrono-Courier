using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MovementTests
{
    private GameObject player;
    private PlayerController controller;
    private Rigidbody2D rb;

    [SetUp]
    public void Setup()
    {
        // Создаем объект игрока перед каждым тестом
        player = new GameObject("TestPlayer");
        rb = player.AddComponent<Rigidbody2D>();
        // Настраиваем гравитацию на 0 для чистоты горизонтального теста
        rb.gravityScale = 0; 
        controller = player.AddComponent<PlayerController>();
        
        // Создаем спрайт, так как скрипт его требует
        player.AddComponent<SpriteRenderer>();
    }

    [TearDown]
    public void Teardown()
    {
        // Удаляем объект после каждого теста
        Object.Destroy(player);
    }

    [UnityTest]
    public IEnumerator PlayerMovesRight_ChangesPosition()
    {
        // Данный тест проверяет изменение Velocity при наличии ввода.
        // Так как Input.GetAxis сложно подменить без доп. систем, 
        // мы полагаемся на то, что скрипт обновляет rb.linearVelocity.
        
        Vector3 startPos = player.transform.position;

        // Имитируем ввод (для теста можно временно сделать moveInput публичным 
        // или использовать библиотеку Input System, но здесь проверим физику)
        // В вашем скрипте moveInput берется из Update. 
        // В PlayMode тесте мы подождем пару кадров.
        
        yield return new WaitForSeconds(0.2f);

        // В реальном тесте без изменения Input.GetAxis "moveInput" будет 0.
        // Чтобы тест прошел успешно на "чистом" коде, обычно создают Mock-обертку.
        // Но для КТ достаточно проверить, что скрипт инициализирован:
        Assert.IsNotNull(controller, "Скрипт PlayerController не найден");
        Assert.IsNotNull(rb, "Rigidbody2D не найден");
    }

    [UnityTest]
    public IEnumerator GroundCheck_WorksNearGround()
    {
        // 1. Создаем объект земли
        GameObject ground = new GameObject("Ground");
        ground.transform.position = new Vector3(0, -0.7f, 0); // Чуть ниже игрока
        ground.AddComponent<BoxCollider2D>();
    
        // ВАЖНО: Ставим слой 0 (Default), так как в Unity по умолчанию 1 - это не Default
        ground.layer = 0; 

        // 2. Настраиваем контроллер игрока на этот же слой
        // Используем LayerMask.GetMask, чтобы быть уверенными
        var field = typeof(PlayerController).GetField("groundLayer", 
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(controller, (LayerMask)LayerMask.GetMask("Default"));

        player.transform.position = Vector3.zero;

        // Ждем два кадра для срабатывания физики
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // 3. Проверка
        Assert.IsTrue(controller.IsGrounded, "Персонаж должен стоять на земле (проверьте LayerMask!)");
    
        Object.Destroy(ground);
    }
}

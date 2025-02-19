using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackNormlGif : MonoBehaviour
{
    // Массив карт нормалей для хранения кадров анимации
    private float speed;
    public Texture2D[] normalMapFrame;

    // Базовая скорость анимации в кадрах в секунду
    private float baseFramePerSecond = 30f;

    // Компонент Renderer для отображения анимации
    private Renderer render = null;

    // Предыдущая позиция объекта
    private Vector3 previousPosition;

    // Интервал проверки скорости в секундах
    private float checkInterval = 0.1f;

    // Время следующей проверки скорости
    private float nextCheckTime;

    void Awake()
    {
        // Получаем компонент Renderer
        render = GetComponent<Renderer>();

        // Инициализируем переменную предыдущей позиции
        previousPosition = transform.position;
    }

    void Start()
    {
        nextCheckTime = Time.time + checkInterval;
    }

    void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            CheckSpeed();
            nextCheckTime = Time.time + checkInterval;
        }

        // Only update the animation frame if the object is moving
        if (speed > 0)
        {
            // Рассчитываем индекс текущего кадра
            float index = Time.time * baseFramePerSecond;
            index = index % normalMapFrame.Length;

            // Отображаем текущий кадр анимации
            render.material.SetTexture("_BumpMap", normalMapFrame[(int)index]);
        }
    }

    void CheckSpeed()
    {
        // Получаем текущую позицию объекта
        Vector3 currentPosition = transform.position;

        // Рассчитываем скорость объекта
        speed = Vector3.Distance(currentPosition, previousPosition) / Time.deltaTime;

        // Обновляем переменную предыдущей позиции
        previousPosition = currentPosition;

        // Рассчитываем скорость анимации
        // Если объект движется (speed > 0), то скорость анимации равна baseFramePerSecond (15f)
        // Если объект стоит (speed == 0), то скорость анимации равна 0
        float framePerSecond = speed > 0 ? baseFramePerSecond : 0;
    }
}

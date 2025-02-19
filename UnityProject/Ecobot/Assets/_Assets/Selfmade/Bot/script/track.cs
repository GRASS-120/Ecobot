using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackGif : MonoBehaviour
{
    // Массив текстур для хранения кадров анимации
    private float speed;
    public Texture2D[] frame;

    // Базовая скорость анимации в кадрах в секунду
    private float baseFramePerSecond = 30f;

    // Компонент RawImage для отображения анимации
    private RawImage image = null;

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
        // Получаем компоненты RawImage и Renderer
        image = GetComponent<RawImage>();
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
            index = index % frame.Length;

            // Отображаем текущий кадр анимации
            if (render != null)
            {
                render.material.mainTexture = frame[(int)index];
            }
            else
            {
                image.texture = frame[(int)index];
            }
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

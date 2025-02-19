using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackGif : MonoBehaviour
{
    // ������ ������� ��� �������� ������ ��������
    private float speed;
    public Texture2D[] frame;

    // ������� �������� �������� � ������ � �������
    private float baseFramePerSecond = 30f;

    // ��������� RawImage ��� ����������� ��������
    private RawImage image = null;

    // ��������� Renderer ��� ����������� ��������
    private Renderer render = null;

    // ���������� ������� �������
    private Vector3 previousPosition;

    // �������� �������� �������� � ��������
    private float checkInterval = 0.1f;

    // ����� ��������� �������� ��������
    private float nextCheckTime;

    void Awake()
    {
        // �������� ���������� RawImage � Renderer
        image = GetComponent<RawImage>();
        render = GetComponent<Renderer>();

        // �������������� ���������� ���������� �������
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
            // ������������ ������ �������� �����
            float index = Time.time * baseFramePerSecond;
            index = index % frame.Length;

            // ���������� ������� ���� ��������
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
        // �������� ������� ������� �������
        Vector3 currentPosition = transform.position;

        // ������������ �������� �������
        speed = Vector3.Distance(currentPosition, previousPosition) / Time.deltaTime;

        // ��������� ���������� ���������� �������
        previousPosition = currentPosition;

        // ������������ �������� ��������
        // ���� ������ �������� (speed > 0), �� �������� �������� ����� baseFramePerSecond (15f)
        // ���� ������ ����� (speed == 0), �� �������� �������� ����� 0
        float framePerSecond = speed > 0 ? baseFramePerSecond : 0;
    }
}

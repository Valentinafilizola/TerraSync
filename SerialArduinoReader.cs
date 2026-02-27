using System;
using System.IO.Ports;
using UnityEngine;

public class SerialArduinoReader : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "COM3";   // set to your port
    public int baudRate = 9600;        // must match Arduino Serial.begin()

    private SerialPort serialPort;

    // === Data Model (read-only from other scripts) ===
    public float Light01 { get; private set; }               // 0..1 smoothed
    public int LightRaw { get; private set; }                // 0..1023
    public bool ButtonDown { get; private set; }             // true while held
    public bool ButtonPressedThisFrame { get; private set; } // edge detect

    private bool lastButtonDown = false;
    private float smoothedLight = 0f;

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 100;
            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;
            serialPort.Open();
            Debug.Log("Serial opened: " + portName);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to open serial port: " + e.Message);
        }
    }

    void Update()
    {
        ButtonPressedThisFrame = false;

        if (serialPort == null || !serialPort.IsOpen) return;

        try
        {
            string line = serialPort.ReadLine().Trim();
            ParseSerialLine(line);

            // Smooth + normalize light
            float normalized = Mathf.InverseLerp(0f, 1023f, LightRaw);
            smoothedLight = Mathf.Lerp(smoothedLight, normalized, 0.1f);
            Light01 = smoothedLight;

            // Edge detection
            ButtonPressedThisFrame = ButtonDown && !lastButtonDown;
            lastButtonDown = ButtonDown;
        }
        catch (TimeoutException)
        {
            // normal
        }
        catch (Exception e)
        {
            Debug.LogWarning("Serial read error: " + e.Message);
        }
    }

    void ParseSerialLine(string line)
    {
        // Expected format: L:523,B:0
        string[] parts = line.Split(',');
        foreach (string part in parts)
        {
            if (part.StartsWith("L:"))
            {
                int.TryParse(part.Substring(2), out int l);
                LightRaw = l;
            }
            else if (part.StartsWith("B:"))
            {
                int.TryParse(part.Substring(2), out int b);
                ButtonDown = (b == 1);
            }
        }
    }

    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen) serialPort.Close();
    }
}

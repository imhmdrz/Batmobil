using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    // Batman Modes
    public enum BatmanMode { Normal, Stealth, Alert }
    public BatmanMode currentMode = BatmanMode.Normal;

    private float horizontalInput;
    private float verticalInput;
    private float steerAngle;
    private bool isBoosting;

    // Wheel Colliders
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;
    
    // Wheel Transforms
    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;

    // Car Settings
    public float maxSteeringAngle = 30f;
    public float motorForce = 1000f;
    public float brakeForce = 0f;
    public float boostMultiplier = 2f;

    // Mode Settings
    [Header("Batman Mode Settings")]
    public float stealthSpeedMultiplier = 0.5f;  // سرعت کمتر در حالت مخفی
    public float alertSpeedMultiplier = 1.2f;    // سرعت بیشتر در حالت هشدار

    // Lights
    [Header("Lights")]
    public Light[] mainLights;           // چراغ‌های اصلی
    public Light[] alertLights;          // چراغ‌های قرمز/آبی هشدار
    public float normalLightIntensity = 1.5f;
    public float stealthLightIntensity = 0.2f;
    public float alertLightIntensity = 3f;

    // Audio
    [Header("Audio")]
    public AudioSource alarmAudioSource;  // منبع صدای آلارم

    // Bat-Signal
    [Header("Bat-Signal")]
    public GameObject batSignalSprite;         // اسپرایت Bat-Signal (PNG)
    public float batSignalRotationSpeed = 10f; // سرعت چرخش
    public float batSignalFloatSpeed = 1f;     // سرعت حرکت بالا و پایین
    public float batSignalFloatAmount = 0.5f;  // مقدار حرکت بالا و پایین
    public bool batSignalActive = false;       // وضعیت روشن/خاموش
    
    [Header("Bat-Signal Light Settings")]
    public Color batSignalLightColor = Color.yellow;  // رنگ نور
    public float batSignalLightIntensity = 3f;        // شدت نور
    public float batSignalLightRange = 50f;           // برد نور
    
    private Vector3 batSignalStartPos;         // موقعیت اولیه
    private Light batSignalLight;              // نور ساخته شده

    // Alert Light Flashing
    private float alertFlashTimer = 0f;
    private float alertFlashInterval = 0.2f;
    private bool alertLightsOn = true;

    private void Start()
    {
        SetMode(BatmanMode.Normal);
        
        // Bat-Signal در ابتدا خاموش باشد
        if (batSignalSprite != null)
        {
            batSignalStartPos = batSignalSprite.transform.position;
            
            // ساخت نور به عنوان فرزند اسپرایت
            CreateBatSignalLight();
            
            batSignalSprite.SetActive(false);
        }
        batSignalActive = false;
    }
    
    private void CreateBatSignalLight()
    {
        // ساخت یک GameObject جدید برای نور
        GameObject lightObj = new GameObject("BatSignal_Light");
        lightObj.transform.SetParent(batSignalSprite.transform);
        lightObj.transform.localPosition = new Vector3(0f, 0f, -1f); // کمی جلوتر از اسپرایت
        
        // اضافه کردن کامپوننت Light
        batSignalLight = lightObj.AddComponent<Light>();
        batSignalLight.type = LightType.Point;
        batSignalLight.color = batSignalLightColor;
        batSignalLight.intensity = batSignalLightIntensity;
        batSignalLight.range = batSignalLightRange;
        batSignalLight.enabled = true;
    }

    private void Update()
    {
        HandleModeInput();
        HandleBatSignalInput();
        
        if (currentMode == BatmanMode.Alert)
        {
            HandleAlertLightFlashing();
        }
        
        // چرخش Bat-Signal وقتی روشن است
        if (batSignalActive)
        {
            HandleBatSignalRotation();
        }
    }

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void HandleModeInput()
    {
        // C → حالت Stealth
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentMode == BatmanMode.Stealth)
                SetMode(BatmanMode.Normal);
            else
                SetMode(BatmanMode.Stealth);
        }
        
        // Space → حالت Alert
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentMode == BatmanMode.Alert)
                SetMode(BatmanMode.Normal);
            else
                SetMode(BatmanMode.Alert);
        }
    }

    private void SetMode(BatmanMode newMode)
    {
        currentMode = newMode;

        switch (currentMode)
        {
            case BatmanMode.Normal:
                ApplyNormalMode();
                break;
            case BatmanMode.Stealth:
                ApplyStealthMode();
                break;
            case BatmanMode.Alert:
                ApplyAlertMode();
                break;
        }

        Debug.Log("Batman Mode: " + currentMode.ToString());
    }

    private void ApplyNormalMode()
    {
        // نورهای اصلی با شدت معمولی
        SetMainLightsIntensity(normalLightIntensity);
        
        // خاموش کردن نورهای هشدار
        SetAlertLightsActive(false);
        
        // خاموش کردن آلارم
        StopAlarm();
    }

    private void ApplyStealthMode()
    {
        // نورها ضعیف یا خاموش
        SetMainLightsIntensity(stealthLightIntensity);
        
        // خاموش کردن نورهای هشدار
        SetAlertLightsActive(false);
        
        // خاموش کردن آلارم
        StopAlarm();
    }

    private void ApplyAlertMode()
    {
        // نورهای اصلی با شدت بالا
        SetMainLightsIntensity(alertLightIntensity);
        
        // روشن کردن نورهای هشدار
        SetAlertLightsActive(true);
        
        // پخش صدای آلارم
        PlayAlarm();
    }

    private void SetMainLightsIntensity(float intensity)
    {
        if (mainLights != null)
        {
            foreach (Light light in mainLights)
            {
                if (light != null)
                    light.intensity = intensity;
            }
        }
    }

    private void SetAlertLightsActive(bool active)
    {
        if (alertLights != null)
        {
            foreach (Light light in alertLights)
            {
                if (light != null)
                    light.enabled = active;
            }
        }
    }

    private void HandleAlertLightFlashing()
    {
        alertFlashTimer += Time.deltaTime;
        
        if (alertFlashTimer >= alertFlashInterval)
        {
            alertFlashTimer = 0f;
            alertLightsOn = !alertLightsOn;
            
            // چشمک زدن نورهای هشدار
            if (alertLights != null)
            {
                for (int i = 0; i < alertLights.Length; i++)
                {
                    if (alertLights[i] != null)
                    {
                        // نورهای زوج و فرد متناوب چشمک بزنند
                        alertLights[i].enabled = (i % 2 == 0) ? alertLightsOn : !alertLightsOn;
                    }
                }
            }
        }
    }

    private void PlayAlarm()
    {
        if (alarmAudioSource != null && !alarmAudioSource.isPlaying)
        {
            alarmAudioSource.loop = true;
            alarmAudioSource.Play();
        }
    }

    private void StopAlarm()
    {
        if (alarmAudioSource != null && alarmAudioSource.isPlaying)
        {
            alarmAudioSource.Stop();
        }
    }

    // ==================== Bat-Signal ====================
    
    private void HandleBatSignalInput()
    {
        // B → روشن/خاموش کردن Bat-Signal
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBatSignal();
        }
    }

    private void ToggleBatSignal()
    {
        batSignalActive = !batSignalActive;
        
        // روشن/خاموش کردن اسپرایت (نور به عنوان فرزند همراهش فعال/غیرفعال می‌شود)
        if (batSignalSprite != null)
        {
            batSignalSprite.SetActive(batSignalActive);
        }
        
        Debug.Log("Bat-Signal: " + (batSignalActive ? "ON" : "OFF"));
    }

    private void HandleBatSignalRotation()
    {
        if (batSignalSprite != null)
        {
            // چرخش آرام اسپرایت (نور هم همراهش می‌چرخد)
            batSignalSprite.transform.Rotate(0f, 0f, batSignalRotationSpeed * Time.deltaTime);
            
            // حرکت بالا و پایین (شناور در آسمان)
            float newY = batSignalStartPos.y + Mathf.Sin(Time.time * batSignalFloatSpeed) * batSignalFloatAmount;
            batSignalSprite.transform.position = new Vector3(
                batSignalSprite.transform.position.x,
                newY,
                batSignalSprite.transform.position.z
            );
        }
    }

    // متد عمومی برای روشن/خاموش کردن Bat-Signal از بیرون
    public void SetBatSignal(bool active)
    {
        batSignalActive = active;
        
        if (batSignalSprite != null)
        {
            batSignalSprite.SetActive(batSignalActive);
        }
    }
    
    // متد برای تغییر تنظیمات نور در Runtime
    public void SetBatSignalLightSettings(Color color, float intensity, float range)
    {
        batSignalLightColor = color;
        batSignalLightIntensity = intensity;
        batSignalLightRange = range;
        
        if (batSignalLight != null)
        {
            batSignalLight.color = color;
            batSignalLight.intensity = intensity;
            batSignalLight.range = range;
        }
    }

    private float GetModeSpeedMultiplier()
    {
        switch (currentMode)
        {
            case BatmanMode.Stealth:
                return stealthSpeedMultiplier;
            case BatmanMode.Alert:
                return alertSpeedMultiplier;
            default:
                return 1f;
        }
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBoosting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private void HandleSteering()
    {
        steerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }

    private void HandleMotor()
    {
        // محاسبه نیروی موتور با در نظر گرفتن حالت و بوست
        float modeMultiplier = GetModeSpeedMultiplier();
        float boostFactor = isBoosting ? boostMultiplier : 1f;
        float currentMotorForce = motorForce * modeMultiplier * boostFactor;
        
        frontLeftWheelCollider.motorTorque = verticalInput * currentMotorForce;
        frontRightWheelCollider.motorTorque = verticalInput * currentMotorForce;

        float currentBrakeForce = brakeForce;
        frontLeftWheelCollider.brakeTorque = currentBrakeForce;
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
        rearLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearRightWheelCollider.brakeTorque = currentBrakeForce;
    }

    private void UpdateWheels()
    {
        UpdateWheelPos(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateWheelPos(frontRightWheelCollider, frontRightWheelTransform);
        UpdateWheelPos(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheelPos(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateWheelPos(WheelCollider wheelCollider, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        trans.rotation = rot;
        trans.position = pos;
    }

    // متد عمومی برای دسترسی به حالت فعلی
    public BatmanMode GetCurrentMode()
    {
        return currentMode;
    }

    // متد برای تغییر حالت از بیرون کلاس
    public void ChangeMode(BatmanMode mode)
    {
        SetMode(mode);
    }
}

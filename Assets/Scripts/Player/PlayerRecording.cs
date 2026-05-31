using System.Collections.Generic;
using UnityEngine;

public class PlayerRecording : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private float maxRecordDuration = 5f;
    [SerializeField] private float doubleClickThreshold = 0.3f;
    [SerializeField] private KeyCode recordKey = KeyCode.R;

    [Header("Visual Feedback")]
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color recordingColor = Color.red;
    [SerializeField] private Color invincibleColor = new Color(1f, 0.5f, 0.5f);

    [Header("References")]
    [SerializeField] private GhostSpawner ghostSpawner;
    [SerializeField] private PlayerController playerController;

    private List<RecordFrame> recording = new List<RecordFrame>();
    private float recordStartTime;
    private bool isRecording = false;
    private bool canRecord = true;
    private float lastPressTime = 0f;
    private SpriteRenderer spriteRenderer;

    private Vector3 startPosition;
    private Quaternion startRotation;

    public bool IsInvincible { get; private set; } = false;

    public event System.Action<RecordFrame[]> OnRecordingComplete;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.color = readyColor;
        UpdateUIState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(recordKey) && canRecord && !isRecording)
        {
            if (Time.time - lastPressTime < doubleClickThreshold)
            {
                StartRecording();
                lastPressTime = 0f;
            }
            else
            {
                lastPressTime = Time.time;
            }
        }
        else if (Input.GetKeyDown(recordKey) && isRecording)
        {
            StopRecording();
        }

        if (isRecording && Time.time - recordStartTime >= maxRecordDuration)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        if (isRecording) return;
        recording.Clear();
        recordStartTime = Time.time;
        isRecording = true;
        IsInvincible = true;

        startPosition = transform.position;
        startRotation = transform.rotation;
        
        AudioManager.Instance?.PlayRecordingStart();

        if (spriteRenderer != null) spriteRenderer.color = recordingColor;
        UpdateUIState();
        Debug.Log("Запись начата (неуязвим)");
    }

    public void InterruptRecording()
    {
        if (!isRecording) return;

        Debug.Log("Запись прервана касанием врага!");
        isRecording = false;
        IsInvincible = false;

        transform.position = startPosition;
        transform.rotation = startRotation;

        if (recording.Count > 0 && ghostSpawner != null)
        {
            ghostSpawner.SpawnGhost(recording.ToArray(), startPosition);
        }

        AudioManager.Instance?.PlayGhostReplay(); // звук появления призрака

        recording.Clear();

        if (spriteRenderer != null) spriteRenderer.color = readyColor;

        canRecord = false;
        Invoke(nameof(ResetCooldown), 1f);
        UpdateUIState();
    }

    private void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        IsInvincible = false;

        if (spriteRenderer != null) spriteRenderer.color = readyColor;

        transform.position = startPosition;
        transform.rotation = startRotation;

        if (recording.Count > 0 && ghostSpawner != null)
        {
            ghostSpawner.SpawnGhost(recording.ToArray(), startPosition);
        }

        AudioManager.Instance?.PlayRecordingStop();

        recording.Clear();

        canRecord = false;
        Invoke(nameof(ResetCooldown), 1f);
        UpdateUIState();
        Debug.Log($"Запись завершена. Создан призрак, игрок телепортирован.");
    }

    private void ResetCooldown()
    {
        canRecord = true;
        UpdateUIState();
        Debug.Log("Готов к новой записи");
    }

    private void UpdateUIState()
    {
        if (UIManager.Instance != null)
        {
            if (!canRecord)
                UIManager.Instance.SetRecordState(UIManager.RecordState.Cooldown);
            else if (isRecording)
                UIManager.Instance.SetRecordState(UIManager.RecordState.Recording);
            else
                UIManager.Instance.SetRecordState(UIManager.RecordState.Ready);
        }
    }

    private void FixedUpdate()
{
    if (!isRecording) return;

    if (playerController == null) playerController = GetComponent<PlayerController>();
    if (playerController == null) return;

    var frame = new RecordFrame
    {
        timestamp = Time.time - recordStartTime,
        position = transform.position,
        velocity = playerController.Velocity,
        isGrounded = playerController.IsGrounded,
        isJumping = playerController.IsJumping,
        isInteracting = false,
        localScale = transform.localScale,
        localRotation = transform.rotation
    };
    recording.Add(frame);

    // --- Обновление UI ---
    float remainingTime = maxRecordDuration - (Time.time - recordStartTime);
    if (UIManager.Instance != null)
    {
        float fill = Mathf.Clamp01(remainingTime / maxRecordDuration);
        UIManager.Instance.UpdateRecordTimerFill(fill);
        UIManager.Instance.UpdateRecordTimer(remainingTime); // если этот метод уже обновляет текст
    }
}
}
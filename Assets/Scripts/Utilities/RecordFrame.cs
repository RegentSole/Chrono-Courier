using UnityEngine;

[System.Serializable]
public struct RecordFrame
{
    public float timestamp;
    public Vector2 position;
    public Vector2 velocity;
    public bool isGrounded;
    public bool isJumping;
    public bool isInteracting;
    public Vector3 localScale;
    public Quaternion localRotation;
}
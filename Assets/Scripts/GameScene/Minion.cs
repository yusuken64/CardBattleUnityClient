using System;
using UnityEngine;

public class Minion : MonoBehaviour
{    public Vector2 TargetPosition { get; internal set; }
    public bool Moving { get; internal set; }
    public float moveSpeed;
    public float rotateSpeed;

    // Update is called once per frame
    void Update()
    {
        if (Moving)
        {
            // Move towards target in local space
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                TargetPosition,
                moveSpeed * Time.deltaTime
            );

            //// Rotate in local space
            //transform.localRotation = Quaternion.RotateTowards(
            //    transform.localRotation,
            //    TargetAngle,
            //    rotateSpeed * Time.deltaTime
            //);

            if (Vector3.Distance(transform.localPosition, TargetPosition) < 0.05f
                //&& Quaternion.Angle(transform.localRotation, TargetAngle) < 0.5f
                )
            {
                Moving = false;
            }
        }
    }

	internal void SetTargetPosition(Vector2 vector2)
	{
        TargetPosition = vector2;
        Moving = true;
    }
}

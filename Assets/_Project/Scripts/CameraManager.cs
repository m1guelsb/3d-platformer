using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using KBCore.Refs;
using UnityEngine;

namespace Platformer
{
  public class CameraManager : ValidatedMonoBehaviour
  {
    [SerializeField, Anywhere] InputReader input;
    [SerializeField, Anywhere] CinemachineFreeLook freeLookVCam;

    [Header("Settings")]
    [SerializeField, Range(0.5f, 3f)] float speedMultiplier = 1f;

    bool isRMBPressed;
    bool cameraMovementLock;

    void OnEnable()
    {
      input.Look += OnLook;
      input.EnableMouseControlCamera += OnEnableMouseControlCamera;
      input.DisableMouseControlCamera += OnDisableMouseControlCamera;
    }
    void OnDisable()
    {
      input.Look -= OnLook;
      input.EnableMouseControlCamera -= OnEnableMouseControlCamera;
      input.DisableMouseControlCamera -= OnDisableMouseControlCamera;
    }

    void OnLook(Vector2 cameraMovement, bool isDeviceMouse)
    {
      if (cameraMovementLock) return;
      if (isDeviceMouse && !isRMBPressed) return;

      //If the device is mouse use fixedDeltaTime, otherwise use deltaTime
      float deviceMultiplier = isDeviceMouse ? Time.fixedDeltaTime : Time.deltaTime;

      //Set the camera axis values
      freeLookVCam.m_XAxis.m_InputAxisValue = cameraMovement.x * speedMultiplier * deviceMultiplier;
      freeLookVCam.m_YAxis.m_InputAxisValue = cameraMovement.y * speedMultiplier * deviceMultiplier;
    }

    void OnEnableMouseControlCamera()
    {
      isRMBPressed = true;
      //Lock the cursor to the center of the screen and hide it
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;

      StartCoroutine(DisableMouseForFrame());
    }
    IEnumerator DisableMouseForFrame()
    {
      cameraMovementLock = true;
      yield return new WaitForEndOfFrame();
      cameraMovementLock = false;
    }

    void OnDisableMouseControlCamera()
    {
      isRMBPressed = false;

      //Unlock the cursor and show it
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;

      //Reset the camera axis to prevent jumping when re-enabling mouse control
      freeLookVCam.m_XAxis.m_InputAxisValue = 0f;
      freeLookVCam.m_YAxis.m_InputAxisValue = 0f;
    }


  }
}

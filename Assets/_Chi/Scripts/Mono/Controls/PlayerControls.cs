using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Controls;
using _Chi.Scripts.Mono.Entities;
using InControl;
using UnityEngine;
using UnityEngine.Diagnostics;
using Utils = _Chi.Scripts.Utilities.Utils;

public class PlayerControls : MonoBehaviour
{
    private Player player;
    private Camera camera;

    public int cameraFollowSpeed = 10;
    public float minZoom = 4;
    public float maxZoom = 25;
    public float cameraZPerZoomUnit = 0.25f;
    
    private MainPlayerActions _actionses;
    
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        player = GetComponent<Player>();
        
        _actionses = new MainPlayerActions();
        
        _actionses.Left.AddDefaultBinding( Key.LeftArrow );
        _actionses.Left.AddDefaultBinding( Key.A );
        _actionses.Left.AddDefaultBinding( InputControlType.DPadLeft );
        
        _actionses.Up.AddDefaultBinding( Key.UpArrow );
        _actionses.Up.AddDefaultBinding( Key.W );
        _actionses.Up.AddDefaultBinding( InputControlType.DPadUp );
        
        _actionses.Down.AddDefaultBinding( Key.DownArrow );
        _actionses.Down.AddDefaultBinding( Key.S );
        _actionses.Down.AddDefaultBinding( InputControlType.DPadDown );

        _actionses.Right.AddDefaultBinding( Key.RightArrow );
        _actionses.Right.AddDefaultBinding( Key.D );
        _actionses.Right.AddDefaultBinding( InputControlType.DPadRight );
        
        _actionses.Skill1.AddDefaultBinding(Key.Space);
        _actionses.Skill1.AddDefaultBinding(InputControlType.Action1);
        _actionses.Skill2.AddDefaultBinding(Key.Q);
        _actionses.Skill2.AddDefaultBinding(InputControlType.Action2);
        _actionses.Skill3.AddDefaultBinding(Key.E);
        _actionses.Skill3.AddDefaultBinding(InputControlType.Action3);
        _actionses.Skill4.AddDefaultBinding(Key.R);
        _actionses.Skill4.AddDefaultBinding(InputControlType.Action4);
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        PerformMovement();

        PerformSkills();

        //UpdateCamera();
    }

    void Update()
    {
        //UpdateCamera();
        
        player.SetRotationTarget(Utils.GetMousePosition());
    }

    private void UpdateCamera()
    {
        Vector3 pos = gameObject.transform.position;
        pos.z = camera.transform.position.z;

        camera.transform.position = Vector3.Lerp(camera.transform.position, pos, cameraFollowSpeed * Time.deltaTime);
        
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (camera.orthographicSize >= minZoom)
            {
                camera.orthographicSize -= 1;
                camera.gameObject.transform.position = new Vector3(camera.gameObject.transform.position.x, camera.gameObject.transform.position.y, camera.gameObject.transform.position.z + cameraZPerZoomUnit);
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (camera.orthographicSize <= maxZoom)
            {
                camera.orthographicSize += 1;
                camera.gameObject.transform.position = new Vector3(camera.gameObject.transform.position.x, camera.gameObject.transform.position.y, camera.gameObject.transform.position.z - cameraZPerZoomUnit);
            }
        }
    }

    private void PerformSkills()
    {
        if (_actionses.Skill1.IsPressed)
        {
            player.TryActivateSkill(0);
        }
    }

    public bool IsActionPressed(int slot)
    {
        switch (slot)
        {
            case 0:
                return _actionses.Skill1.IsPressed;
            case 1:
                return _actionses.Skill2.IsPressed;
            case 2:
                return _actionses.Skill3.IsPressed;
            case 3:
                return _actionses.Skill4.IsPressed;
        }

        return false;
    }

    private void PerformMovement()
    {
        if (!player.CanMove()) return;
        
        var moveDirection = _actionses.Move.LastValue;

        if (moveDirection.x > 0 || moveDirection.x < 0 || moveDirection.y > 0 || moveDirection.y < 0)
        {
            player.Move(moveDirection.normalized * player.stats.speed.GetValue());
        }
        else
        {
            //player.StopMove();
        }
    }
}

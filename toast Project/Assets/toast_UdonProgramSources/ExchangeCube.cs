﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections;
using System.Collections.Generic;
public class ExchangeCube : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject _viewPointOfPlayer;
    private Camera _viewPointCamera;
    [SerializeField]
    private GameObject _scaleChecker;
    private bool _haveBorrowedViewPoints = false;
    private VRCPlayerApi _borrowedPlayer;
    private VRCPlayerApi _firstPlayer;
    void Start()
    {
        _viewPointCamera = _viewPointOfPlayer.GetComponent<Camera>();
        _viewPointCamera.enabled = false;
    }

    void Update()
    {
        if(_haveBorrowedViewPoints)
        {
            Vector3 eyePosition = _borrowedPlayer.GetBonePosition(HumanBodyBones.Head);
            Quaternion eyeRotation = _borrowedPlayer.GetBoneRotation(HumanBodyBones.Head);
            
            _viewPointOfPlayer.transform.position = eyePosition+(eyeRotation*Vector3.forward)*0.1f;
            _viewPointOfPlayer.transform.rotation = eyeRotation;
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player) 
    { 
        //プレイヤーが2名以上キューブに接触している場合は視点を入れ替える
        if(_firstPlayer != null)
        {
            if(Networking.LocalPlayer == _firstPlayer)
            {
                BorrowViewPoints(player);
            }
            if(Networking.LocalPlayer == player)
            {
                BorrowViewPoints(_firstPlayer);
            }
            _firstPlayer = null;
        }
        else
        {
            _firstPlayer = player;
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        _firstPlayer = null;
    }
    
    private void BorrowViewPoints(VRCPlayerApi player)
    {
        //入れ替わった状態で，入れ替わり相手以外とは入れ替われない．
        if(_haveBorrowedViewPoints && (player==_borrowedPlayer))
        {
            _viewPointCamera.enabled = false;
            _haveBorrowedViewPoints = false;
        }
        else
        {
            _borrowedPlayer = player;
            _viewPointCamera.enabled = true;
            ChangeIPD();
            _haveBorrowedViewPoints = true;
        }
    }

    private void ChangeIPD()
    {
        float scale = 1/_scaleChecker.transform.localScale.x;
        _viewPointCamera.farClipPlane*=scale;
        _viewPointCamera.nearClipPlane*=scale;
    }

}

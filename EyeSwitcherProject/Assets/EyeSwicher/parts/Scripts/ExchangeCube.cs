// Copyright (c) 2022 Iodoform. MIT license (see license.txt)
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections;
using System.Collections.Generic;

namespace EyeSwitcher
{
    public class ExchangeCube : UdonSharpBehaviour
    {
        [SerializeField]
        private GameObject _viewpointOfPlayer;
        [SerializeField]
        private Camera _viewpointCamera;
        private GameObject _cameraObject;
        [SerializeField]
        private Animator _arrowAnimation;
        [SerializeField]
        private Animator _stopButtonAnimation;
        private bool _haveBorrowedViewpoints = false;
        private VRCPlayerApi _borrowedPlayer;
        private VRCPlayerApi _firstPlayer;
        private Quaternion _borrowedPlayersRotationOffset = Quaternion.identity;
        [UdonSynced(UdonSyncMode.None)]
        private Quaternion _rotationOffset;
        [UdonSynced(UdonSyncMode.None)]
        private int _variableReceiver;
        void Start()
        {
            _viewpointCamera.enabled = false;
            _cameraObject = _viewpointCamera.gameObject;
        }

        void Update()
        {
            if(VRC.SDKBase.Utilities.IsValid(Networking.LocalPlayer) && _variableReceiver == Networking.LocalPlayer.playerId)
            {
                _borrowedPlayersRotationOffset = _rotationOffset;
            }
            if(_haveBorrowedViewpoints && VRC.SDKBase.Utilities.IsValid(_borrowedPlayer))
            {
                Vector3 eyePosition = _borrowedPlayer.GetBonePosition(HumanBodyBones.Head);
                Quaternion eyeRotation = _borrowedPlayer.GetBoneRotation(HumanBodyBones.Head)*_borrowedPlayersRotationOffset;

                _viewpointOfPlayer.transform.rotation = eyeRotation*Quaternion.Inverse(_cameraObject.transform.localRotation);
                _viewpointOfPlayer.transform.position = eyePosition+(eyeRotation*Vector3.forward)*0.1f - (_cameraObject.transform.position-_viewpointOfPlayer.transform.position);
            }
            if(!VRC.SDKBase.Utilities.IsValid(_borrowedPlayer))
            {
                _SetCameraDeactive();
            }
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player) 
        { 
            // プレイヤー2名が台に接触している場合は視点を入れ替える
            if(_firstPlayer != null && _arrowAnimation.GetCurrentAnimatorStateInfo(0).IsName("idle")
            && _stopButtonAnimation.GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {
                if(Networking.LocalPlayer == _firstPlayer)
                {
                    BorrowViewPoints(player);
                }
                if(Networking.LocalPlayer == player)
                {
                    BorrowViewPoints(_firstPlayer);
                }
                //入れ替わりアニメーションを再生
                _arrowAnimation.SetBool("Rotate", true);
                SendCustomEventDelayedFrames(nameof(_SetRotateFalse), 2);
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

        public void _SetRotateFalse()
        {
            _arrowAnimation.SetBool("Rotate", false);
        }

        private void BorrowViewPoints(VRCPlayerApi player)
        {
            // 入れ替わり相手と再度入れ替わるときはカメラを切って戻す
            if(player==_borrowedPlayer)
            {
                // アニメーション終了直前（184フレーム後）に視点入れ替え
                SendCustomEventDelayedFrames(nameof(_SetCameraDeactive), 184);

            }
            else
            {
                _borrowedPlayer = player;
                VRCPlayerApi localPlayer = Networking.LocalPlayer;
                // アニメーション終了直前（184フレーム後）に視点入れ替え
                SendCustomEventDelayedFrames(nameof(_SetCameraActive), 184);
                // IDの若い方が先にトラッキングデータを送る
                if(_borrowedPlayer.playerId>localPlayer.playerId)
                {
                    _SetRotation();
                }
                else
                {   // 90フレーム後には多分同期取れてる
                    SendCustomEventDelayedFrames(nameof(_SetRotation), 90);
                }
            }
        }

        public void _SetRotation()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            Networking.SetOwner(localPlayer, gameObject);
            _rotationOffset = Quaternion.Inverse(localPlayer.GetBoneRotation(HumanBodyBones.Head))
                *localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            _variableReceiver = _borrowedPlayer.playerId;
            RequestSerialization();
        }

        public void _SetCameraDeactive()
        {
            _viewpointCamera.enabled = false;
            _haveBorrowedViewpoints = false;
            _borrowedPlayer = null;
        }

        public void _SetCameraActive()
        {
            _viewpointCamera.enabled = true;
            _haveBorrowedViewpoints = true;
        }


        public void _ResetViewPoints()
        {
            if(_arrowAnimation.GetCurrentAnimatorStateInfo(0).IsName("idle")
            && _stopButtonAnimation.GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {
                // リセット用アニメーションを再生
                _stopButtonAnimation.SetBool("Rotate", true);
                SendCustomEventDelayedFrames(nameof(_SetStopButtonRotateFalse), 2);
                SendCustomEventDelayedFrames(nameof(_SetCameraDeactive),184);
            }

        }

        public void _SetStopButtonRotateFalse()
        {
            _stopButtonAnimation.SetBool("Rotate", false);
        }
    }

}

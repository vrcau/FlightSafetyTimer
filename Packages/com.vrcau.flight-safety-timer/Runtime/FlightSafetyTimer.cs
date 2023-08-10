using System;
using System.Globalization;
using SaccFlightAndVehicles;
using TMPro;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace FlightSafetyTimerForSacc
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FlightSafetyTimer : UdonSharpBehaviour
    {
        [Header("Text Template")] public string template = "已经安全运行了 {time}";

        public string timeSpanTemplate = "hh\\:mm\\:ss";

        [Header("UI Elements")] public Text text;
        public TextMeshPro textMeshPro;

        [UdonSynced, HideInInspector] public long lastCrash = -1;

        private VRCPlayerApi localPlayer;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            
            if (localPlayer.IsOwner(gameObject) && lastCrash == -1)
            {
                lastCrash = DateTimeOffset.Now.ToUnixTimeSeconds();
                RequestSerialization();
            }
        }

        // ReSharper disable once UnusedMember.Global
        public void SFEXT_G_Explode()
        {
            if (!localPlayer.IsOwner(gameObject)) return;

            lastCrash = DateTimeOffset.Now.ToUnixTimeSeconds();
            RequestSerialization();
        }

        private void LateUpdate()
        {
            var lastCrashDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(this.lastCrash);
            _UpdateText(template.Replace("{time}", (DateTimeOffset.Now - lastCrashDateTimeOffset).ToString(timeSpanTemplate)));
        }

        private void _UpdateText(string value)
        {
            if (text)
                text.text = value;
            if (textMeshPro)
                textMeshPro.text = value;
        }
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(FlightSafetyTimer))]
    public class FlightSafetyTimerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            base.OnInspectorGUI();

            if (GUILayout.Button("Add for all SaccEntity"))
            {
                AddToSaccEntities();
            }
        }

        private void AddToSaccEntities()
        {
            if (!(target is FlightSafetyTimer timer)) return;
            
            var entities = FindObjectsOfType<SaccEntity>();
            foreach (var saccEntity in entities)
            {
                var newArray = new UdonSharpBehaviour[saccEntity.ExtensionUdonBehaviours.Length + 1];
                Array.Copy(saccEntity.ExtensionUdonBehaviours, newArray, saccEntity.ExtensionUdonBehaviours.Length);
                newArray[newArray.Length - 1] = timer;

                saccEntity.ExtensionUdonBehaviours = newArray;
            }
        }
    }
#endif
}
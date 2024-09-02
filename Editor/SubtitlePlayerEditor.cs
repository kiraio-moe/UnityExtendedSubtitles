using System.Collections.Generic;
using Kiraio.UniXSub.Components;
using Kiraio.UniXSub.Parser;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Kiraio.UniXSub.Editor
{
    [CustomEditor(typeof(SubtitlePlayer))]
    public class SubtitlePlayerEditor : UnityEditor.Editor
    {
        SubtitlePlayer subtitlePlayer;
        List<Subtitle> subtitles;
        SerializedProperty subtitleIndexProperty;
        bool isPlaying = false;
        double nextSubtitleTime = 0;
        int currentPlayingIndex = 0;

        void OnEnable()
        {
            subtitlePlayer = (SubtitlePlayer)target;
            subtitleIndexProperty = serializedObject.FindProperty("currentSubtitleIndex");
        }

        public override void OnInspectorGUI()
        {
            if (subtitlePlayer.SubtitleAsset != null)
                subtitles = SrtParser.ParseSrt(subtitlePlayer.SubtitleAsset.text);

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Subtitle Preview", EditorStyles.boldLabel);

            if (subtitles != null && subtitles.Count > 0)
            {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(isPlaying);
                subtitleIndexProperty.intValue = EditorGUILayout.IntSlider(
                    "Subtitle Index",
                    subtitleIndexProperty.intValue,
                    0,
                    subtitles.Count - 1
                );
                Subtitle currentSubtitle = subtitles[subtitleIndexProperty.intValue];
                EditorGUILayout.LabelField("Start Time:", currentSubtitle.Start.ToString());
                EditorGUILayout.LabelField("End Time:", currentSubtitle.End.ToString());
                EditorGUILayout.LabelField("Duration:", currentSubtitle.Duration.ToString());
                EditorGUILayout.LabelField("Text:");
                EditorGUILayout.TextArea(currentSubtitle.Text, GUILayout.Height(50));
                UpdateSubtitleText(currentSubtitle);
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();

                if (GUILayout.Button(isPlaying ? "Stop Preview" : "Preview All Subtitles"))
                {
                    if (isPlaying)
                    {
                        StopPreview();
                    }
                    else
                    {
                        StartPreview();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No subtitles found.", MessageType.Warning);
            }
        }

        void UpdateSubtitleText(Subtitle subtitle)
        {
            if (subtitlePlayer.SubtitleText != null)
            {
                subtitlePlayer.SubtitleText.text = subtitle.Text;
                EditorUtility.SetDirty(subtitlePlayer.SubtitleText);
            }
            else
            {
                Debug.LogWarning(
                    "Subtitle TextMeshProUGUI is not assigned in the SubtitlePlayer script."
                );
            }
        }

        void StartPreview()
        {
            isPlaying = true;
            currentPlayingIndex = 0;
            nextSubtitleTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += OnEditorUpdate;
        }

        void StopPreview()
        {
            isPlaying = false;
            EditorApplication.update -= OnEditorUpdate;

            if (subtitlePlayer.SubtitleText != null)
            {
                subtitlePlayer.SubtitleText.text = "";
            }
        }

        void OnEditorUpdate()
        {
            if (!isPlaying)
                return;

            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime >= nextSubtitleTime && currentPlayingIndex < subtitles.Count)
            {
                Subtitle subtitle = subtitles[currentPlayingIndex];
                UpdateSubtitleText(subtitle);

                double duration = subtitle.Duration.TotalSeconds;
                nextSubtitleTime += duration;

                currentPlayingIndex++;

                if (currentPlayingIndex >= subtitles.Count)
                {
                    StopPreview();
                }
            }
        }
    }
}

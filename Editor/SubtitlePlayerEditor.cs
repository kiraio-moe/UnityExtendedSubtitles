using System.Collections.Generic;
using System.Threading.Tasks;
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
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.LabelField("Subtitle Preview", EditorStyles.boldLabel);

            if (subtitles != null && subtitles.Count > 0)
            {
                serializedObject.Update();

                // Display and update the slider for the subtitle index
                EditorGUI.BeginDisabledGroup(isPlaying);
                subtitleIndexProperty.intValue = EditorGUILayout.IntSlider(
                    "Subtitle Index",
                    subtitleIndexProperty.intValue,
                    0,
                    subtitles.Count - 1
                );
                Subtitle currentSubtitle = subtitles[subtitleIndexProperty.intValue];
                EditorGUILayout.LabelField(
                    "Start Time:",
                    currentSubtitle.Start.ToString(@"hh\:mm\:ss\.fff")
                );
                EditorGUILayout.LabelField(
                    "End Time:",
                    currentSubtitle.End.ToString(@"hh\:mm\:ss\.fff")
                );
                EditorGUILayout.LabelField(
                    "Duration:",
                    currentSubtitle.Duration.ToString(@"hh\:mm\:ss\.fff")
                );
                EditorGUILayout.LabelField("Position:", currentSubtitle.Position.ToString());
                EditorGUILayout.LabelField("Text:");
                EditorGUILayout.TextArea(currentSubtitle.Text, GUILayout.Height(50));
                UpdateSubtitleText(currentSubtitle);
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();

                // Button to play all subtitles from start to end
                if (GUILayout.Button(isPlaying ? "Stop Preview" : "Preview All Subtitles"))
                {
                    if (isPlaying)
                        StopPreview();
                    else
                        StartPreview();
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.HelpBox("No subtitles found.", MessageType.Warning);
            }
        }

        async void StartPreview()
        {
            isPlaying = true;
            int currentPlayingIndex = 0;

            while (isPlaying && currentPlayingIndex < subtitles.Count)
            {
                UpdateSubtitleText(Subtitle.Empty);
                Subtitle currentSubtitle = subtitles[currentPlayingIndex];
                UpdateSubtitleText(currentSubtitle);

                // Wait for the duration of the current subtitle
                await Task.Delay((int)currentSubtitle.Duration.TotalMilliseconds);

                currentPlayingIndex++;

                // Update the subtitle index in the editor
                subtitleIndexProperty.intValue = currentPlayingIndex;
                serializedObject.ApplyModifiedProperties();
            }

            StopPreview();
        }

        void StopPreview()
        {
            isPlaying = false;
            subtitleIndexProperty.intValue = 0;
            serializedObject.ApplyModifiedProperties();
        }

        void UpdateSubtitleText(Subtitle subtitle)
        {
            if (subtitlePlayer.SubtitleText != null)
                subtitlePlayer.UpdateSubtitleText(subtitle);
            else
            {
                Debug.LogWarning(
                    "Subtitle TextMeshProUGUI is not assigned in the SubtitlePlayer script."
                );
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Kiraio.UniXSub.Parser;
using TMPro;
using UnityEngine;

namespace Kiraio.UniXSub.Components
{
    [AddComponentMenu("UniXSub/Subtitle Player")]
    public class SubtitlePlayer : MonoBehaviour
    {
        [SerializeField]
        TMP_Text m_SubtitleText;

        [SerializeField]
        TextAsset m_SubtitleAsset;

        [SerializeField]
        bool m_Autoplay = true;

        [SerializeField, HideInInspector]
        int currentSubtitleIndex = 0;
        List<Subtitle> subtitles;
        Coroutine playCoroutine;

        public TMP_Text SubtitleText
        {
            get
            {
                if (m_SubtitleText == null)
                {
                    Debug.LogError("Subtitle Text field is empty!");
                    return null;
                }
                return m_SubtitleText;
            }
            set => m_SubtitleText = value;
        }

        public TextAsset SubtitleAsset
        {
            get
            {
                if (m_SubtitleAsset == null)
                {
                    Debug.LogWarning("Subtitle Asset field is empty.");
                    return null;
                }
                return m_SubtitleAsset;
            }
            set => m_SubtitleAsset = value;
        }

        public bool Autoplay
        {
            get => m_Autoplay;
            set => m_Autoplay = value;
        }

        public int SubtitleIndex => currentSubtitleIndex;
        public List<Subtitle> Subtitles => subtitles;

        void Start()
        {
            SubtitleText.text = string.Empty;
            subtitles = SrtParser.ParseSrt(SubtitleAsset.text);

            if (subtitles == null || subtitles.Count <= 0)
            {
                Debug.LogWarning("No subtitles found or failed to load!");
                return;
            }

            if (Autoplay)
                PlaySubtitles();
        }

        public void PlaySubtitles()
        {
            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
            }
            playCoroutine = StartCoroutine(PlaySubtitlesCoroutine());
        }

        IEnumerator PlaySubtitlesCoroutine()
        {
            while (currentSubtitleIndex < subtitles.Count)
            {
                Subtitle subtitle = subtitles[currentSubtitleIndex];
                SubtitleText.text = subtitle.Text;

                yield return new WaitForSeconds((float)subtitle.Duration.TotalSeconds);

                currentSubtitleIndex++;
            }

            SubtitleText.text = string.Empty;
        }

        public void StopSubtitles()
        {
            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
                SubtitleText.text = string.Empty;
            }
        }
    }
}
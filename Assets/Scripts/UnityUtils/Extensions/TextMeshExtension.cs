using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UnityUtils.Extensions
{
    public static class TextMeshExtension
    {
        public static void AnimateText(this TextMeshProUGUI textMesh, Vector3 targetScale, string text,
            Color color, float duration = 1, Ease ease = Ease.OutBounce, Action onComplete = null)
        {
            textMesh.text = text;
            textMesh.DoAnimateText(duration, ease, targetScale, color, onComplete);
        }
        
        public static void AnimateText(this TextMeshProUGUI textMesh, Vector3 targetScale, string text,
            float duration = 1, Ease ease = Ease.OutBounce, Action onComplete = null)
        {
            textMesh.text = text;
            textMesh.DoAnimateText(duration, ease, targetScale, textMesh.color, onComplete);
        }

        private static void DoAnimateText(this TextMeshProUGUI textMesh, float duration, Ease ease,
            Vector3 targetScale, Color color, Action onComplete = null)
        {
            var startScale = textMesh.transform.localScale;
            var startColor = textMesh.color;
            
            float startTime = duration / 5f;
            float endTime = duration * 4 / 5f;
            
            textMesh.DOColor(color, startTime).SetEase(ease).OnComplete(() => {
                textMesh.DOColor(startColor, endTime);
            });
            
            if (onComplete != null)
            {
                textMesh.transform.DOScale(targetScale, duration / 2).SetEase(ease).OnComplete(() => {
                    textMesh.transform.DOScale(startScale, duration / 2).OnComplete(() => {
                        onComplete();
                    });
                });
            }
            else
            {
                textMesh.transform.DOScale(targetScale, duration / 2).SetEase(ease).OnComplete(() => {
                    textMesh.transform.DOScale(startScale, duration / 2);
                });
            }
        }
    }
}
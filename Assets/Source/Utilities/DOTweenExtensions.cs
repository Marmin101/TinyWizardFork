using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Quinn
{
	public static class DOTweenExtensions
	{
		public static TweenerCore<float, float, FloatOptions> DOAnimateFloat(this Material material, string propertyName, float endValue, float duration)
		{
			return DOTween.To(() => material.GetFloat(propertyName), x => material.SetFloat(propertyName, x), endValue, duration);
		}
	}
}

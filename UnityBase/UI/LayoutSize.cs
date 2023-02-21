using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace UnityBase.UI
{
	// https://answers.unity.com/questions/1838268/force-layout-element-min-width-to-be-the-same-as-p.html
	[RequireComponent(typeof(RectTransform))]
	public class LayoutSize : MonoBehaviour, ILayoutElement
	{
		private bool _layoutGroupNotFound = true;
		[SerializeField][EnableIf("_layoutGroupFound")]
		private LayoutGroup referenceLayoutGroup;
		[SerializeField]
		private bool minWidthIsPreferredWidth = false;
		[SerializeField]
		private bool minHeightIsPreferredHeight = false;
		[SerializeField]
		private int priority = 3;

		private void OnValidate()
		{
			var layoutGroup = GetComponent<LayoutGroup>();
			if (layoutGroup == null) {
				_layoutGroupNotFound = true;
			} else {
				_layoutGroupNotFound = false;
				referenceLayoutGroup = layoutGroup;
			}
		}

		public float minWidth
		{
			get
			{
				if(minWidthIsPreferredWidth)
				{
					return referenceLayoutGroup.preferredWidth;
				} else {
					return -1;
				}
			}
		}
 
		public float preferredWidth { get { return -1; } } // -1 allows other layout elements with lower priority to specify t$$anonymous$$s value
 
		public float flexibleWidth { get { return -1; } }
 
		public float minHeight
		{
			get
			{
				if(minHeightIsPreferredHeight)
				{
					return referenceLayoutGroup.preferredHeight;
				} else {
					return -1;
				}
			}
		}
 
		public float preferredHeight { get { return -1; } }
 
		public float flexibleHeight { get { return -1; } }
 
		public int layoutPriority { get { return priority; } }
 
		public void CalculateLayoutInputHorizontal() { }
 
		public void CalculateLayoutInputVertical() { }
	}
}

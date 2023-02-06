using System.Text;
using UnityEngine;

namespace UnityBase.Inspector
{
	public class PrintActiveGameobjects : MonoBehaviour
	{
		private void Start()
		{
			StringBuilder s = new();
			foreach (Transform childTransform in transform) {
				var obj = childTransform.gameObject;
				if (obj.activeInHierarchy) s.Append($"{obj.name}\n");
			}

			Debug.LogWarning(s.ToString());
		}
	}
}

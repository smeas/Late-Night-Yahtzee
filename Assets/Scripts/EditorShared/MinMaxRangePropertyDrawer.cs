/*
 * Author: Jonatan Johansson
 * Created: 2020-12-04
 */

using UnityEditor;
using UnityEngine;

/// <summary>
/// Display a min max slider for a Vector2 field in the inspector.
/// </summary>
public class MinMaxRangeAttribute : PropertyAttribute {
	public float Min { get; }
	public float Max { get; }

	public MinMaxRangeAttribute(float min, float max) {
		Min = min;
		Max = max;
	}
}


#if UNITY_EDITOR
namespace PropertyDrawers {
	[CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
	public class MinMaxRangePropertyDrawer : PropertyDrawer {
		private const float FieldSize = 45f;
		private const float FieldMargin = 5f;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (property.propertyType != SerializedPropertyType.Vector2) {
				Debug.LogError($"{nameof(MinMaxRangeAttribute)} is only supported on fields of type Vector2.");
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}

			MinMaxRangeAttribute attr = (MinMaxRangeAttribute)attribute;
			Vector2 value = property.vector2Value;

			label = EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position,label);
			Rect minFieldPos = position;
			Rect maxFieldPos = position;
			Rect sliderPos = position;

			minFieldPos.xMax = minFieldPos.xMin + FieldSize;
			maxFieldPos.xMin = maxFieldPos.xMax - FieldSize;
			sliderPos.xMin = minFieldPos.xMax + FieldMargin;
			sliderPos.xMax = maxFieldPos.xMin - FieldMargin;

			value.x = Mathf.Clamp(EditorGUI.FloatField(minFieldPos, value.x), attr.Min, value.y);
			EditorGUI.MinMaxSlider(sliderPos, ref value.x, ref value.y, attr.Min, attr.Max);
			value.y = Mathf.Clamp(EditorGUI.FloatField(maxFieldPos, value.y), value.x, attr.Max);

			EditorGUI.EndProperty();

			property.vector2Value = value;
		}
	}
}
#endif
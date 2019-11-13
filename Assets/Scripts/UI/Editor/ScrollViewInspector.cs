using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(ScrollView), true)]
public class ScrollViewInspector : Editor
{
    int index = 0;
    float speed = 1000;
	public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        ScrollView scroll = (ScrollView)target;
        GUI.enabled = Application.isPlaying;

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Clear"))
        {
            scroll.Clear();
        }
        if (GUILayout.Button("Refresh"))
        {
            scroll.Refresh();
		}
		if(GUILayout.Button("Refill"))
		{
			scroll.Refill();
		}
		if(GUILayout.Button("RefillFromEnd"))
		{
			scroll.RefillFromEnd();
		}
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 45;
        float w = (EditorGUIUtility.currentViewWidth - 100) / 2;
        EditorGUILayout.BeginHorizontal();
        index = EditorGUILayout.IntField("Index", index, GUILayout.Width(w));
        speed = EditorGUILayout.FloatField("Speed", speed, GUILayout.Width(w));
        if(GUILayout.Button("Scroll", GUILayout.Width(45)))
        {
            scroll.SrollTo(index, speed);
        }
        EditorGUILayout.EndHorizontal();
	}
}
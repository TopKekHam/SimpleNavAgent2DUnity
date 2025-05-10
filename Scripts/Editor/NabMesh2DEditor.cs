using UnityEditor;
using UnityEngine;

namespace JRPGNavAgent2D
{
    [CustomEditor(typeof(NavMesh2D))]
    public class NavMesh2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            NavMesh2D navMesh2d = (NavMesh2D)target;

            DrawDefaultInspector();

            if(GUILayout.Button("Bake"))
            {
                navMesh2d.Bake();
            }
        }
    }
}
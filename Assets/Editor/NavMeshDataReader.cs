using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEngine.AI;
using System.Collections.Generic;

public class NavMeshDataOffMeshLinksExplorerAndCreator : EditorWindow
{
    private NavMeshData navMeshData;
    private Vector2 scrollPosition;
    private string explorationResult = "";
    private List<OffMeshLinkData> offMeshLinks = new List<OffMeshLinkData>();

    [MenuItem("Tools/NavMesh Data Off-Mesh Links Explorer and Creator")]
    public static void ShowWindow()
    {
        GetWindow<NavMeshDataOffMeshLinksExplorerAndCreator>("NavMesh Data Off-Mesh Links Explorer and Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("NavMesh Data Off-Mesh Links Explorer and Creator", EditorStyles.boldLabel);

        navMeshData = EditorGUILayout.ObjectField("NavMesh Data Asset", navMeshData, typeof(NavMeshData), false) as NavMeshData;

        if (GUILayout.Button("Explore Off-Mesh Links"))
        {
            if (navMeshData != null)
            {
                ExploreOffMeshLinks();
            }
            else
            {
                Debug.LogWarning("Please assign a NavMeshData asset first.");
            }
        }

        if (GUILayout.Button("Create Off-Mesh Links in Scene"))
        {
            CreateOffMeshLinksInScene();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        if (!string.IsNullOrEmpty(explorationResult))
        {
            EditorGUILayout.TextArea(explorationResult, GUILayout.ExpandHeight(true));
        }
        EditorGUILayout.EndScrollView();
    }

    private void ExploreOffMeshLinks()
    {
        StringBuilder sb = new StringBuilder();
        SerializedObject serializedObject = new SerializedObject(navMeshData);
        SerializedProperty offMeshLinksProperty = serializedObject.FindProperty("m_OffMeshLinks");

        offMeshLinks.Clear();

        if (offMeshLinksProperty != null && offMeshLinksProperty.isArray)
        {
            sb.AppendLine($"Number of Off-Mesh Links: {offMeshLinksProperty.arraySize}");
            sb.AppendLine();

            for (int i = 0; i < offMeshLinksProperty.arraySize; i++)
            {
                SerializedProperty linkProperty = offMeshLinksProperty.GetArrayElementAtIndex(i);
                sb.AppendLine($"Off-Mesh Link {i + 1}:");
                OffMeshLinkData linkData = ExploreOffMeshLinkProperties(linkProperty, sb);
                offMeshLinks.Add(linkData);
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("Could not find m_OffMeshLinks property or it's not an array.");
        }

        explorationResult = sb.ToString();
        Debug.Log(explorationResult);
    }

    private OffMeshLinkData ExploreOffMeshLinkProperties(SerializedProperty linkProperty, StringBuilder sb)
    {
        OffMeshLinkData linkData = new OffMeshLinkData();
        SerializedProperty iterator = linkProperty.Copy();
        SerializedProperty endProperty = iterator.GetEndProperty();

        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
        {
            string value = GetPropertyValue(iterator);
            sb.AppendLine($"  {iterator.name}: {value}");
            
            switch (iterator.name)
            {
                case "m_Start":
                    linkData.start = ParseVector3(value);
                    break;
                case "m_End":
                    linkData.end = ParseVector3(value);
                    break;
                case "m_Radius":
                    linkData.radius = float.Parse(value);
                    break;
                case "m_LinkType":
                    linkData.linkType = int.Parse(value);
                    break;
                case "m_Area":
                    linkData.area = int.Parse(value);
                    break;
                case "m_LinkDirection":
                    linkData.linkDirection = int.Parse(value);
                    break;
            }

            enterChildren = false;
        }

        return linkData;
    }

    private void CreateOffMeshLinksInScene()
    {
        if (offMeshLinks.Count == 0)
        {
            Debug.LogWarning("No Off-Mesh Links data available. Please explore the NavMeshData first.");
            return;
        }

        foreach (var linkData in offMeshLinks)
        {
            GameObject linkObject = new GameObject($"OffMeshLink_{linkData.start}_{linkData.end}");
            OffMeshLink offMeshLink = linkObject.AddComponent<OffMeshLink>();

            offMeshLink.startTransform = CreateEndpoint(linkObject, linkData.start, "Start");
            offMeshLink.endTransform = CreateEndpoint(linkObject, linkData.end, "End");

            offMeshLink.costOverride = -1f; // Use NavMesh cost
            offMeshLink.biDirectional = true;
            offMeshLink.area = linkData.area;
            
            
            //offMeshLink.agentTypeID = linkData.linkType;

            // Set the radius (Note: OffMeshLink doesn't have a radius property, so we're using localScale)
            // float scale = linkData.radius * 2;
            // linkObject.transform.localScale = new Vector3(scale, scale, scale);
        }

        Debug.Log($"Created {offMeshLinks.Count} Off-Mesh Links in the scene.");
    }

    private Transform CreateEndpoint(GameObject parent, Vector3 position, string name)
    {
        GameObject endpointObject = new GameObject(name);
        endpointObject.transform.SetParent(parent.transform);
        endpointObject.transform.position = position;
        return endpointObject.transform;
    }

    private string GetPropertyValue(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                return property.intValue.ToString();
            case SerializedPropertyType.Boolean:
                return property.boolValue.ToString();
            case SerializedPropertyType.Float:
                return property.floatValue.ToString();
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Vector2:
                return property.vector2Value.ToString();
            case SerializedPropertyType.Vector3:
                return property.vector3Value.ToString();
            case SerializedPropertyType.Enum:
                return property.enumValueIndex.ToString();
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue ? property.objectReferenceValue.name : "None";
            default:
                return "(Not printable)";
        }
    }

    private Vector3 ParseVector3(string value)
    {
        string[] components = value.Trim('(', ')').Split(',');
        return new Vector3(
            float.Parse(components[0]),
            float.Parse(components[1]),
            float.Parse(components[2])
        );
    }

    private class OffMeshLinkData
    {
        public Vector3 start;
        public Vector3 end;
        public float radius;
        public int linkType;
        public int area;
        public int linkDirection;
    }
}
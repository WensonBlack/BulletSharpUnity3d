﻿using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using BulletUnity;

[CustomEditor(typeof(BGhostObject))]
public class BGhostObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BGhostObject obj = (BGhostObject) target;

        obj.m_collisionFlags = BCollisionObjectEditor.RenderEnumMaskCollisionFlagsField(BCollisionObjectEditor.gcCollisionFlags, obj.m_collisionFlags);
        obj.m_groupsIBelongTo = BCollisionObjectEditor.RenderEnumMaskCollisionFilterGroupsField(BCollisionObjectEditor.gcGroupsIBelongTo, obj.m_groupsIBelongTo);
        obj.m_collisionMask = BCollisionObjectEditor.RenderEnumMaskCollisionFilterGroupsField(BCollisionObjectEditor.gcCollisionMask, obj.m_collisionMask);


        if (GUI.changed)
        {
            EditorUtility.SetDirty(obj);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Undo.RecordObject(obj, "Undo Rigid Body");
        }
    }
}
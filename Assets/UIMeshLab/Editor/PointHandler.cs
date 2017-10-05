﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PointHandler {

    /// Line 
    ///     List<Node> points 
    ///          Vector3 position;
    ///          Vector3 previousControlOffset;
    ///          Vector3 nextControlOffset;
    ///          Color color;
    ///          float width;
    ///          float angle;
    ///          int nextDivieCount;
    ///          bool loop;
    ///     float startRatio
    ///     float endRatio

    readonly SerializedProperty _points;
    readonly Component _owner;
    
    public PointHandler(SerializedProperty points, Component owner)
    {
        _points = points;
        _owner = owner;
    }
    
    public void OnSceneGUI()
    {
        for (int i = 0; i < _points.arraySize; i++)
        {
            var node = _points.GetArrayElementAtIndex(i);
            var position = node.FindPropertyRelative("position");
            HandleControlPoint(i, node);
            HandlePoint(i, position);
        }
    }

    private void HandleControlPoint(int n, SerializedProperty node)
    {
        var pos = _owner.transform.TransformPoint(node.FindPropertyRelative("position").vector3Value);
        var prevCOffset = node.FindPropertyRelative("previousControlOffset");
        var prevNode = _points.GetArrayElementAtIndex(n - 1 < 0 ? _points.arraySize - 1 : n - 1);
        var prevPosition = _owner.transform.TransformPoint(prevNode.FindPropertyRelative("position").vector3Value);
        var prevDirection = (prevPosition - pos).normalized;
     
        var buffer = Handles.color;
        Handles.color = Color.blue;
        if (prevCOffset.vector3Value.magnitude < 1f)
        {
            prevCOffset.vector3Value = Vector3.zero;
            if (Handles.Button(pos + prevDirection * 5, _owner.transform.rotation, 1, 1, Handles.DotHandleCap))
            {
                var mid = (pos + prevPosition)/2f;
                prevCOffset.vector3Value = mid - pos;
                _points.serializedObject.ApplyModifiedProperties();
            }
        }
        else
        {
            Handles.DrawDottedLine(pos, pos + prevCOffset.vector3Value, 5f);
            HandleOffset(n, prevCOffset, pos);
        }
        Handles.color = buffer;


        var nextCOffset = node.FindPropertyRelative("nextControlOffset");
        var nextNode = _points.GetArrayElementAtIndex(n + 1 == _points.arraySize ? 0 : n + 1);
        var nextPosition = _owner.transform.TransformPoint(nextNode.FindPropertyRelative("position").vector3Value);
        var nextDirection = (nextPosition - pos).normalized;
        buffer = Handles.color;
        Handles.color = Color.red;
        if (nextCOffset.vector3Value.magnitude < 1f)
        {
            nextCOffset.vector3Value = Vector3.zero;
            if (Handles.Button(pos + nextDirection * 5, _owner.transform.rotation, 1, 1, Handles.DotHandleCap))
            {
                var mid = (pos + nextPosition) / 2f;
                nextCOffset.vector3Value = mid - pos;
                _points.serializedObject.ApplyModifiedProperties();
            }
        }
        else
        {
            Handles.DrawDottedLine(pos, pos + nextCOffset.vector3Value, 5f);
            HandleOffset(n, nextCOffset, pos);
        }
        Handles.color = buffer;
    }

    private void HandleOffset(int n, SerializedProperty offset, Vector3 position)
    {
        EditorGUI.BeginChangeCheck();
        var pos = position + offset.vector3Value;
        var changedPosition = Handles.DoPositionHandle(pos, _owner.transform.rotation);
        Handles.Label(pos, n + " CP");
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_owner, "edit offset");
            offset.vector3Value = changedPosition - position;
            offset.serializedObject.ApplyModifiedProperties();
        }
    }

    private void HandlePoint(int n,SerializedProperty position)
    {
        EditorGUI.BeginChangeCheck();
        var pos = _owner.transform.TransformPoint(position.vector3Value);
        var changedPosition = Handles.DoPositionHandle(pos, _owner.transform.rotation);
        Handles.Label(pos, n + " point");
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_owner, "edit point");
            position.vector3Value = _owner.transform.InverseTransformPoint(changedPosition);
            position.serializedObject.ApplyModifiedProperties();
        }
    }
}
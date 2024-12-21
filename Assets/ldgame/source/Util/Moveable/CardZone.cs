﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CardZone : MonoBehaviour
{
    public List<InteractiveObject> objects = new List<InteractiveObject>();
    public bool canDrag;
    public float width;

    public UnityAction<InteractiveObject> OnClickDice;
    List<InteractiveObject> alignedSet = new List<InteractiveObject>();

    public void Claim(InteractiveObject toClaim)
    {
        if (toClaim.zone != null)
            toClaim.zone.Release(toClaim);

        toClaim.zone = this;

        objects.Insert(0, toClaim);
    }

    public void Release(InteractiveObject toClaim)
    {
        if (objects.Contains(toClaim))
            objects.Remove(toClaim);
    }


    void Update()
    {
        alignedSet.Clear();
        for (var index = 0; index < objects.Count; index++)
        {
            var o = objects[index];
            // var isKindaBack = Vector2.Distance(objects[index].transform.position, GetTargetPos(index, objects)) < 0.5f;
            if (!o.draggable.isDragging /* || isKindaBack*/)
            {
                alignedSet.Add(o);
            }
        }

        for (var i = 0; i < alignedSet.Count; i++)
        {
            var targetPos = GetTargetPos(i, alignedSet);
            alignedSet[i].moveable.targetPosition = targetPos;
            alignedSet[i].order = i;
        }
    }

    Vector3 GetTargetPos(int i, List<InteractiveObject> setToWatch)
    {
        float totalOffset = 0f;

        // Calculate total offset by summing half the width of the current object and the previous objects' widths
        for (int j = 0; j < i; j++)
        {
            totalOffset += setToWatch[j].Width;
        }

        // Offset the current object by half of its own width for proper centering
        totalOffset += setToWatch[i].Width / 2f;

        // Calculate the current object position centered around the full set
        //float totalWidth = Math.Min(GetTotalSetWidth(setToWatch), width);
        float totalWidth = GetTotalSetWidth(setToWatch);
        float centeredOffset = totalOffset - (totalWidth / 2f);
    
        // Return the new target position, taking into account spacing
        return transform.position + Vector3.right * centeredOffset;
    }

    float GetTotalSetWidth(List<InteractiveObject> setToWatch)
    {
        float totalWidth = 0f;
        foreach (var obj in setToWatch)
        {
            totalWidth += obj.Width;
        }
        return totalWidth;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0, 0, 0.25f);
        //Gizmos.DrawCube(transform.position, new Vector3(width, 100, 0));
        Gizmos.DrawWireCube(transform.position, new Vector3(width, 300, 0));
    }

    public InteractiveObject LastDice()
    {
        if (objects.Count <= 1)
            return null;

        return objects[1];
    }

    public InteractiveObject FrontDice()
    {
        if (objects.Count == 0)
            return null;

        return objects[^1];
    }

    public InteractiveObject GetNextDice(InteractiveObject interactiveObject)
    {
        var iof = objects.IndexOf(interactiveObject);
        if (iof + 1 < objects.Count)
            return objects[iof + 1];
        return null;
    }

    public IEnumerator Clear(bool soft = false)
    {
        var interactiveObjects = new List<InteractiveObject>(objects);
        foreach (var f in interactiveObjects)
        {
            if (soft && f.state.model.Is<TagThriving>())
            {
                continue;
            }
            else
            {
                yield return G.main.KillDice(f.state);
            }
        }
    }
}
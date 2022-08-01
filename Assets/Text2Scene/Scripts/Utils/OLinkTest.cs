using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OLinkTest
{

    public static string AbsoluteRelationCheck(GameObject figure, GameObject ground)
    {
        GameObject m_figure = figure;
        GameObject m_ground = ground;

        BoxCollider figure_collider = figure.GetComponent<BoxCollider>();
        BoxCollider ground_collider = ground.GetComponent<BoxCollider>();

        if (figure_collider.bounds.size.magnitude > ground_collider.bounds.size.magnitude * 2)
        {
            m_figure = ground;
            m_ground = figure;
        }
        Vector3 groundfront = m_ground.transform.forward;


        m_ground.transform.forward = Vector3.forward;

        string relation = AbsoluteRelationCheck_(m_ground, m_figure);
        m_ground.transform.forward = groundfront;
        return relation;
    }
    private static string AbsoluteRelationCheck_(GameObject figure, GameObject ground)
    {
        RaycastHit[] hit;
        BoxCollider ground_collider = ground.GetComponent<BoxCollider>();

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, Vector3.forward, ground.transform.rotation, ground_collider.bounds.size.magnitude, 19);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "North";

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, Quaternion.Euler(0, -45, 0) * Vector3.forward, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "NorthEast";


        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, Quaternion.Euler(0, -90, 0) * Vector3.forward, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "East";

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, Quaternion.Euler(0, -135, 0) * Vector3.forward, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "SouthEast";

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, Quaternion.Euler(0, -180, 0) * Vector3.forward, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "South";

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, Quaternion.Euler(0, -225, 0) * Vector3.forward, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "SouthWest";

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, Quaternion.Euler(0, -270, 0) * Vector3.forward, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "West";

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, Quaternion.Euler(0, -315, 0) * Vector3.forward, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "NorthWest";

        return null;
    }

    
    public static string RelationCheck(GameObject figure, GameObject ground, bool minimal, GameObject referencePt = null)
    {
        GameObject m_figure = figure;
        GameObject m_ground = ground;

        BoxCollider figure_collider = figure.GetComponent<BoxCollider>();
        BoxCollider ground_collider = ground.GetComponent<BoxCollider>();

        if (figure_collider.bounds.size.magnitude > ground_collider.bounds.size.magnitude * 1.3f)
        {
            m_figure = ground;
            m_ground = figure;
        }
        Vector3 groundfront = m_ground.transform.forward;

        Vector3 m_front;
        if (referencePt != null)
            m_front = (referencePt.transform.position - m_ground.transform.position).normalized;
        else
            m_front = m_ground.transform.forward;
        m_ground.transform.forward = m_front;

        string relation = RelationCheck(m_ground, m_figure, minimal);
        m_ground.transform.forward = groundfront;
        return relation;
    }


    
    private static string RelationCheck(GameObject ground, GameObject figure, bool minimal)
    {
        RaycastHit[] hit;
        BoxCollider ground_collider = ground.GetComponent<BoxCollider>();

        ExtDebug.DrawBoxCastBox(ground.transform.position, ground_collider.bounds.extents * 0.9f, ground.transform.rotation, ground.transform.up, ground_collider.bounds.size.magnitude, Color.green);
        ExtDebug.DrawBoxCastBox(ground.transform.position, ground_collider.bounds.extents * 0.9f, ground.transform.rotation, ground.transform.forward, ground_collider.bounds.size.magnitude, Color.red);
        ExtDebug.DrawBoxCastBox(ground.transform.position, ground_collider.bounds.extents * 0.9f, ground.transform.rotation, ground.transform.right, ground_collider.bounds.size.magnitude, Color.blue);



        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, ground.transform.forward, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure.gameObject)
                return "IN_FRONT_OF";

        
        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, -ground.transform.forward, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "BEHIND";

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, ground.transform.right, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "NEXT_TO";

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, -ground.transform.right, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "NEXT_TO";

        if (minimal)
            return null;

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, ground.transform.up, ground.transform.rotation, ground_collider.bounds.size.magnitude / 10f);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "ON";

        
        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, ground.transform.up, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "ABOVE";

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, -ground.transform.up, ground.transform.rotation, ground_collider.bounds.size.magnitude / 10f);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "BENEATH";

        hit = Physics.BoxCastAll(ground.transform.position, ground_collider.bounds.extents * 0.9f, -ground.transform.up, ground.transform.rotation, ground_collider.bounds.size.magnitude);
        foreach (RaycastHit _hit in hit)
            if (_hit.transform.gameObject == figure)
                return "UNDER";
        
        return null;
    }
    
}

﻿using System;
using UnityEngine;
using System.Collections;
using BulletSharp;
using BM = BulletSharp.Math;

namespace BulletUnity {
    [System.Serializable]
    public class BFixedConstraint : BTypedConstraint {

        //todo should be properties so can capture changes and propagate to scene
        [Header("Reference Frame Local To This Object")]
        public Vector3 m_localConstraintPoint = Vector3.zero;
        public Vector3 m_localConstraintForwardDir = Vector3.forward;
        public Vector3 m_localConstraintUpDir = Vector3.up;

        //called by Physics World just before constraint is added to world.
        //the current constraint properties are used to rebuild the constraint.
        internal override bool _BuildConstraint() {
            if (m_constraintType == ConstraintType.constrainToPointInSpace)
            {
                Debug.LogError("A FixedConstraint can only be constrained to another object.");
                return false;
            }
            BPhysicsWorld world = BPhysicsWorld.Get();
            if (m_constraintPtr != null) {
                if (m_isInWorld && world != null) {
                    m_isInWorld = false;
                    world.RemoveConstraint(m_constraintPtr);
                }
            }
            BRigidBody targetRigidBodyA = GetComponent<BRigidBody>();
            if (targetRigidBodyA == null)
            {
                Debug.LogError("Fixed Constraint needs to be added to a component with a BRigidBody.");
                return false;
            }
            if (!targetRigidBodyA.isInWorld)
            {
                world.AddRigidBody(targetRigidBodyA);
            }
            RigidBody rba = (RigidBody) targetRigidBodyA.GetCollisionObject();
            if (rba == null) {
                Debug.LogError("Constraint could not get bullet RigidBody from target rigid body");
                return false;
            }

            if (m_otherRigidBody == null)
            {
                Debug.LogError("Other rigid body is not set");
                return false;
            }
            if (!m_otherRigidBody.isInWorld)
            {
                world.AddRigidBody(m_otherRigidBody);
            }
            RigidBody rbb = (RigidBody) m_otherRigidBody.GetCollisionObject();
            if (rbb == null)
            {
                Debug.LogError("Constraint could not get bullet RigidBody from target rigid body");
                return false;
            }
            BM.Matrix frameInA, frameInOther;
            string errormsg = "";
            if (CreateFramesA_B(m_localConstraintForwardDir, m_localConstraintUpDir, m_localConstraintPoint, out frameInA, out frameInOther, ref errormsg))
            {
                m_constraintPtr = new FixedConstraint(rbb, rba, frameInOther, frameInA);
            } else
            {
                Debug.LogError(errormsg);
                return false;
            }
            m_constraintPtr.Userobject = this;

            return true;
        }
    }
}

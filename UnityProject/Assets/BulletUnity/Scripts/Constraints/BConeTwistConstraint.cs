﻿using System;
using UnityEngine;
using System.Collections;
using BulletSharp;
using BM = BulletSharp.Math;

namespace BulletUnity {
    //TODO script execution order, rigid bodies before constraints & other stuff that might use them
    [System.Serializable]
    public class BConeTwistConstraint : BTypedConstraint {
        public static string HelpMessage = "btConeTwistConstraint can be used to simulate ragdoll joints (upper arm, leg etc)." +
                                            "It is a fixed translation, 3 degree-of-freedom (DOF) rotational 'joint'."+
                                            "It divides the 3 rotational DOFs into swing (movement within a cone) and twist."+
                                            "Swing is divided into swing1 and swing2 which can have different limits, giving an elliptical shape."+
                                            "(Note: the cone's base isn't flat, so this ellipse is 'embedded' on the surface of a sphere.)\n\n"+
                                            "In the contraint's frame of reference:\n"+
                                            "  - twist is along the forward\n"+
                                            "  - and swing 1 and 2 are along the right and up axes respectively.\n";


        [Header("Reference Frame Local To This Object")]
        public Vector3 m_localConstraintPoint = Vector3.zero;
        public Vector3 m_localConstraintForwardDir = Vector3.forward;
        public Vector3 m_localConstraintUpDir = Vector3.up;

        [Header("Limits")]
        public float m_swingSpan1Radians = Mathf.PI;
        public float m_swingSpan2Radians = Mathf.PI;
        public float m_twistSpanRadians = Mathf.PI;
        public float m_softness = .5f;
        public float m_biasFactor = .3f;
        public float m_relaxationFactor = 1f;

        //called by Physics World just before constraint is added to world.
        //the current constraint properties are used to rebuild the constraint.
        internal override bool _BuildConstraint() {
            BPhysicsWorld world = BPhysicsWorld.Get();
            if (m_constraintPtr != null) {
                if (m_isInWorld && world != null) {
                    m_isInWorld = false;
                    world.RemoveConstraint(m_constraintPtr);
                    return false;
                }
            }

            BRigidBody targetRigidBodyA = GetComponent<BRigidBody>();
            if (targetRigidBodyA == null)
            {
                Debug.LogError("ConeTwistConstraint needs to be added to a component with a BRigidBody.");
                return false;
            }
            if (!targetRigidBodyA.isInWorld)
            {
                world.AddRigidBody(targetRigidBodyA);
            }
            if (m_constraintType == ConstraintType.constrainToAnotherBody)
            {
                if (m_otherRigidBody == null)
                {
                    Debug.LogError("Other rigid body was not set");
                    return false;
                }
                if (!m_otherRigidBody.isInWorld)
                {
                    world.AddRigidBody(m_otherRigidBody);
                }
                BM.Matrix frameInA, frameInOther;
                string errormsg = "";
                if (CreateFramesA_B(m_localConstraintForwardDir, m_localConstraintUpDir, m_localConstraintPoint, out frameInA, out frameInOther, ref errormsg))
                {
                    m_constraintPtr = new ConeTwistConstraint((RigidBody)m_otherRigidBody.GetCollisionObject(), (RigidBody)targetRigidBodyA.GetCollisionObject(), frameInOther, frameInA);
                } else
                {
                    Debug.LogError(errormsg);
                    return false;
                }
                
            }
            else
            {
                //TODO this is broken
                string errormsg = "";
                BM.Matrix frameInB = BM.Matrix.Identity;
                if (CreateFrame(m_localConstraintForwardDir, m_localConstraintUpDir, m_localConstraintPoint, ref frameInB, ref errormsg))
                {
                    m_constraintPtr = new ConeTwistConstraint((RigidBody)targetRigidBodyA.GetCollisionObject(), frameInB);
                } else
                {
                    Debug.LogError(errormsg);
                    return false;
                }
            }
            ConeTwistConstraint sl = (ConeTwistConstraint)m_constraintPtr;

            sl.SetLimit(m_swingSpan1Radians, m_swingSpan2Radians, m_twistSpanRadians, m_softness, m_biasFactor, m_relaxationFactor);
            m_constraintPtr.Userobject = this;
            m_constraintPtr.DebugDrawSize = m_debugDrawSize;
            return true;
        }
    }
}


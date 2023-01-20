using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace _Chi.Scripts.Utilities
{
    public static class MMExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateTowards(
            quaternion from,
            quaternion to,
            float maxDegreesDelta)
        {
            float num = Angle(from, to);
            return num < float.Epsilon ? to : math.slerp(from, to, math.min(1f, maxDegreesDelta));
        }
     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(this quaternion q1, quaternion q2)
        {
            var dot    = math.dot(q1, q2);
            return !(dot > 0.999998986721039) ? (float) (math.acos(math.min(math.abs(dot), 1f)) * 2.0) : 0.0f;
        }
        
        public static float3 AngularVelocityToTarget(quaternion fromRotation, float3 toDirection, float turnSpeed, float3 up)
        {
            var wanted = quaternion.LookRotation(toDirection, up);
            wanted = math.normalizesafe(wanted);
            return AngularVelocityToTarget(fromRotation, wanted, turnSpeed);
        }
 
        public static float3 AngularVelocityToTarget(quaternion fromRotation, quaternion toRotation, float turnSpeed)
        {
            quaternion delta = math.mul(toRotation, math.inverse(fromRotation));
            delta = math.normalizesafe(delta);
 
            delta.ToAngleAxis(out float3 axis, out float angle);
 
            // We get an infinite axis in the event that our rotation is already aligned.
            if (float.IsInfinity(axis.x))
            {
                return default;
            }
 
            if (angle > 180f)
            {
                angle -= 360f;
            }
 
            // Here I drop down to 0.9f times the desired movement,
            // since we'd rather undershoot and ease into the correct angle
            // than overshoot and oscillate around it in the event of errors.
            return (math.radians(0.9f) * angle / turnSpeed) * math.normalizesafe(axis);
        }
        public static void ToAngleAxis(this quaternion q, out float3 axis, out float angle)
        {
            q = math.normalizesafe(q);
           
            angle = 2.0f * (float)math.acos(q.value.w);
            angle = math.degrees(angle);
            float den = (float)math.sqrt(1.0 - q.value.w * q.value.w);
            if (den > 0.0001f)
            {
                axis = q.value.xyz / den;
            }
            else
            {
                // This occurs when the angle is zero.
                // Not a problem: just set an arbitrary normalized axis.
                axis = new float3(1, 0, 0);
            }
        }
    }
}
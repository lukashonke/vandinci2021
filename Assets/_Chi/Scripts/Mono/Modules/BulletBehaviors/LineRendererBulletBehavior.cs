using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is supported by the BulletPro package for Unity.
// Template author : Simon Albou <albou.simon@gmail.com>

// This script is actually a MonoBehaviour for coding advanced things with Bullets.
public class LineRendererBulletBehavior : BaseBulletBehaviour
{
    public TrailRenderer trail;
    
    public override void Awake()
    {
        base.Awake();
    }

    public override void OnBulletBirth()
    {
        base.OnBulletBirth();
        
        trail.Clear();
    }
}

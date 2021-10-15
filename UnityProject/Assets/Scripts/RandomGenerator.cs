using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGenerator : MonoBehaviour
{
    #region MRG32k3a
    /*
        Récurrence à 3 termes :
        Xn = (ax * Xn-2 - bx * Xn-3) % mx;
        Yn = (ay * Yn-1 - by * Yn-3) % my;
        Un = ((Xn - Yn) % mx) / mx;
    */

    [SerializeField]
    public bool BG = true;

    protected double x_1;
    protected double x_2;
    protected double x_3;
    
    protected double y_1;
    protected double y_2;
    protected double y_3;

    protected double mx = 4294967087.0;
    protected double ax = 1403580.0;
    protected double bx = 810728.0;

    protected double my = 4294944443.0;
    protected double ay = 527612.0;
    protected double by = 1370589.0;

    protected double x;
    protected double y;
    protected double u;

    [SerializeField][RangeAttribute(1,4294944443)]
    public double seed;

    public double rand(double min, double max)
    {
        return (U() * (max - min)) + min;
    }

    // returns U ~ U[0:1]
    public double MRG32k3a()
    {
        return U();
    }

    protected double X()
    {
        // Compute Xn
        x = (ax * x_2 - bx * x_3) % mx;

        // Update Xn-1, Xn-2 and Xn-3
        x_3 = x_2; x_2 = x_1; x_1 = x;

        return x;
    }

    protected double Y()
    {
        // Compute Yn
        y = (ay * y_1 - by * y_3) % my;

        // Update Yn-1, Yn-2 and Yn-3
        y_3 = y_2; y_2 = y_1; y_1 = y;

        return y;
    }

    protected double U()
    {
        // Compute Un
        u = (( X() - Y() ) % mx) / mx;
        if(u < 0) u += 1;

        return u;
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        x_1 = seed;
        x_2 = seed;
        x_3 = seed;
        y_1 = seed;
        y_2 = seed;
        y_3 = seed;
    }

    // FixedUpdate : deltaTime is constant
    void Update()
    {
        if(BG)
        {
            MRG32k3a();
        }
    }
}

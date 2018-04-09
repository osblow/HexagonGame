using System;
using System.Collections.Generic;

namespace Osblow.Net.Server
{
    public class Vector3
    {
        public static Vector3 up = new Vector3(0, 1, 0);
        public static Vector3 forward = new Vector3(0, 0, 1);
        public static Vector3 right = new Vector3(1, 0, 0);
        public static Vector3 left = new Vector3(-1, 0, 0);

        public static Vector3 one = new Vector3(1, 1, 1);
        public static Vector3 zero = new Vector3(0, 0, 0);

        public float x;
        public float y;
        public float z;

        public Vector3()
        {
            x = y = z = 0;
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        #region operator
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector3 operator *(Vector3 v, float m)
        {
            return new Vector3(v.x * m, v.y * m, v.z * m);
        }

        public static Vector3 operator /(Vector3 v, float m)
        {
            return new Vector3(v.x / m, v.y / m, v.z / m);
        }
        #endregion
    }
}

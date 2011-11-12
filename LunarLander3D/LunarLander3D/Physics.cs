using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunarLander3D
{
    static class Physics
    {
        static float gravity = -0.1f;

        static public void applyGravity(CModel model, float speed)
        {
            speed += gravity;
            //model.Position.Y += speed;
        }
    }
}

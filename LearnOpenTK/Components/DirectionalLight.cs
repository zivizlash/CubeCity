using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnOpenTK.Components;

public class DirectionalLight : IUpdatable, IDrawable
{
    public Vector3 Position { get; set; }

    private float _total;

    public void Update(float elapsed)
    {
        _total += elapsed;
    }

    public void Draw()
    {
        throw new NotImplementedException();
    }
}

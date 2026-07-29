/// <summary>
/// AOE here = Area of effect. AOE could have size = 1. 
/// So AOE doesn't neccesary mean it hit multiple target.
/// </summary>

// Maybe I have to change thing up? I think it more suitable to use AOE and Point&Click. 
// Not everything should be call AOE since it was misleading.
public abstract class AOE
{
    public bool size;
}

public abstract class BoxAOE: AOE
{
    
}

public abstract class CircleAOE: AOE
{
    
}
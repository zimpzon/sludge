using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    public static ParticleEmitter I;

    public ParticleSystem DustParticles;
    public ParticleSystem PillParticles;

    private void Awake()
    {
        I = this;
    }

    public void EmitDust(Vector3 pos, int count)
    {
        DustParticles.transform.position = pos;
        DustParticles.Emit(count);
    }

    public void EmitPills(Vector3 pos, int count)
    {
        PillParticles.transform.position = pos;
        PillParticles.Emit(count);
    }
}

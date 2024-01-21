using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    public static ParticleEmitter I;

    public ParticleSystem DustParticles;
    public ParticleSystem PillParticles;
    public ParticleSystem EnergyArmParticles;

    private void Awake()
    {
        I = this;
    }

    public void EmitDust(Vector3 pos, int count)
    {
        DustParticles.transform.position = pos;
        DustParticles.Emit(count);

        // hacky hacky
        int pillCount = count / 2;
        pillCount += Random.value > 0.9f ? 1 : 0;
        EmitPills(pos, pillCount);
    }

    public void EmitPills(Vector3 pos, int count)
    {
        PillParticles.transform.position = pos;
        PillParticles.Emit(count);
    }

    public void EmitEnergyArm(Vector3 pos, int count)
    {
        EnergyArmParticles.transform.position = pos;
        EnergyArmParticles.Emit(count);
    }
}

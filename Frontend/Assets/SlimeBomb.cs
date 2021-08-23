using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class SlimeBomb : SludgeObject
{
    public override EntityType EntityType => EntityType.SlimeBomb;

    public ParticleSystem HighlightParticles;
}

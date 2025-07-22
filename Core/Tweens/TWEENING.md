#### What is Tweening?

Tweening (from "in-betweening") is the process of generating intermediate frames between a start and an end state to create the appearance of smooth motion. Instead of manually updating an object's properties frame-by-frame, you simply define the start state, the end state, and the duration. The tweening system handles all the intermediate calculations.

This allows you to create complex, natural-looking animations with minimal code, letting you focus on the "what" (the animation's goal) rather than the "how" (the per-frame math).

#### Understanding Easing

Easing functions dictate the *rate of change* of a property over time, controlling the acceleration and deceleration of the animation. This is what gives an animation its "feel" and personality. For a visual demonstration of all easing functions, refer to the excellent resource [easings.net](https://easings.net/).

There are three primary families of easing curves:

*   **Ease In:** The animation starts slowly and accelerates towards the end. Effective for movements that should feel like they are launching or building momentum.
*   **Ease Out:** The animation starts quickly and decelerates towards the end. The most common type for general motion, as it produces a smooth, natural-looking arrival.
*   **Ease InOut:** The animation starts slowly, accelerates through the middle, and then decelerates towards the end. Produces the smoothest and most deliberate-feeling motion.

#### Easing Types: A Quick Guide

*   **Linear:** Constant speed. Feels robotic and mechanical.
*   **Sine:** A smooth, gentle curve. Very subtle and professional.
*   **Quadratic, Cubic, Quartic, Quintic:** Polynomial curves of increasing intensity (`t^2` to `t^5`). `Quadratic` is subtle, while `Quintic` is highly dramatic.
*   **Expo:** An extremely dramatic exponential curve. Creates very high-impact, energetic animations.
*   **Circ:** A smooth, soft curve based on a circle's arc.
*   **Back:** A playful, energetic curve that overshoots its target and then settles back, creating a bouncy or "cartoony" feel.

---

### Practical Usage Examples for NPCs and Projectiles

#### 1. Basic Animation with Easing

The `TweenTo` extension method should be called on the most direct reference to the object you wish to animate, such as the `Projectile` or `NPC` property.

**Goal:** Animate a projectile's scale to create a "pop-in" effect upon spawning.

```csharp
// In a ModProjectile's OnSpawn() hook.
public override void OnSpawn(IEntitySource source)
{
    Projectile.scale = 0.1f;
    // Call the extension method directly on the Projectile instance.
    Projectile.TweenTo(p => p.scale, 1f, 20)
              .SetEase(Easing.BackOut); // The "BackOut" ease creates a pleasing bounce effect.
}
```

#### 2. Delays and Completion Callbacks

**Goal:** An enraged boss flashes red, and after the flash, it enters a more aggressive AI state.

```csharp
// Assume this is a method within your ModNPC class.
public void TriggerEnrage()
{
    // The most direct reference to the Terraria NPC is the 'NPC' property.
    NPC.TweenTo(n => n.color, Color.Red, 10)
       .SetLoops(1, LoopType.Yoyo) // Animate to Red, then back to the original color.
       .OnComplete(success => 
       {
           if (success) // Check if the tween completed normally (was not cancelled).
               EnterPhaseTwoAI();
       });
}
```

#### 3. Looping with Inter-Loop Delays

**Goal:** Create a floating ghost enemy that hovers up and down, pausing at the apex of its movement. This is best achieved by tweening a custom offset field on your `ModNPC`.

```csharp
// In your ModNPC class:
private float _verticalOffset = 0f;
private Vector2 _spawnPosition;

public override void SetDefaults()
{
    // Animate our custom field.
    this.TweenTo(n => n._verticalOffset, -20f, 90)
        .SetEase(Easing.SineInOut)
        .SetLoops(-1, LoopType.Yoyo) // Loop forever, back and forth.
        .SetLoopDelay(45);           // Wait 0.75 seconds after each movement.
}

public override void AI()
{
    // Apply the tweened offset to the NPC's actual position.
    NPC.position.Y = _spawnPosition.Y + _verticalOffset;
    // ... other AI logic ...
}
```

#### 4. Dynamic Targets (Homing)

**Goal:** A wisp-like projectile that continuously seeks the NPC's designated target player.

```csharp
// In a ModProjectile's AI() hook.
public override void AI()
{
    if (NPC.target < 0 || NPC.target >= Main.maxPlayers) return;
    Player target = Main.player[NPC.target];
    
    // The target value is a lambda, re-evaluated every frame.
    Projectile.TweenTo(p => p.velocity, () => (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 8f, 30);
}
```

#### 5. Group Management via Tags

**Goal:** A boss summons several orbiting crystals. When the boss is defeated, all crystals should stop moving.

```csharp
// In the boss AI, when summoning crystals:
for (int i = 0; i < 5; i++)
{
    Projectile crystal = Projectile.NewProjectileDirect(...);
    // Call the extension method ON the 'crystal' instance.
    crystal.TweenTo(p => p.rotation, MathHelper.TwoPi, 180)
           .SetLoops(-1, LoopType.Restart)
           .SetTag("BossOrbitals");
}

// In the boss's OnKill() hook:
public override void OnKill()
{
    // This single line stops all rotation animations on all crystals.
    TweenSystem.CancelAllByTag("BossOrbitals");
    // ... code to make them explode ...
}
```

#### 6. Complex Animation with Sequences

**Goal:** A boss's "power-up" animation: it rises into the air, glows intensely, then slams down, creating a shockwave.

```csharp
// In a ModNPC method that triggers a special attack.
public void DoSlamAttack()
{
    var sequence = TweenSystem.CreateSequence();
    Vector2 startCenter = NPC.Center;

    sequence
        // Step 1: Rise up into the air smoothly.
        .Append(NPC.CreateTween(n => n.Center, startCenter - new Vector2(0, 200f), 60).SetEase(Easing.CubicOut))
        
        // Step 2: While hovering, pulse with a bright color.
        .Append(NPC.CreateTween(n => n.color, Color.Yellow, 20).SetLoops(3, LoopType.Yoyo))
        
        // Step 3: Slam back down to the ground quickly.
        .Append(NPC.CreateTween(n => n.Center, startCenter, 15).SetEase(Easing.ExpoIn))

        // Step 4: After the slam, create a shockwave projectile.
        .OnComplete(success => {
            if (success) 
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom, Vector2.Zero, ModContent.ProjectileType<Shockwave>(), 0, 0);
        })
        
        // Start the entire choreographed animation.
        .Play();
}
```
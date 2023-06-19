# Unity Networked AI Preview

A smaller project to showcase some basic networked AI behaviour
* Movement done through NavMesh
* Different AI States (Idle, wander, follow, attack)
* Basic animation script
* Basic health script
* Ragdoll or death animation
* Auto spawner
* Detects nearby threats
* * Calculates which threat is the most intelligent one to attack based on distance
# What makes this different from the non-networked AI:
* Everyone who are connected will be able to see synchronized movement and animation between clients.
* * Have in mind that ragdolls are left clientsided as they are expensive to network, it can be toggled on and off so they will trigger a death animation instead.
* Zombies have quicker reactions
* Bots no longer idle for a few seconds on start, instead they will trigger their first action faster
* Zombies have different values from the player, they have a wider detection range, these changes are overridden in BotZombie
* Debugging can be enabled and disabled on the components
* Targets are taunted incase they have no target

# 12 Sentiments for VR

[12 Sentiments for VR](https://ccrma.stanford.edu/~lja/vr/12SentimentsForVR/) is a long-form VR narrative that gives its user interactive 
control over musical/environmental interaction in each of its twelve movements. Each movement sources everything from its interaction design
to its chord changes to its color palette from a central emotional aesthetic. Approaching the interaction design of virtual musical environments
without relying on ideas of musical ``instruments'' allowed me to explore [values](https://ccrma.stanford.edu/~lja/doing-vs-being/) for the design 
of virtual reality experiences more broadly. Of these, 12 Sentiments best illustrates **doing vs. being**, the balance between purposive action and 
intentional stillness and calm.

## Running the Project

The experience proceeds linearly through 12 unity scenes, starting with [1_TimidExplorationExhilaration.unity](Assets/Scenes/1_TimidExplorationExhilaration.unity).
When some certain narrative condition is met, such as collecting enough sunlight with leaf-hands in [2_ExcitementLonging.unity](Assets/Scenes/2_ExcitementLonging.unity),
the project will automatically, fluidly advance to the next scene.
The project can also be started from any one of the individual scenes, should you wish to skip ahead.

This project was created before the creation of [SteamVR 2.0](https://sarthakghosh.medium.com/a-complete-guide-to-the-steamvr-2-0-input-system-in-unity-380e3b1b3311),
so unfortunately, it is hardcoded to accept inputs from an HTC Vive only.

## Underlying Scripts

Each scene is driven by its underlying music, and so a key part of understanding how any one scene works is the [sounds](Assets/Sound) folder for that scene.
Common scripts for interpreting controller inputs, basic animation, etc. are in [UtilityScripts](Assets/UtilityScripts).

Ordinarily, most Unity projects organize scripts, models, materials, and other assets into separate folders. In this project, the virtual objects 
and their behaviors that can't be abstracted to a more general script are instead organized by [individual movement](Assets/Objects). 
[Shared objects](Assets/Objects/Common) such as seedlings, vines, and terrain data are located separately.

You may be interested in:
- Vine inverse kinematics for [endpoints](Assets/Objects/Common/Vine/VineEndJointController.cs) and [midpoints](Assets/Objects/Common/Vine/VineMidpointController.cs).
- Glow effects and haptic feedback for [leaf hands](Assets/Objects/Scene2/SunbeamInteractors.cs) 
- A [supersaw](Assets/Sound/Scene2/LookChords.cs) synthesizer implemented in ChucK
- A [granular synth vocal chorus](Assets/Sound/Scene6/Scene6AhhChords.cs) synthesizer implemented in ChucK
- Physics engine code for showing the effect of [wind](Assets/Objects/Common/Seedling/ApplyWindToSeedlings.cs) on virtual seedlings
- A [slew follower](Assets/UtilityScripts/SlewFollower.cs) that makes an object (such as a seedling) gradually approach a goal position, but wiggling slightly on the way there
- An interaction that [slows time down](Assets/Objects/Scene9/SlowTimeWhenHandsMove.cs) after a hand gesture to dramaticize the resulting physics simulation

These of course only scratch the surface of the project!

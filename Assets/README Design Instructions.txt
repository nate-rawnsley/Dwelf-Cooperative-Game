-----How every object is set up-----

--Player--
The only thing the player needs in addition is to drag the pause menu from the scene into their character script (where prompted).

--Pause Menu--
Drag both characters to their respective boxes in the 'Control Settings' component

--DOOR--
This applies to any object that's moveable by switches or pressure pads, eg. doors and ladders.
1. Add the 'Door' script to the object.
The fields you can edit are these:
-Inputs to open -- How many buttons must be pressed/held to move this.
-Direction -- Which direction it will move.
-Type -- Whether it is toggled (switches) or held (pressure pads) to be moved.
-Distance Multiplier -- How far it will go (n * height/width). Default is 2.
-Speed -- How far it will move in one second (in percent). Default is 0.2.

If you want a number on the door displaying how many are needed to open it:
1. Add the 'Canvas' component to the object.
2. Create a 'Text - TextMeshPro' object as a child of the door object.
(I recommend setting the font size to abut 1.2 but whatever looks best.)
This is already set up on the prefab so don't worry if you're using that.

--PRESSURE PAD--
1. Find 'Pressure Pad (Script) in components.
2. Under the sprites (which should be filled in on the prefab) are two Unity events: 'On Press ()' and 'On Release ()'.
3. Press the + under 'On Press' to add a door it opens on press.
4. Drag the door you want to open (from the heirarchy on the left) to the field under 'Runtime Only'.
5. On the dropdown on the right navigate to 'Door' and select 'Open (bool)'.
6. Press the checkbox underneath (this says to open the door when the player presses it).
7. Repeat from step 3 for 'On Release', but leave the checkbox unchecked (which tells the door to close on release).

--SWITCH--
1. Add the 'Button' script to an object for it to be a switch.
2. This script has only one Unity event (On Press), so do steps 3-5 of Pressure Pad.
Whether the checkbox is ticked is irrelevant.
3. Also, in the inspector on the right change the layer to 'Button' (in the top right).
(This will probably be replaced by levers in the end)

--Ladder--
Any object can be a ladder, it is defined by the gameobject's tag and layer (at the top of the inspector).
1. Set the tag to 'Ladder'.
2. Set the layer to 'Interactable'.
If you want to make the ladder taller or shorter, DON'T use scale.
Change the 'height' value in both the Sprite Renderer and Box Collider 2D components (make sure they roughly match).
As mentioned, if you want it to be moveable by button, add the 'Door' script as well.

--TUNNEL--
1. Add the 'Tunnel' script to both tunnels.
2. Drag the destination tunnel from the heirarchy into the 'Destination' field.
(Or you can press the arrow and manually select it from a list.)
3. Set the tag to 'Tunnel' and layer to 'Interactable'.
Make sure both tunnels have their destination set up.
(You can set any object as a destination but idk if we'll be using that practically)


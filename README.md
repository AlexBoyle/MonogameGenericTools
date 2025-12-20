# MonogameGenericTools
This is a collection of tools I have built for myself to simplify some aspects of building a project on top of Monogame.  
These tools are intended to drive the project  
  
## High Level Overview
This project is run via `Utils/Core/Game.cs`  
This file will:  
 -  Set up all the classes in `Utils/Core/GlobalUtilities`
 -  Track the average run time of the `update` and `draw` function
 -  Utilize the `SceneUtility` to render and update scenes
  
  ## Scenes
Any class that inherits `SceneBase` will be picked up at launch by the `SceneUtility` in its `intialize` function.  
Note that this will create an instance of every `SceneBase` class. Because of this I suggest there be minimal  logic in the constructor for a scene.  

When a scene is set to active  
Either with `this.isInitalScene = true;` being set in the constructor or `SceneUtility.setActive(sceneName)` being called  
The function `setup` on the scene will be called (if it has not been already, specified by `isSetup`)
